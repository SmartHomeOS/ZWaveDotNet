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

using System.IO.Ports;
using ZWaveDotNet.SerialAPI.Enums;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.SerialAPI
{
    public class Frame
    {
        public static readonly Frame ACK = new Frame(FrameType.ACK);
        public static readonly Frame NAK = new Frame(FrameType.NAK);
        public static readonly Frame CAN = new Frame(FrameType.CAN);
        public static volatile bool reading;

        public readonly FrameType Type;
        public readonly DataFrameType DataType;
        public readonly Function CommandID;
        public readonly Memory<byte> Payload;

        public Frame() { }

        public Frame (FrameType type, DataFrameType dataType = DataFrameType.Other, Function CommandID = Function.None, PayloadWriter? payload = null)
        {
            if (payload == null)
                this.Payload = Memory<byte>.Empty;
            else
                this.Payload = payload.GetPayload();
            this.Type = type;
            this.DataType = dataType;
            this.CommandID = CommandID;
        }

        public Frame(FrameType type, DataFrameType dataType, Function CommandID, Memory<byte> payload)
        {
            this.Payload = payload;
            this.Type = type;
            this.DataType = dataType;
            this.CommandID = CommandID;
        }

        public static async Task<Frame?> Read(Stream stream)
        {
            Memory<byte> buff = new byte[512];
            FrameType frame;
            do
            {
                reading = false;
                if (await stream.ReadAsync(buff.Slice(0, 1)) == 0)
                    throw new EndOfStreamException();
                reading = true;
                frame = (FrameType)buff.Span[0];
                if (frame == FrameType.ACK)
                    return ACK;
                else if (frame == FrameType.NAK)
                    return NAK;
                else if (frame == FrameType.CAN)
                    return CAN;
            } while (frame != FrameType.SOF);

            if (frame == FrameType.SOF)
            {
                try
                {
                    CancellationTokenSource cts = new CancellationTokenSource(1500);
                    if (await stream.ReadAsync(buff.Slice(1, 1), cts.Token) == 0)
                        throw new EndOfStreamException();
                    byte len = buff.Span[1];
                    int total = 0;
                    do
                    {
                        int read = await stream.ReadAsync(buff.Slice(2 + total, len), cts.Token);
                        if (read == 0)
                            throw new EndOfStreamException();
                        total += read;
                    }
                    while (total < len);
                    reading = false;
                    if (!ValidateChecksum(buff))
                        return null;
                    return new Frame(frame, (DataFrameType)buff.Span[2], (Function)buff.Span[3], buff.Slice(4, len - 3));
                }
                catch (OperationCanceledException) { }
            }
            return null;
        }

        private static bool ValidateChecksum(Memory<byte> buff)
        {
            byte len = buff.Span[1];
            return (buff.Span[len + 1] == MemoryUtil.XOR(buff.Slice(1, len), 0xFF));
        }

        private Memory<byte> GetPayload()
        {
            Memory<byte> ret = new byte[Payload.Length + 5];
            ret.Span[0] = (byte)FrameType.SOF;
            ret.Span[1] = (byte)(Payload.Length + 3);
            ret.Span[2] = (byte)DataType;
            ret.Span[3] = (byte)CommandID;
            Payload.CopyTo(ret.Slice(4));
            //Add checksum 
            ret.Span[ret.Length - 1] = MemoryUtil.XOR(ret.Slice(1, ret.Length - 2), 0xFF);
            return ret;
        }

        public async Task WriteBytes(SerialPort port, CancellationToken cancellationToken = default)
        {
            if (Type == FrameType.SOF)
            {
                Memory<byte> payload = GetPayload();
                while (reading || port.BytesToRead != 0)
                    await Task.Delay(1, cancellationToken);
                await port.BaseStream.WriteAsync(payload.ToArray(), 0, payload.Length, cancellationToken);
            }
            else
                port.BaseStream.WriteByte((byte)Type);
        }

        public override string ToString()
        {
            if (Type != FrameType.SOF)
                return Type.ToString();
            return $"{CommandID}: {BitConverter.ToString(Payload.ToArray())}";
        }
    }
}
