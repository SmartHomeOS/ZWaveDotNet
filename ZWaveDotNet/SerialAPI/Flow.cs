using Serilog;
using System.Threading.Channels;
using ZWaveDotNet.SerialAPI.Messages;

namespace ZWaveDotNet.SerialAPI
{
    public class Flow
    {
        Port port;
        Channel<Frame> Unsolicited;

        public Flow(string portName)
        {
            this.port = new Port(portName);
            Unsolicited = CreateReader();
            Task.Factory.StartNew(MultiplexReader);
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
            var reader = CreateReader();
            try
            {
                await SendAcknowledgedIntl(reader, frame);
            }
            finally
            {
                DisposeReader(reader);
            }
        }
        private async Task SendAcknowledgedIntl(Channel<Frame> reader, Frame frame)
        {
            CancellationTokenSource cts = new CancellationTokenSource(1500);
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
            var reader = CreateReader();
            await SendAcknowledgedIntl(reader, frame);
            try
            {
               return await SendAcknowledgedResponseIntl(cts.Token, frame, reader);
            }
            finally 
            { 
                DisposeReader(reader); 
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
            var reader = CreateReader();
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
                DisposeReader(reader);
            }
        }

        public async Task<Message> GetUnsolicited()
        {
            return GetMessage(await Unsolicited.Reader.ReadAsync());
        }

        private async Task<bool> SuccessfulAck(Channel<Frame> reader, CancellationToken token)
        {
            Frame f = await reader.Reader.ReadAsync(token);
            return f.Type == FrameType.ACK;
        }

        private List<Channel<Frame>> channels = new List<Channel<Frame>>();
        private Channel<Frame> CreateReader()
        {
            Channel<Frame> reader = Channel.CreateUnbounded<Frame>();
            lock (channels)
                channels.Add(reader);
            return reader;
        }

        private void DisposeReader(Channel<Frame> reader)
        {
            lock (channels)
                channels.Remove(reader);
        }

        private async Task MultiplexReader()
        {
            while (true)
            {
                try
                {
                    Frame frame = await port.ReadFrameAsync();
                    lock (channels)
                        channels.ForEach(channel => channel.Writer.TryWrite(frame));
                }catch(Exception ex)
                {
                    Log.Error(ex, "Ooops");
                }
            }
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
