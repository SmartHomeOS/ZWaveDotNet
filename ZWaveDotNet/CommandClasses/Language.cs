using System.Globalization;
using System.Text;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Language)]
    public class Language : CommandClassBase
    {
        enum LanguageCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public Language(Node node, byte endpoint) : base(node, endpoint, CommandClass.Language) { }

        public async Task<CultureInfo> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");
            ReportMessage response = await SendReceive(LanguageCommand.Get, LanguageCommand.Report, cancellationToken);
            return new CultureInfo(Encoding.ASCII.GetString(response.Payload.Slice(0, 3).Span));
        }

        public async Task Set(CultureInfo culture, RegionInfo region, CancellationToken cancellationToken = default)
        {
            byte[] payload = new byte[5];
            Encoding.ASCII.GetBytes(culture.ThreeLetterISOLanguageName).CopyTo(payload, 0);
            Encoding.ASCII.GetBytes(region.TwoLetterISORegionName).CopyTo(payload, 3);
            await SendCommand(LanguageCommand.Set, cancellationToken, payload);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Needed
            return SupervisionStatus.NoSupport;
        }
    }
}
