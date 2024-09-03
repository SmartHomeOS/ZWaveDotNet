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
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.WakeUp, 1, 3)]
    public class WakeUp : CommandClassBase
    {
        private readonly ConcurrentQueue<TaskCompletionSource> taskCompletionSources = new ConcurrentQueue<TaskCompletionSource>();
        public event CommandClassEvent<ReportMessage>? Awake;

        enum WakeUpCommand
        {
            IntervalSet = 0x04,
            IntervalGet = 0x05,
            IntervalReport = 0x06,
            Notification = 0x07,
            NoMoreInformation = 0x08,
            IntervalCapabilitiesGet = 0x09,
            IntervalCapabilitiesReport = 0x0A
        }

        public WakeUp(Node node, byte endpoint) : base(node, endpoint, CommandClass.WakeUp) { }

        public async Task<WakeUpIntervalReport> GetInterval(CancellationToken cancellationToken = default)
        {
            ReportMessage message = await SendReceive(WakeUpCommand.IntervalGet, WakeUpCommand.IntervalReport, cancellationToken);
            return new WakeUpIntervalReport(message.Payload);
        }

        public async Task SetInterval(TimeSpan interval, byte targetNodeID, CancellationToken cancellationToken = default)
        {
            byte[] seconds = PayloadConverter.FromUInt24((uint)interval.TotalSeconds);
            await SendCommand(WakeUpCommand.IntervalSet, cancellationToken, seconds[0], seconds[1], seconds[2], targetNodeID);
        }

        public async Task NoMoreInformation(CancellationToken cancellationToken = default)
        {
            Log.Information($"Node {node.ID} returned to sleep");
            await SendCommand(WakeUpCommand.NoMoreInformation, cancellationToken);
        }

        public async Task<WakeUpIntervalCapabilitiesReport> GetIntervalCapabilities(CancellationToken cancellationToken = default)
        {
            ReportMessage message = await SendReceive(WakeUpCommand.IntervalCapabilitiesGet, WakeUpCommand.IntervalCapabilitiesReport, cancellationToken);
            return new WakeUpIntervalCapabilitiesReport(message.Payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)WakeUpCommand.Notification)
            {
                while (taskCompletionSources.TryDequeue(out TaskCompletionSource? tcs))
                    tcs.TrySetResult();
                await FireEvent(Awake, null);
                Log.Information($"Node {node.ID} awake");
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }

        public async Task WaitForAwake(CancellationToken cancellationToken = default)
        {
            TaskCompletionSource tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            cancellationToken.Register(() => tcs.TrySetCanceled());
            taskCompletionSources.Enqueue(tcs);
            await tcs.Task;
        }
    }
}
