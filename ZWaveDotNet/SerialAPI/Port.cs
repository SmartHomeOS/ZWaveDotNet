using Serilog;
using System.IO.Ports;
using System.Threading.Channels;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public class Port
    {
        private SerialPort port;
        private readonly Channel<Frame> tx = Channel.CreateUnbounded<Frame>();
        private List<Channel<Frame>> rXChannels = new List<Channel<Frame>>();

        public Port(string path) 
        {
            port = new SerialPort(path, 115200, Parity.None, 8, StopBits.One);
            port.Open();
            Reset();

            Task.Factory.StartNew(WriteTask);
            Task.Factory.StartNew(ReadTask);
        }

        public ValueTask QueueTX(Frame frame)
        {
            return tx.Writer.WriteAsync(frame);
        }

        public void Reset()
        {
            //TODO - Flush Channels
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
        }

        public Channel<Frame> CreateReader()
        {
            Channel<Frame> reader = Channel.CreateUnbounded<Frame>();
            lock (rXChannels)
                rXChannels.Add(reader);
            return reader;
        }

        public bool DisposeReader(Channel<Frame> reader)
        {
            lock (rXChannels)
                return rXChannels.Remove(reader);
        }

        private async Task WriteTask()
        {
            try
            {
                while (await tx.Reader.WaitToReadAsync())
                {
                    var frame = await tx.Reader.ReadAsync();
                    Log.Information("Wrote " + frame.ToString());
                    await frame.WriteBytes(port.BaseStream);
                }
            }
            catch (IOException io)
            {
                Log.Error(io, "Failed to write to port");
            }
        }

        private async Task ReadTask()
        {
            try
            {
                Frame? frame;
                do
                {
                    frame = await Frame.Read(port.BaseStream);
                    if (frame == null)
                    {
                        port.DiscardInBuffer();
                        await QueueTX(Frame.NAK);
                        Log.Information("Invalid Frame");
                    }
                    else
                    {
                        Log.Information("Read " + frame.ToString());
                        if (frame.Type == FrameType.SOF)
                            await QueueTX(Frame.ACK);
                        lock (rXChannels)
                            rXChannels.ForEach(channel => channel.Writer.TryWrite(frame));
                    }
                } while (port.IsOpen);
            }
            catch (IOException io)
            {
                Log.Error(io, "Failed to read from port");
            }
        }
    }
}
