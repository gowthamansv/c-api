
using Cycle_App.Data;
using Dapper;
using FastEndpoints;
using Npgsql;

namespace CycleMaintenance.Features.Users.DeleteUser;

public class DeleteUserEndpoint : EndpointWithoutRequest
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DeleteUserEndpoint(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public override void Configure()
    {
        Delete("/api/users/{id}");
        
        // Development only
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Route<Guid>("id");

        using var connection = _dbConnectionFactory.CreateConnection();

        if (connection is not NpgsqlConnection dbConnection)
        {
            throw new InvalidOperationException(
                "Expected an Npgsql database connection.");
        }

        await dbConnection.OpenAsync(ct);

        const string rolesql = """
        DELETE FROM UserRoles
        WHERE UserId = @UserId;
        """;

        const string groupsql = """
        DELETE FROM UserGroups
        WHERE UserId = @UserId;
        """;

        const string sql = """
            DELETE FROM Users
            WHERE Id = @UserId;
            """;

        var rowsAffected = await dbConnection.ExecuteAsync(
            sql,
            new { UserId = userId }
        );
        var roleAffected = await dbConnection.ExecuteAsync(
            rolesql,
            new { UserId = userId }
        );
        var groupAffected = await dbConnection.ExecuteAsync(
            groupsql,
            new { UserId = userId }
        );

        if (rowsAffected == 0 && roleAffected == 0 && groupAffected == 0)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
