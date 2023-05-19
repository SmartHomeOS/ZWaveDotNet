using Serilog;
using System.IO.Ports;
using System.Threading.Channels;

namespace ZWaveDotNet.SerialAPI
{
    public class Port
    {
        private SerialPort port;
        private readonly Channel<Frame> tx = Channel.CreateUnbounded<Frame>();
        private readonly Channel<Frame> rx = Channel.CreateUnbounded<Frame>();

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

        public ValueTask<Frame> ReadFrameAsync(CancellationToken token = default)
        {
            return rx.Reader.ReadAsync(token);
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
                        await QueueTX(Frame.NAK);
                        Log.Information("Read Invalid Data");
                    }
                    else
                    {
                        Log.Information("Read " + frame.ToString());
                        if (frame.Type == FrameType.SOF)
                            await QueueTX(Frame.ACK);
                        await rx.Writer.WriteAsync(frame);
                    }
                } while (frame != null);
            }
            catch (IOException io)
            {
                Log.Error(io, "Failed to read from port");
            }
        }
    }
}
