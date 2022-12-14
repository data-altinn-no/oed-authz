using oed_authz.Models;

namespace oed_authz.Interfaces;
public interface IOedRoleRepositoryService
{
    public Task<List<OedRoleAssignment>> GetRoleAssignmentsForEstate(string estateSsn, string? recipientSsnOnly = null);
    public Task<List<OedRoleAssignment>> GetRoleAssignmentsForPerson(string recipientSsn, string? estateSsnOnly = null);
    public Task AddRoleAssignment(OedRoleAssignment roleAssignment);
    public Task RemoveRoleAssignment(OedRoleAssignment roleAssignment);
}
