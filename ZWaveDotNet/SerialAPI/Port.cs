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

            _ = Task.Factory.StartNew(WriteTask, TaskCreationOptions.LongRunning);
            _ = Task.Factory.StartNew(ReadTask, TaskCreationOptions.LongRunning);
        }

        public bool IsConnected()
        {
            return port.IsOpen;
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
                        await frame.WriteBytes(port);
                        Log.Verbose($"Wrote " + frame.ToString());
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
                        if (frame.Type == FrameType.CAN)
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
