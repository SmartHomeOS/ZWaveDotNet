using System.Threading.Channels;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;
using ZWaveDotNet.SerialAPI.Messages.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public class Flow
    {
        private Port port;
        private Channel<Frame> unsolicited;

        public Flow(string portName)
        {
            this.port = new Port(portName);
            unsolicited = port.CreateReader();
        }

        public async Task SendUnacknowledged(Function function, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            await port.QueueTX(frame);
        }

        public Task SendAcknowledged(Function function, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            return SendAcknowledged(frame);
        }

        public Task SendAcknowledged(Message message, CancellationToken cancellationToken = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            return SendAcknowledged(frame);
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
            return GetMessage(await SendAcknowledgedResponse(frame, cancellationToken));
        }

        public async Task<Message> SendAcknowledgedResponse(Message message, CancellationToken cancellationToken = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            Frame response = await SendAcknowledgedResponse(frame, cancellationToken);
            return GetMessage(response)!;
        }

        public async Task<Frame> SendAcknowledgedResponse(Frame frame, CancellationToken cancellationToken = default)
        {
            var reader = port.CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame, cancellationToken);
                return await SendAcknowledgedResponseIntl(frame, reader, cancellationToken);
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
                return await SendAcknowledgedResponseCallbackIntl(reader, frame, message.SessionID, token);
            }
            finally {
                port.DisposeReader(reader);
            }
        }

        public async Task<ReportMessage> SendReceiveSequence(DataMessage message, CommandClass ResponseClass, byte ResponseCommand, CancellationToken cancellationToken = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            var reader = port.CreateReader();
            try
            {
                DataCallback dc = await SendAcknowledgedResponseCallbackIntl(reader, frame, message.SessionID, cancellationToken);
                if (dc.Status != TransmissionStatus.CompleteOk)
                    throw new Exception("Transmission Failure " + dc.Status.ToString());
                while (!cancellationToken.IsCancellationRequested)
                {
                    Frame response = await reader.Reader.ReadAsync(cancellationToken);
                    if (response.DataType == DataFrameType.Request)
                    {
                        Message msg = GetMessage(response)!;
                        if (msg is ApplicationCommand ac && ac.SourceNodeID == message.DestinationNodeID)
                        {
                            ReportMessage rm = new ReportMessage(ac);
                            if (rm.CommandClass == ResponseClass && rm.Command == ResponseCommand)
                                return rm;
                        }
                    }
                }
                throw new TimeoutException("Response not received");
            }
            finally
            {
                port.DisposeReader(reader);
            }
        }

        public async Task<Message> GetUnsolicited()
        {
            return GetMessage(await unsolicited.Reader.ReadAsync());
        }

        private async Task<DataCallback> SendAcknowledgedResponseCallbackIntl(Channel<Frame> reader, Frame frame, byte sessionId, CancellationToken token = default)
        {
            await SendAcknowledgedIntl(reader, frame, token);
            Frame status = await SendAcknowledgedResponseIntl(frame, reader, token);
            if (!new Response(status.Payload, status.CommandID).Success)
                throw new Exception("Failed to transmit command");
            while (!token.IsCancellationRequested)
            {
                Frame response = await reader.Reader.ReadAsync(token);
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
            CancellationTokenSource timeout = new CancellationTokenSource(1600);
            CancellationToken token = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken).Token;
            do
            {
                await port.QueueTX(frame);
            } while (!await SuccessfulAck(reader, token));
        }

        private async Task<Frame> SendAcknowledgedResponseIntl(Frame frame, Channel<Frame> reader, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Frame response = await reader.Reader.ReadAsync(token);
                if (response.DataType == DataFrameType.Response)
                    return response;
            }
            throw new TimeoutException("Response not received");
        }

        private async Task<bool> SuccessfulAck(Channel<Frame> reader, CancellationToken token)
        {
            Frame f;
            do
            {
                f = await reader.Reader.ReadAsync(token);
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
                        return new ApplicationCommand(frame.Payload, frame.CommandID);
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
                    case Function.AddNodeToNetwork:
                    case Function.RemoveNodeFromNetwork:
                        return new InclusionStatus(frame.Payload, frame.CommandID);
                }
            }
            return new PayloadMessage(frame.Payload, frame.CommandID);
        }
    }
}
