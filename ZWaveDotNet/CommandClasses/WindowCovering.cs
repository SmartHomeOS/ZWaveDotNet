﻿using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.WindowCovering)]
    public class WindowCovering : CommandClassBase
    {
        public event CommandClassEvent? Report;
        
        enum WindowCoveringCommand : byte
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x03,
            Report = 0x04,
            Set = 0x05,
            StartLevelChange = 0x06,
            StopLevelChange = 0x07
        }

        public WindowCovering(Node node, byte endpoint) : base(node, endpoint, CommandClass.WindowCovering) { }

        public async Task<WindowCoveringParameter[]> GetSupported(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(WindowCoveringCommand.SupportedGet, WindowCoveringCommand.SupportedReport, cancellationToken);
            if (response.Payload.Length < 2 || response.Payload.Span[0] == 0x0)
                throw new DataException($"The Supported Window Covering Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            BitArray bits = new BitArray(response.Payload.Slice(1, response.Payload.Span[0] & 0xF).ToArray());
            List<WindowCoveringParameter> ret = new List<WindowCoveringParameter>();
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((WindowCoveringParameter)i);
            }
            return ret.ToArray();
        }

        public async Task<WindowCoveringReport> Get(WindowCoveringParameter parameter, CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(WindowCoveringCommand.Get, WindowCoveringCommand.Report, cancellationToken, (byte)parameter);
            return new WindowCoveringReport(response.Payload);
        }

        public async Task Set(WindowCoveringParameter param, byte value, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            Dictionary<WindowCoveringParameter, byte> arg = new Dictionary<WindowCoveringParameter, byte>
            {
                { param, value }
            };
            await Set(arg, duration, cancellationToken);
        }

        public async Task Set(Dictionary<WindowCoveringParameter, byte> values, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[(values.Count + 1) * 2];
            payload[0] = (byte)values.Count;
            int i = 1;
            foreach (var kvp in values)
            {
                payload[i++] = (byte)kvp.Key;
                payload[i++] = kvp.Value;
            }
            payload[payload.Length - 1] = PayloadConverter.GetByte(duration);
            await SendCommand(WindowCoveringCommand.Set, cancellationToken, payload);
        }

        public async Task StartLevelChange(WindowCoveringParameter parameter, bool decrease, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[3];
            payload[0] = (decrease) ? (byte)0x40 : (byte)0x0;
            payload[1] = (byte)parameter;
            payload[2] = PayloadConverter.GetByte(duration);

            await SendCommand(WindowCoveringCommand.StartLevelChange, cancellationToken, payload);
        }

        public async Task StopLevelChange(WindowCoveringParameter parameter, CancellationToken cancellationToken = default)
        {
            await SendCommand(WindowCoveringCommand.StopLevelChange, cancellationToken, (byte)parameter);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            if (message.Command == (byte)WindowCoveringCommand.Report)
            {
                WindowCoveringReport rpt = new WindowCoveringReport(message.Payload);
                await FireEvent(Report, rpt);
                return SupervisionStatus.Success;
            }
            return SupervisionStatus.NoSupport;
        }
    }
}
