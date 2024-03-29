﻿using System.Text.Json;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Models.Dto;
using oed_authz.Settings;

namespace oed_authz.Services;
public class AltinnEventHandlerService : IAltinnEventHandlerService
{
    private readonly IOedRoleRepositoryService _oedRoleRepositoryService;
    private readonly IProxyManagementService _proxyManagementService;
    private readonly ILogger<AltinnEventHandlerService> _logger;

    public AltinnEventHandlerService(
        IOedRoleRepositoryService oedRoleRepositoryService,
        IProxyManagementService proxyManagementService,
        ILogger<AltinnEventHandlerService> logger)
    {
        _oedRoleRepositoryService = oedRoleRepositoryService;
        _proxyManagementService = proxyManagementService;
        _logger = logger;
    }

    public async Task HandleEvent(CloudEvent cloudEvent)
    {
        switch (cloudEvent.Type)
        {
            case "no.altinn.events.digitalt-dodsbo.v1.case-status-updated":
                await HandleEstateInstanceCreatedOrUpdated(cloudEvent);
                break;
            case "no.altinn.events.digitalt-dodsbo.v1.heir-roles-updated":
                await HandleEstateInstanceCreatedOrUpdated(cloudEvent);
                break;
            case "platform.events.validatesubscription":
                return;
            default:
                throw new ArgumentException("Unknown event type");
        }
    }

    private async Task HandleEstateInstanceCreatedOrUpdated(CloudEvent daEvent)
    {
        if (daEvent.Data == null)
        {
            _logger.LogError("Empty data in event: {CloudEvent}", JsonSerializer.Serialize(daEvent));
            throw new ArgumentNullException(nameof(daEvent.Data));
        }

        var updatedRoleAssignments = JsonSerializer.Deserialize<EventRoleAssignmentDataDto>(daEvent.Data.ToString()!)!;

        _logger.LogInformation("Handling event {Id}: {CloudEvent}", daEvent.Id, JsonSerializer.Serialize(daEvent));

        // Get all current roles given from this estate
        var estateSsn = Utils.GetEstateSsnFromCloudEvent(daEvent);
        var currentRoleAssignments = await _oedRoleRepositoryService.GetRoleAssignmentsForEstate(estateSsn);

        // Filter out all role assignments that are not court assigned
        currentRoleAssignments = currentRoleAssignments.Where(x => x.RoleCode.StartsWith(Constants.CourtRoleCodePrefix)).ToList();

        // Find assignments in updated list but not in current list to add
        var assignmentsToAdd = new List<RepositoryRoleAssignment>();
        foreach (var updatedRoleAssignment in updatedRoleAssignments.HeirRoles)
        {
            if (!Utils.IsValidSsn(updatedRoleAssignment.Nin))
            {
                throw new ArgumentException(nameof(updatedRoleAssignment.Nin));
            }

            // Check if we have any current role assigments that are newer than this. If so, this means we're handling
            // an out-of-order and outdated event so we just bail.
            if (currentRoleAssignments.Any(x => x.Created >= daEvent.Time))
            {
                return;
            }

            // Check that all role codes are within the correct namespace
            if (!updatedRoleAssignment.Role.StartsWith(Constants.CourtRoleCodePrefix))
            {
                throw new ArgumentException("Rolecode must start with " + Constants.CourtRoleCodePrefix);
            }

            if (!currentRoleAssignments.Exists(x => x.RecipientSsn == updatedRoleAssignment.Nin && x.RoleCode == updatedRoleAssignment.Role))
            {
                assignmentsToAdd.Add(new RepositoryRoleAssignment
                {
                    EstateSsn = estateSsn,
                    RecipientSsn = updatedRoleAssignment.Nin,
                    RoleCode = updatedRoleAssignment.Role,
                    Created = daEvent.Time
                });
            }
        }

        // Find assignments in current list that's not in the updated list. These should be removed.
        var assignmentsToRemove = new List<RepositoryRoleAssignment>();
        foreach (var currentRoleAssignment in currentRoleAssignments)
        {
            if (!updatedRoleAssignments.HeirRoles.Exists(x =>
                    x.Nin == currentRoleAssignment.RecipientSsn && x.Role == currentRoleAssignment.RoleCode))
            {
                assignmentsToRemove.Add(new RepositoryRoleAssignment
                {
                    EstateSsn = estateSsn,
                    RecipientSsn = currentRoleAssignment.RecipientSsn,
                    RoleCode = currentRoleAssignment.RoleCode
                });
            }
        }

        _logger.LogInformation("Handling event {Id}: {AssignmentsToAdd} assignments to add and {AssignmentsToRemove} assignments to remove",
            daEvent.Id, assignmentsToAdd.Count, assignmentsToRemove.Count);

        foreach (var roleAssignment in assignmentsToAdd)
        {
            await _oedRoleRepositoryService.AddRoleAssignment(roleAssignment);
        }

        foreach (var roleAssignment in assignmentsToRemove)
        {
            await _oedRoleRepositoryService.RemoveRoleAssignment(roleAssignment);
        }

        // Handle collective proxy roles
        await _proxyManagementService.UpdateProxyRoleAssigments(estateSsn);
    }
}
