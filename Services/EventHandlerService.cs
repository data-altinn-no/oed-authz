using System.Text.Json;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz.Services;
public class AltinnEventHandlerService : IAltinnEventHandlerService
{
    private readonly IOedRoleRepositoryService _oedRoleRepositoryService;

    public AltinnEventHandlerService(IOedRoleRepositoryService oedRoleRepositoryService)
    {
        _oedRoleRepositoryService = oedRoleRepositoryService;
    }

    public async Task HandleDaEvent(CloudEvent daEvent)
    {
        switch (daEvent.Type)
        {
            case "roleAssignment":
                foreach (var roleAssignment in GetOedRoleAssignments(daEvent)) await _oedRoleRepositoryService.AddRoleAssignment(roleAssignment);
                break;
            case "roleRevocation":
                foreach (var roleAssignment in GetOedRoleAssignments(daEvent)) await _oedRoleRepositoryService.RemoveRoleAssignment(roleAssignment);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(daEvent.Type));
        }
    }

    private List<OedRoleAssignment> GetOedRoleAssignments(CloudEvent daEvent)
    {
        if (daEvent.Subject == null)
        {
            throw new ArgumentNullException(nameof(daEvent.Subject));
        }

        var subject = daEvent.Subject.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (subject.Length != 2 || subject[0] != "person" || !Utils.IsValidSsn(subject[1]))
        {
            throw new ArgumentException(nameof(daEvent.Subject) + " must be SSN with '/person/' prefix");
        }

        var estateSsn = subject[1];

        if (daEvent.Data == null)
        {
            throw new ArgumentNullException(nameof(daEvent.Data));
        }

        var eventRoleAssignment = JsonSerializer.Deserialize<EventRoleAssignmentData>(daEvent.Data.ToString()!)!;

        var roleCode = eventRoleAssignment.RoleCode switch
        {
            "formuesfullmakt"                       => "urn:digitaltdodsbo:formuesfullmakt",
            "kandidatarving_partnerEllerEktefelle"  => "urn:digitaltdodsbo:arving:partnerEllerEktefelle",
            "kandidatarving_barn"                   => "urn:digitaltdodsbo:arving:barn",
            "kandidatarving_barnebarn"              => "urn:digitaltdodsbo:arving:barnebarn",
            "kandidatarving_mor"                    => "urn:digitaltdodsbo:arving:mor",
            "kandidatarving_far"                    => "urn:digitaltdodsbo:arving:far",
            "kandidatarving_onkel"                  => "urn:digitaltdodsbo:arving:onkel",
            "kandidatarving_tante"                  => "urn:digitaltdodsbo:arving:tante",
            "kandidatarving_soskenbarn"             => "urn:digitaltdodsbo:arving:soskenbarn",
            "kandidatarving_besteforelder"          => "urn:digitaltdodsbo:arving:besteforelder",
            _ => throw new ArgumentOutOfRangeException(nameof(eventRoleAssignment.RoleCode))
        };

        if (!Utils.IsValidSsn(eventRoleAssignment.Recipient))
        {
            throw new ArgumentException(nameof(eventRoleAssignment.Recipient));
        }

        var now = DateTimeOffset.UtcNow;

        var roleAssignments = new List<OedRoleAssignment>
        {
            new()
            {
                EstateSsn = estateSsn,
                RecipientSsn = eventRoleAssignment.Recipient,
                RoleCode = roleCode,
                Created = now
            }
        };

        // Include base "arving" role 
        if (roleCode.Contains(":arving:"))
        {
            roleAssignments.Add(new()
            {
                EstateSsn = estateSsn,
                RecipientSsn = eventRoleAssignment.Recipient,
                RoleCode = "urn:digitaltdodsbo:arving",
                Created = now
            });
        }

        return roleAssignments;
    }
}
