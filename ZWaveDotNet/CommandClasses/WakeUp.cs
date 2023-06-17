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
        private ConcurrentQueue<TaskCompletionSource> taskCompletionSources = new ConcurrentQueue<TaskCompletionSource>();
        public event CommandClassEvent? Awake;

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
