using System.Collections;
using System.Data;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.SwitchColor, 1, 3)]
    public class SwitchColor : CommandClassBase
    {
        public event CommandClassEvent? ColorChange;

        enum SwitchColorCommand
        {
            SupportedGet = 0x01,
            SupportedReport = 0x02,
            Get = 0x03,
            Report = 0x04,
            Set = 0x05,
            StartLevelChange = 0x06,
            StopLevelChange = 0x07
        }

        public SwitchColor(Node node, byte endpoint) : base(node, endpoint, CommandClass.SwitchColor) { }

        public async Task Set(KeyValuePair<ColorType,byte>[] components, CancellationToken cancellationToken = default, TimeSpan? duration = null)
        {
            var payload = new List<byte>();
            payload.Add((byte)Math.Min(components.Length, 31)); //31 Components max
            payload.AddRange(components.SelectMany(element => new byte[] { (byte)element.Key, element.Value }));
            if (duration != null)
                payload.Add(PayloadConverter.GetByte(duration.Value));
            await SendCommand(SwitchColorCommand.Set, cancellationToken, payload.ToArray());
        }

        public async Task<SwitchColorReport> Get(ColorType component, CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchColorCommand.Get, SwitchColorCommand.Report, cancellationToken, (byte)component);
            return new SwitchColorReport(response.Payload);
        }

        public async Task<ColorType[]> GetSupported(CancellationToken cancellationToken = default)
        {
            ReportMessage response = await SendReceive(SwitchColorCommand.SupportedGet, SwitchColorCommand.SupportedReport, cancellationToken);
            if (response.Payload.Length != 2)
                throw new DataException($"The response was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            List<ColorType> ret = new List<ColorType>();
            BitArray bits = new BitArray(response.Payload.ToArray());
            for (byte i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                    ret.Add((ColorType)i);
            }
            return ret.ToArray();
        }

        public async Task StartLevelChange(bool up, ColorType component, int startLevel, CancellationToken cancellationToken = default)
        {
            byte flags = 0x0;
            if (startLevel < 0)
                flags |= 0x20; //Ignore Start
            if (up)
                flags |= 0x40;
            await SendCommand(SwitchColorCommand.StartLevelChange, cancellationToken, flags, (byte)component, (byte)Math.Max(0, startLevel));
        }

        public async Task StopLevelChange(ColorType component, CancellationToken cancellationToken = default)
        {
            await SendCommand(SwitchColorCommand.StopLevelChange, cancellationToken, (byte)component);
        }

        protected override async Task Handle(ReportMessage message)
        {
            if (message.Command == (byte)SwitchColorCommand.Report)
                await FireEvent(ColorChange, new SwitchColorReport(message.Payload));
        }
    }
}
