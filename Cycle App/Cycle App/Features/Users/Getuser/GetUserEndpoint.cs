
using Cycle_App.Data;
using Dapper;
using FastEndpoints;
using Npgsql;

namespace CycleMaintenance.Features.Users.GetUser;

public class GetUserEndpoint
    : EndpointWithoutRequest<List<GetUserResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetUserEndpoint(
        IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public override void Configure()
    {
        Get("/api/users");

        // Temporary during development.
        // Later restrict to Admin users.
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        if (connection is not NpgsqlConnection dbConnection)
        {
            throw new InvalidOperationException(
                "Expected an Npgsql database connection.");
        }

        // 1. Open database connection
        await dbConnection.OpenAsync(ct);

        // 2. Get all users
        const string userSql = """
            SELECT
                Id,
                FirstName,
                LastName,
                Email,
                PhoneNumber,
                IsActive,
                CreatedAt
            FROM Users
            ORDER BY CreatedAt DESC;
            """;

        var users = (await dbConnection.QueryAsync<GetUserResponse>(
            userSql
        )).ToList();

        // 3. Get all user roles

      const string roleSql = """
    SELECT
        ur.UserId,
        r.Id,
        r.Name
    FROM UserRoles ur
    INNER JOIN Roles r
        ON ur.RoleId = r.Id
    WHERE r.IsActive = TRUE
    ORDER BY r.Name;
    """;

var roles = (
    await dbConnection.QueryAsync<UserRoleMappingResponse>(roleSql)
).ToList();

var roleLookup = roles
    .GroupBy(x => x.UserId)
    .ToDictionary(
        g => g.Key,
        g => g.Select(x => new RoleResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToList()
    );

foreach (var user in users)
{
    user.Roles = roleLookup.TryGetValue(user.Id, out var userRoles)
        ? userRoles
        : new List<RoleResponse>();
}
    // 4. Get all groups 
const string groupSql = """
    SELECT
        ug.UserId,
        g.Id,
        g.Name
    FROM UserGroups ug
    INNER JOIN Groups g
        ON ug.GroupId = g.Id
    WHERE g.IsActive = TRUE
    ORDER BY g.Name;
    """;

var groups = (
    await dbConnection.QueryAsync<UserGroupMappingResponse>(groupSql)
).ToList();

var groupLookup = groups
    .GroupBy(x => x.UserId)
    .ToDictionary(
        g => g.Key,
        g => g.Select(x => new GroupResponse
        {
            Id = x.Id,
            Name = x.Name
        }).ToList()
    );

foreach (var user in users)
{
    user.Groups = groupLookup.TryGetValue(user.Id, out var userGroups)
        ? userGroups
        : new List<GroupResponse>();
}

// const string groupSql = """
//     SELECT UserId, GroupId
//     FROM UserGroups;
//     """;

// var groups = (await dbConnection.QueryAsync<(Guid UserId, Guid GroupId)>(groupSql))
//     .ToList();

// // Group roles by UserId
// var roleLookup = roles
//     .GroupBy(r => r.UserId)
//     .ToDictionary(
//         g => g.Key,
//         g => g.Select(r => r.RoleId).ToList()
//     );

// // Group groups by UserId
// var groupLookup = groups
//     .GroupBy(g => g.UserId)
//     .ToDictionary(
//         g => g.Key,
//         g => g.Select(g => g.GroupId).ToList()
//     );

// // Attach roles and groups to users
// foreach (var user in users)
// {
//     user.RoleIds = roleLookup.TryGetValue(user.Id, out var roleIds)
//         ? roleIds
//         : new List<Guid>();

//     user.GroupIds = groupLookup.TryGetValue(user.Id, out var groupIds)
//         ? groupIds
//         : new List<Guid>();
// }



        // 6. Return all users
        await Send.OkAsync(users, ct);
    }
}