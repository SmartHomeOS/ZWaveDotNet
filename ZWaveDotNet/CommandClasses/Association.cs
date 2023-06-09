﻿using Serilog;
using ZWaveDotNet.CommandClassReports;
using ZWaveDotNet.CommandClassReports.Enums;
using ZWaveDotNet.Entities;
using ZWaveDotNet.Enums;
using ZWaveDotNet.SerialAPI;

namespace ZWaveDotNet.CommandClasses
{
    [CCVersion(CommandClass.Association, 1, 3)]
    public class Association : CommandClassBase
    {
        public const byte LIFELINE_GROUP = 0x1;
        enum AssociationCommand
        {
            Set = 0x01,
            Get = 0x02,
            Report = 0x03,
            Remove = 0x04,
            GroupingsGet = 0x05,
            GroupingsReport = 0x06,
            SpecificGroupGet = 0x0B,
            SpecificGroupReport = 0x0C
        }
        public Association(Node node, byte endpoint) : base(node, endpoint, CommandClass.Association) { }

        public async Task<AssociationReport> Get(byte groupID, CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(AssociationCommand.Get, AssociationCommand.Report, cancellationToken, groupID);
            return new AssociationReport(response.Payload);
        }

        public async Task<byte> GetSpecific(CancellationToken cancellationToken)
        {
            var response = await SendReceive(AssociationCommand.SpecificGroupGet, AssociationCommand.SpecificGroupReport, cancellationToken);
            return response.Payload.Span[0];
        }

        public async Task Add(byte groupID, CancellationToken cancellationToken, params byte[] nodeIDs)
        {
            await SendCommand(AssociationCommand.Set, cancellationToken, nodeIDs.Prepend(groupID).ToArray());
        }

        public async Task Remove(byte groupID, CancellationToken cancellationToken, params byte[] nodeIDs)
        {
            await SendCommand(AssociationCommand.Remove, cancellationToken, nodeIDs.Prepend(groupID).ToArray());
        }

        public async Task<AssociationGroupsReport> GetGroups(CancellationToken cancellationToken)
        {
            ReportMessage response = await SendReceive(AssociationCommand.GroupingsGet, AssociationCommand.GroupingsReport, cancellationToken);
            return new AssociationGroupsReport(response.Payload);
        }

        public override async Task Interview(CancellationToken cancellationToken)
        {
            await Add(LIFELINE_GROUP, cancellationToken, (byte)controller.ControllerID);
            Log.Information("Assigned Lifeline Group");
        }

        protected override async Task<SupervisionStatus> Handle(ReportMessage message)
        {
            //Nothing Unsolicited
            return SupervisionStatus.NoSupport;
        }
    }
}
