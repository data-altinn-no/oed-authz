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

    public async Task HandleDaEvent(CloudEventRequestModel daEvent)
    {
        if (daEvent.Type != "roleAssignment")
        {
            throw new ArgumentOutOfRangeException(nameof(daEvent.Type));
        }

        if (daEvent.AlternativeSubject == null)
        {
            throw new ArgumentNullException(nameof(daEvent.AlternativeSubject));
        }

        var alternativeSubject = daEvent.AlternativeSubject.Split('/');
        if (alternativeSubject.Length != 2 || alternativeSubject[0] != "person" || !Utils.IsValidSsn(alternativeSubject[1]))
        {
            throw new ArgumentException(nameof(daEvent.AlternativeSubject));
        }

        var estateSsn = alternativeSubject[1];

        if (daEvent.Data is not EventRoleAssignmentData eventRoleAssignment)
        {
            throw new ArgumentException(nameof(daEvent.Data));
        }

        var roleCode = eventRoleAssignment.RoleCode switch
        {
            "formuesfullmakt" => "urn:digitaltdodsbo:formuesfullmakt",
            "kandidatarving_partnerEllerEktefelle" => "urn:digitaltdodsbo:arving:partnerEllerEktefelle",
            "kandidatarving_barn" => "urn:digitaltdodsbo:arving:barn",
            "kandidatarving_mor" => "urn:digitaltdodsbo:arving:mor",
            "kandidatarving_far" => "urn:digitaltdodsbo:arving:far",
            _ => throw new ArgumentOutOfRangeException(nameof(eventRoleAssignment.RoleCode))
        };

        if (!Utils.IsValidSsn(eventRoleAssignment.Recipient))
        {
            throw new ArgumentException(nameof(eventRoleAssignment.Recipient));
        }

        var oedRoleAssignment = new OedRoleAssignment
        {
            EstateSsn = estateSsn,
            RecipientSsn = eventRoleAssignment.Recipient,
            RoleCode = roleCode
        };

        await _oedRoleRepositoryService.AddRoleAssignment(oedRoleAssignment);
    }
}
