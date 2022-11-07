using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Settings;

namespace oed_authz.Services;
public class OedRoleRepositoryService : IOedRoleRepositoryService
{
    private readonly NpgsqlDataSourceBuilder _dataSourceBuilder;

    public OedRoleRepositoryService(IOptions<ConnectionStrings> connectionStrings, ILoggerFactory loggerFactory)
    {
        _dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionStrings.Value.PostgreSql);
        _dataSourceBuilder.UseLoggerFactory(loggerFactory); // Configure logging
       
    }

    public async Task<List<OedRoleAssignment>> GetRoleAssignmentsForEstate(string estateSsn, string? recipientSsnOnly = null) => await Query(estateSsn, recipientSsnOnly);
    
    public async Task<List<OedRoleAssignment>> GetRoleAssignmentsForUser(string recipientSsn, string? estateSsnOnly = null) =>  await Query(estateSsnOnly, recipientSsn);

    public async Task AddRoleAssignment(OedRoleAssignment roleAssignment)
    {
        await using var dataSource = _dataSourceBuilder.Build();

        await using var cmd = dataSource.CreateCommand("INSERT INTO oedauthz.roleassignments (estateSsn, recipientSsn, roleCode, created) VALUES ($1, $2, $3, $4)");
        cmd.Parameters.AddWithValue(roleAssignment.EstateSsn);
        cmd.Parameters.AddWithValue(roleAssignment.RecipientSsn);
        cmd.Parameters.AddWithValue(roleAssignment.RoleCode);
        cmd.Parameters.AddWithValue(roleAssignment.Created);

        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<List<OedRoleAssignment>> Query(string? estateSsn, string? recipientSsn)
    {

        var baseSql = "SELECT estateSsn, recipientSsn, roleCode, created FROM oedauthz.roleassignments";

        if (estateSsn != null && !Utils.IsValidSsn(estateSsn))
        {
            throw new ArgumentException(nameof(estateSsn));
        }

        if (recipientSsn != null && !Utils.IsValidSsn(recipientSsn))
        {
            throw new ArgumentException(nameof(recipientSsn));
        }

        await using var dataSource = _dataSourceBuilder.Build();
        NpgsqlCommand cmd;

        if (estateSsn != null && recipientSsn != null)
        {
            cmd = dataSource.CreateCommand(baseSql + " WHERE estateSsn = $1 AND recipientSsn = $2");
            cmd.Parameters.AddWithValue(estateSsn);
            cmd.Parameters.AddWithValue(recipientSsn);
        }
        else if (estateSsn != null)
        {
            cmd = dataSource.CreateCommand(baseSql + " WHERE estateSsn = $1");
            cmd.Parameters.AddWithValue(estateSsn);
        }
        else if (recipientSsn != null)
        {
            cmd = dataSource.CreateCommand(baseSql + " WHERE recipientSsn = $1");
            cmd.Parameters.AddWithValue(recipientSsn);
        }
        else
        {
            throw new ArgumentNullException(nameof(recipientSsn), "Both recipientSsn and estateSsn cannot be null");
        }

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync();

            var roleAssignments = new List<OedRoleAssignment>();
            while (await reader.ReadAsync())
            {
                roleAssignments.Add(new OedRoleAssignment
                {
                    EstateSsn = reader.GetString(0),
                    RecipientSsn = reader.GetString(1),
                    RoleCode = reader.GetString(2),
                    Created = reader.GetDateTime(3)
                });
            }

            return roleAssignments;
        }
        finally
        {
            await cmd.DisposeAsync();
        }
    }



}
