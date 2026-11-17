
using Cycle_App.Data;
using CycleMaintenance.Services.Authentication;
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
        // 1. Get user ID from route
        var userId = Route<Guid>("id");

        using var connection = _dbConnectionFactory.CreateConnection();

        if (connection is not NpgsqlConnection dbConnection)
        {
            throw new InvalidOperationException(
                "Expected an Npgsql database connection.");
        }

        // 2. Open database connection
        await dbConnection.OpenAsync(ct);

        // 3. Begin transaction
        using var transaction = await dbConnection.BeginTransactionAsync(ct);

        try
        {
            

            // 5. Update user details
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
                    Id = userId,
                    req.FirstName,
                    req.LastName,
                    req.Email,
                    req.PhoneNumber,
                    req.IsActive
                },
                transaction);

            // 6. Check whether user exists
            if (user == null)
            {
                await transaction.RollbackAsync(ct);

                await Send.NotFoundAsync(ct);
                return;
            }

            // 7. Remove previous role assignments
            const string deleteRolesSql = """
                DELETE FROM UserRoles
                WHERE UserId = @UserId;
                """;

            await dbConnection.ExecuteAsync(
                deleteRolesSql,
                new { UserId = userId },
                transaction);

            // 8. Insert updated role assignments
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
                        UserId = userId,
                        RoleId = roleId
                    },
                    transaction);
            }

            // 9. Remove previous group assignments
            const string deleteGroupsSql = """
                DELETE FROM UserGroups
                WHERE UserId = @UserId;
                """;

            await dbConnection.ExecuteAsync(
                deleteGroupsSql,
                new { UserId = userId },
                transaction);

            // 10. Insert updated group assignments
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
                        UserId = userId,
                        GroupId = groupId
                    },
                    transaction);
            }

            // 11. Commit all changes
            await transaction.CommitAsync(ct);

            // 12. Return updated user
            await Send.OkAsync(user, ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
