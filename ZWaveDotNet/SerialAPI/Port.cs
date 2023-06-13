using Serilog;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading.Channels;
using ZWaveDotNet.SerialAPI.Enums;

namespace ZWaveDotNet.SerialAPI
{
    public class Port
    {
        private readonly SerialPort port;
        private readonly Channel<Frame> tx = Channel.CreateUnbounded<Frame>();
        private readonly ConcurrentDictionary<Channel<Frame>, byte> rxChannels = new ConcurrentDictionary<Channel<Frame>, byte>();

        public Port(string path) 
        {
            port = new SerialPort(path, 115200, Parity.None, 8, StopBits.One);
            port.Open();
            Reset();

            Task.Factory.StartNew(WriteTask);
            Task.Factory.StartNew(ReadTask);
        }

        public async ValueTask QueueTX(Frame frame)
        {
            await tx.Writer.WriteAsync(frame);
        }

        public void Reset()
        {
            port.DiscardInBuffer();
            port.DiscardOutBuffer();
        }

        public Channel<Frame> CreateReader()
        {
            Channel<Frame> reader = Channel.CreateUnbounded<Frame>();
            rxChannels.TryAdd(reader, 0);
            return reader;
        }

        public bool DisposeReader(Channel<Frame> reader)
        {
            return rxChannels.Remove(reader, out _);
        }

        private async Task WriteTask()
        {
            try
            {
                while (await tx.Reader.WaitToReadAsync())
                {
                    var frame = await tx.Reader.ReadAsync();
                    Log.Debug("Wrote " + frame.ToString());
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
                        Log.Debug("Invalid Frame");
                    }
                    else
                    {
                        Log.Debug("Read " + frame.ToString());
                        if (frame.Type == FrameType.SOF)
                            await QueueTX(Frame.ACK);
                        foreach (Channel<Frame> channel in rxChannels.Keys)
                            channel.Writer.TryWrite(frame);
                    }
                } while (port.IsOpen);
                Log.Error("Port is closed");
            }
            catch (IOException io)
            {
                Log.Error(io, "Failed to read from port");
            }
        }
    }
}
