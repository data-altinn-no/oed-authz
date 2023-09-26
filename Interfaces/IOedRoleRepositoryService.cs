using oed_authz.Models;

namespace oed_authz.Interfaces;
public interface IOedRoleRepositoryService
{
    public Task<List<RepositoryRoleAssignment>> GetRoleAssignmentsForEstate(string estateSsn, string? recipientSsnOnly = null);
    public Task<List<RepositoryRoleAssignment>> GetRoleAssignmentsForPerson(string recipientSsn, string? estateSsnOnly = null);
    public Task AddRoleAssignment(RepositoryRoleAssignment roleAssignment);
    public Task RemoveRoleAssignment(RepositoryRoleAssignment roleAssignment);
}
