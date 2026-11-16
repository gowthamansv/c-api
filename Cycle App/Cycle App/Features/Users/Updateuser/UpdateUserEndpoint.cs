
using Cycle_App.Data;
using Dapper;
using FastEndpoints;
using Npgsql;

namespace CycleMaintenance.Features.Users.UpdateUser;

public class UpdateUserEndpoint
    : Endpoint<UpdateUserRequest, UpdateUserResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UpdateUserEndpoint(
        IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public override void Configure()
    {
        Put("/api/users/{id}");

        // Temporary during development.
        // Later restrict to Admin users.
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        UpdateUserRequest req,
        CancellationToken ct)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        if (connection is not NpgsqlConnection dbConnection)
        {
            throw new InvalidOperationException(
                "Expected an Npgsql database connection.");
        }

        // 1. Open database connection
        await dbConnection.OpenAsync(ct);

        // 2. Begin transaction
        using var transaction = dbConnection.BeginTransaction();

        try
        {
            // 3. Update user details
            const string updateUserSql = """
                UPDATE Users
                SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    PhoneNumber = @PhoneNumber,
                    IsActive = @IsActive
                WHERE Id = @Id
                RETURNING
                    Id,
                    FirstName,
                    LastName,
                    Email,
                    PhoneNumber,
                    IsActive,
                    CreatedAt;
                """;

            var user = await dbConnection.QuerySingleOrDefaultAsync<UpdateUserResponse>(
                updateUserSql,
                new
                {
                    req.Id,
                    req.FirstName,
                    req.LastName,
                    req.Email,
                    req.PhoneNumber,
                    req.IsActive
                },
                transaction);

            // 4. Check whether user exists
            if (user == null)
            {
                transaction.Rollback();

                await Send.NotFoundAsync(ct);
                return;
            }

            // 5. Remove previous role assignments
            const string deleteRolesSql = """
                DELETE FROM UserRoles
                WHERE UserId = @UserId;
                """;

            await dbConnection.ExecuteAsync(
                deleteRolesSql,
                new { UserId = req.Id },
                transaction);

            // 6. Insert updated role assignments
            const string insertRoleSql = """
                INSERT INTO UserRoles
                (
                    Id,
                    UserId,
                    RoleId,
                    CreatedAt
                )
                VALUES
                (
                    gen_random_uuid(),
                    @UserId,
                    @RoleId,
                    NOW()
                );
                """;

            foreach (var roleId in (req.RoleIds ?? new()).Distinct())
            {
                await dbConnection.ExecuteAsync(
                    insertRoleSql,
                    new
                    {
                        UserId = req.Id,
                        RoleId = roleId
                    },
                    transaction);
            }

            // 7. Remove previous group assignments
            const string deleteGroupsSql = """
                DELETE FROM UserGroups
                WHERE UserId = @UserId;
                """;

            await dbConnection.ExecuteAsync(
                deleteGroupsSql,
                new { UserId = req.Id },
                transaction);

            // 8. Insert updated group assignments
            const string insertGroupSql = """
                INSERT INTO UserGroups
                (
                    Id,
                    UserId,
                    GroupId,
                    CreatedAt
                )
                VALUES
                (
                    gen_random_uuid(),
                    @UserId,
                    @GroupId,
                    NOW()
                );
                """;

            foreach (var groupId in (req.GroupIds ?? new()).Distinct())
            {
                await dbConnection.ExecuteAsync(
                    insertGroupSql,
                    new
                    {
                        UserId = req.Id,
                        GroupId = groupId
                    },
                    transaction);
            }

            // 9. Commit all changes
            transaction.Commit();

            // 10. Return updated user
            await Send.OkAsync(user, ct);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
