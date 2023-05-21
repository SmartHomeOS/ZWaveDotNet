using System.Threading.Channels;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.SerialAPI.Messages;

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

        public Task SendUnacknowledged(Function function, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            return SendUnacknowledged(frame);
        }

        public Task SendUnacknowledged(Message message)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            return SendUnacknowledged(frame);
        }

        public async Task SendUnacknowledged(Frame frame)
        {
            await port.QueueTX(frame);
        }

        public Task SendAcknowledged(Function function, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            return SendAcknowledged(frame);
        }

        public Task SendAcknowledged(Message message)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            return SendAcknowledged(frame);
        }

        public async Task SendAcknowledged(Frame frame)
        {
            var reader = port.CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame);
            }
            finally
            {
                port.DisposeReader(reader);
            }
        }
        private async Task SendAcknowledgedIntl(Channel<Frame> reader, Frame frame)
        {
            CancellationTokenSource cts = new CancellationTokenSource(1600);
            do
            {
                await port.QueueTX(frame);
            } while (!await SuccessfulAck(reader, cts.Token));
        }

        public async Task<Message> SendAcknowledgedResponse(Function function, params byte[] payload)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, function, payload);
            return GetMessage(await SendAcknowledgedResponse(frame));
        }

        public async Task<Message> SendAcknowledgedResponse(Message message)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            Frame response = await SendAcknowledgedResponse(frame);
            return GetMessage(response)!;
        }

        public async Task<Frame> SendAcknowledgedResponse(Frame frame)
        {
            CancellationTokenSource cts = new CancellationTokenSource(1500);
            var reader = port.CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame);
                return await SendAcknowledgedResponseIntl(cts.Token, frame, reader);
            }
            finally 
            {
                port.DisposeReader(reader); 
            }
        }

        private async Task<Frame> SendAcknowledgedResponseIntl(CancellationToken token, Frame frame, Channel<Frame> reader)
        {
            while (!token.IsCancellationRequested)
            {
                Frame response = await reader.Reader.ReadAsync(token);
                if (response != null && response.Type == FrameType.SOF && response.DataType == DataFrameType.Response)
                    return response;
            }
            throw new TimeoutException("Response not received");
        }

        public async Task<Message> SendAcknowledgedResponseCallback(DataMessage message, CancellationToken token = default)
        {
            Frame frame = new Frame(FrameType.SOF, DataFrameType.Request, message.Function, message.GetPayload());
            var reader = port.CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame);
                Frame status = await SendAcknowledgedResponseIntl(token, frame, reader);
                if (!new Response(status.Payload, status.CommandID).Success)
                    throw new Exception("Failed to transmit command");
                while (!token.IsCancellationRequested)
                {
                    Frame response = await reader.Reader.ReadAsync(token);
                    if (response != null && response.Type == FrameType.SOF && response.DataType == DataFrameType.Request)
                    {
                        Message msg = GetMessage(response)!;
                        if (msg is DataCallback dc && dc.SessionID == message.SessionID)
                            return dc;
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
                            return new Response(frame.Payload, frame.CommandID); //This is new
                        else
                            return ApplicationUpdate.From(frame.Payload);
                    case Function.SerialAPIStarted:
                        return new APIStarted(frame.Payload);
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
