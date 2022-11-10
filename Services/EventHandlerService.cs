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
            case "estateInstanceCreatedOrUpdated":
                await HandleEstateInstanceCreatedOrUpdated(daEvent);
                break;
            default: 
                // Ignore all other event types
                return;
        }
    }

    private async Task HandleEstateInstanceCreatedOrUpdated(CloudEvent daEvent)
    {
        if (daEvent.Data == null)
        {
            throw new ArgumentNullException(nameof(daEvent.Data));

        }
        var updatedRoleAssignments = JsonSerializer.Deserialize<List<EventRoleAssignmentData>>(daEvent.Data.ToString()!)!;
        
        // Get all current roles given from this estate
        var estateSsn = Utils.GetEstateSsnFromCloudEvent(daEvent);
        var currentRoleAssignments = await _oedRoleRepositoryService.GetRoleAssignmentsForEstate(estateSsn);
        var now = DateTimeOffset.UtcNow;

        // Find assignments in updated list but not in current list to add
        var assignmentsToAdd = new List<OedRoleAssignment>();
        foreach (var updatedRoleAssignment in updatedRoleAssignments)
        {
            if (!Utils.IsValidSsn(updatedRoleAssignment.Recipient))
            {
                throw new ArgumentException(nameof(updatedRoleAssignment.Recipient));
            }

            if (!currentRoleAssignments.Exists(x => x.Recipient == updatedRoleAssignment.Recipient && x.RoleCode == GetPipRoleCode(updatedRoleAssignment.RoleCode)))
            {
                assignmentsToAdd.Add(new OedRoleAssignment
                {
                    EstateSsn = estateSsn,
                    Recipient = updatedRoleAssignment.Recipient,
                    RoleCode = GetPipRoleCode(updatedRoleAssignment.RoleCode),
                    Created = now
                });
            }
        }

        // Find assignments not in updated list but in current list to remove
        var assignmentsToRemove = new List<OedRoleAssignment>();
        foreach (var currentRoleAssignment in currentRoleAssignments)
        {
            if (!updatedRoleAssignments.Exists(x =>
                    x.Recipient == currentRoleAssignment.Recipient && GetPipRoleCode(x.RoleCode) == currentRoleAssignment.RoleCode))
            {
                assignmentsToRemove.Add(new OedRoleAssignment
                {
                    EstateSsn = estateSsn,
                    Recipient = currentRoleAssignment.Recipient,
                    RoleCode = currentRoleAssignment.RoleCode
                });
            }
        }

        foreach (var roleAssignment in assignmentsToAdd)
        {
            await _oedRoleRepositoryService.AddRoleAssignment(roleAssignment);
        }

        foreach (var roleAssignment in assignmentsToRemove)
        {
            await _oedRoleRepositoryService.RemoveRoleAssignment(roleAssignment);
        }
    }

    private List<OedRoleAssignment> GetOedRoleAssignments(CloudEvent daEvent)
    {
        var estateSsn = Utils.GetEstateSsnFromCloudEvent(daEvent);

        if (daEvent.Data == null)
        {
            throw new ArgumentNullException(nameof(daEvent.Data));
        }

        var eventRoleAssignment = JsonSerializer.Deserialize<EventRoleAssignmentData>(daEvent.Data.ToString()!)!;

        if (!Utils.IsValidSsn(eventRoleAssignment.Recipient))
        {
            throw new ArgumentException(nameof(eventRoleAssignment.Recipient));
        }

        var now = DateTimeOffset.UtcNow;
        var roleCode = GetPipRoleCode(eventRoleAssignment.RoleCode);
        var roleAssignments = new List<OedRoleAssignment>
        {
            new()
            {
                EstateSsn = estateSsn,
                Recipient = eventRoleAssignment.Recipient,
                RoleCode = roleCode,
                Created = now
            }
        };

        return roleAssignments;
    }

    private string GetPipRoleCode(string roleCode)
    {
        return roleCode switch
        {
            "formuesfullmakt"                      => "urn:digitaltdodsbo:formuesfullmakt",
            "kandidatarving_ektefelleEllerPartner" => "urn:digitaltdodsbo:arving:ektefelleEllerPartner",
            "kandidatarving_barn"                  => "urn:digitaltdodsbo:arving:barn",
            "kandidatarving_barnebarn"             => "urn:digitaltdodsbo:arving:barnebarn",
            "kandidatarving_mor"                   => "urn:digitaltdodsbo:arving:mor",
            "kandidatarving_far"                   => "urn:digitaltdodsbo:arving:far",
            "kandidatarving_onkel"                 => "urn:digitaltdodsbo:arving:onkel",
            "kandidatarving_tante"                 => "urn:digitaltdodsbo:arving:tante",
            "kandidatarving_soskenbarn"            => "urn:digitaltdodsbo:arving:soskenbarn",
            "kandidatarving_besteforelder"         => "urn:digitaltdodsbo:arving:besteforelder",
            _ => throw new ArgumentOutOfRangeException(nameof(roleCode))
        };
    }
}
