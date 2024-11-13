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

using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClasses.Enums;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    /// <summary>
    /// The Barrier Operator Command Class is used to control and query the status of motorized barriers.
    /// </summary>
    [CCVersion(CommandClass.BarrierOperator, 1)]
    public class BarrierOperator : CommandClassBase
    {
        public event CommandClassEvent<BarrierReport>? BarrierState;
        
        enum BarrierOperatorCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            SupportedGet = 0x04,
            SupportedReport = 0x05,
            SignalSet = 0x06,
            SignalGet = 0x07,
            SignalReport = 0x08
        }

        public BarrierOperator(Node node, byte endpoint) : base(node, endpoint, CommandClass.BarrierOperator) { }

        /// <summary>
        /// Request the current state of a barrier operator device
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        public async Task<BarrierReport> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(BarrierOperatorCommand.Get, BarrierOperatorCommand.Report, cancellationToken);
            return new BarrierReport(response.Payload.Span);
        }

        /// <summary>
        /// Initiate an unattended change in state of the barrier
        /// </summary>
        /// <param name="open"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Set(bool open, CancellationToken cancellationToken = default)
        {
            await SendCommand(BarrierOperatorCommand.Set, cancellationToken, open ? (byte)0xFF : (byte)0x00);
        }

        /// <summary>
        /// Query a device for available subsystems which may be controlled via Z-Wave
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BarrierSignal[]> GetSupportedSignals(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(BarrierOperatorCommand.SupportedGet, BarrierOperatorCommand.SupportedReport, cancellationToken);
            List<BarrierSignal> supportedTypes = new List<BarrierSignal>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    supportedTypes.Add((BarrierSignal)(i));
            }
            return supportedTypes.ToArray();
        }

        /// <summary>
        /// Turn on or off an event signaling subsystem that is supported by the device
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="active"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SignalSet(BarrierSignal signal, bool active, CancellationToken cancellationToken = default)
        {
            await SendCommand(BarrierOperatorCommand.Set, cancellationToken, (byte)signal, active ? (byte)0xFF : (byte)0x00);
        }

        /// <summary>
        /// Request the state of a signaling subsystem to a supporting node
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="MethodAccessException"></exception>
        /// <exception cref="DataException"></exception>
        public async Task<KeyValuePair<BarrierSignal, bool>> SignalGet(BarrierSignal signal, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(BarrierOperatorCommand.Get, BarrierOperatorCommand.Report, cancellationToken, (byte)signal);
            if (response.Payload.Length < 2)
                throw new DataException($"The Barrier Signal Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            return KeyValuePair.Create((BarrierSignal)response.Payload.Span[0], response.Payload.Span[1] != 0x0);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)BarrierOperatorCommand.Report)
            {
                BarrierReport rpt = new BarrierReport(message.Payload.Span);
                await FireEvent(BarrierState, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
