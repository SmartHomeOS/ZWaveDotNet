using System.Buffers.Binary;
using System.Data;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;
using ZWaveDotNet.Util;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.TimeParams)]
    public class TimeParameters : CommandClassBase
    {   
        enum TimeParamCommand : byte
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03
        }

        public TimeParameters(Node node, byte endpoint) : base(node, endpoint, CommandClass.TimeParams) { }

        public async Task<DateTime> Get(CancellationToken cancellationToken = default)
        {
            if (node.ID == Node.BROADCAST_ID)
                throw new MethodAccessException("GET methods may not be called on broadcast nodes");

            ReportMessage response = await SendReceive(TimeParamCommand.Get, TimeParamCommand.Report, cancellationToken);
            if (response.Payload.Length < 7)
                throw new DataException($"The Time Paramaters Report was not in the expected format. Payload: {MemoryUtil.Print(response.Payload)}");
            return new DateTime(BinaryPrimitives.ReadUInt16BigEndian(response.Payload.Span), response.Payload.Span[2], response.Payload.Span[3], 
                                response.Payload.Span[4], response.Payload.Span[5], response.Payload.Span[6], DateTimeKind.Utc);
        }

        public async Task Set(DateTime value, CancellationToken cancellationToken = default)
        {
            DateTime dt = value.ToUniversalTime();
            byte[] payload = new byte[7];
            BinaryPrimitives.WriteUInt16BigEndian(payload, (ushort)dt.Year);
            payload[2] = (byte)dt.Month;
            payload[3] = (byte)dt.Day;
            payload[4] = (byte)dt.Hour;
            payload[5] = (byte)dt.Minute;
            payload[6] = (byte)dt.Second;
            await SendCommand(TimeParamCommand.Set, cancellationToken, payload);
        }

        public override async Task Interview(CancellationToken cancellationToken)
        {
            await Set(DateTime.UtcNow, cancellationToken);
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Not Used
            return SupervisionStatus.NoSupport;
        }
    }
}
