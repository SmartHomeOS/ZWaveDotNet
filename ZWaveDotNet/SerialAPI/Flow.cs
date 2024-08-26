// ZWaveDotNet Copyright (C) 2024 
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Serilog;
using System.Threading.Channels;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.SerialAPI
{
    public class Flow
    {
        private readonly Port port;
        private readonly Channel<Frame> unsolicited;
        private readonly SemaphoreSlim portLock = new SemaphoreSlim(1, 1);
        public bool WideID { get; set; }
        internal bool IsConnected { get { return port.IsConnected(); } }

        public Flow(string portName)
        {
            this.port = new Port(portName);
            unsolicited = port.CreateReader();
        }

        public async Task SendUnacknowledged(Function function, params byte[] payload)
        {
            try
            {
                await portLock.WaitAsync();
                Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
                await port.QueueTX(frame);
            }
            finally
            {
                portLock.Release();
            }
        }

        public async Task SendAcknowledged(Function function, CancellationToken cancellationToken = default, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            await SendAcknowledged(frame, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAcknowledged(Message message, CancellationToken cancellationToken = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            await SendAcknowledged(frame, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAcknowledged(Frame frame, CancellationToken cancellationToken = default)
        {
            var reader = port.CreateReader();
            try {
                await SendAcknowledgedIntl(reader, frame, cancellationToken);
            }
            finally {
                port.DisposeReader(reader);
            }
        }

        public async Task<Message> SendAcknowledgedResponse(Function function, CancellationToken cancellationToken = default, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            Frame response = await SendAcknowledgedResponse(frame, cancellationToken).ConfigureAwait(false);
            return GetMessage(response);
        }

        public async Task<Message> SendAcknowledgedResponse(Message message, CancellationToken cancellationToken = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            Frame response = await SendAcknowledgedResponse(frame, cancellationToken).ConfigureAwait(false);
            return GetMessage(response)!;
        }

        public async Task<Frame> SendAcknowledgedResponse(Frame frame, CancellationToken cancellationToken = default)
        {
            var reader = port.CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame, cancellationToken);
                return await GetAcknowledgedResponseIntl(reader, cancellationToken).ConfigureAwait(false);
            }
            finally  {
                port.DisposeReader(reader); 
            }
        }

        public async Task<DataCallback> SendAcknowledgedResponseCallback(DataMessage message, CancellationToken token = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            var reader = port.CreateReader();
            try {
                return await SendAcknowledgedResponseCallbackIntl(reader, frame, message.SessionID, token).ConfigureAwait(false);
            }
            finally {
                port.DisposeReader(reader);
            }
        }

        public async Task<Message> GetUnsolicited()
        {
            return GetMessage(await unsolicited.Reader.ReadAsync().ConfigureAwait(false));
        }

        private async Task<DataCallback> SendAcknowledgedResponseCallbackIntl(Channel<Frame> reader, Frame frame, byte sessionId, CancellationToken token = default)
        {
            await SendAcknowledgedIntl(reader, frame, token);
            Frame status = await GetAcknowledgedResponseIntl(reader, token).ConfigureAwait(false);
            if (!new Response(status.Payload, status.CommandID).Success)
                throw new Exception("Failed to transmit command");
            while (!token.IsCancellationRequested)
            {
                Frame response = await reader.Reader.ReadAsync(token).ConfigureAwait(false);
                if (response.DataType == DataFrameType.Request)
                {
                    Message msg = GetMessage(response)!;
                    if (msg is DataCallback dc && dc.SessionID == sessionId)
                        return dc;
                }
            }
            throw new TimeoutException("Callback not received");
        }

        private async Task SendAcknowledgedIntl(Channel<Frame> reader, Frame frame, CancellationToken cancellationToken)
        {
            try
            {
                await portLock.WaitAsync(cancellationToken);
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    using (CancellationTokenSource timeout = new CancellationTokenSource(1600))
                    using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken))
                    {
                        await port.QueueTX(frame);
                        if (await SuccessfulAck(reader, cts.Token))
                            break;
                    }
                    Log.Warning($"Retransmit Attempt {attempt + 1}");
                    await Task.Delay(100 + (1000 * attempt), cancellationToken);
                }
            }
            finally
            {
                portLock.Release();
            }
        }

        private static async Task<Frame> GetAcknowledgedResponseIntl(Channel<Frame> reader, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Frame response = await reader.Reader.ReadAsync(token);
                if (response.DataType == DataFrameType.Response)
                    return response;
            }
            throw new TimeoutException("Response not received");
        }

        private static async Task<bool> SuccessfulAck(Channel<Frame> reader, CancellationToken token)
        {
            Frame f;
            do
            {
                f = await reader.Reader.ReadAsync(token).ConfigureAwait(false);
            } while (f.Type == FrameType.SOF);
            return f.Type == FrameType.ACK;
        }

        private Message GetMessage(Frame frame)
        {
            if (frame.Type == FrameType.SOF)
            {
                switch (frame.CommandID)
                {
                    case Function.ApplicationCommand:
                    case Function.ApplicationCommandHandlerBridge:
                        return new ApplicationCommand(frame.Payload, frame.CommandID, WideID);
                    case Function.ApplicationUpdate:
                        if (frame.DataType == DataFrameType.Response)
                            return new Response(frame.Payload, frame.CommandID);
                        else
                            return ApplicationUpdate.From(frame.Payload);
                    case Function.SerialAPIStarted:
                        return new APIStarted(frame.Payload);
                    case Function.GetNodeProtocolInfo:
                        return new NodeProtocolInfo(frame.Payload);
                    case Function.SendData:
                    case Function.SendDataMulticast:
                    case Function.SendDataBridge:
                    case Function.SendDataBridgeMulticast:
                    case Function.SendDataEndNode:
                    case Function.SendDataEndNodeMulticast:
                    case Function.RequestNodeInfo:
                        if (frame.DataType == DataFrameType.Response)
                            return new Response(frame.Payload, frame.CommandID);
                        else
                            return new DataCallback(frame.Payload, frame.CommandID);
                    case Function.GetSerialAPIInitData:
                        return new InitData(frame.Payload);
                    case Function.GetLRNodes:
                        return new LongRangeNodes(frame.Payload);
                    case Function.AddNodeToNetwork:
                    case Function.RemoveNodeFromNetwork:
                        return new InclusionStatus(frame.Payload, frame.CommandID);
                }
            }
            return new PayloadMessage(frame.Payload, frame.CommandID);
        }
    }
}
