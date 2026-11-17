
using Cycle_App.Data;
using Dapper;
using FastEndpoints;

namespace CycleMaintenance.Features.Cycles.GetCycle;

public class GetCyclesEndpoint
    : Endpoint<EmptyRequest, List<GetCycleResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetCyclesEndpoint(
        IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public override void Configure()
    {
        Get("/api/cycles");
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        EmptyRequest req,
        CancellationToken ct)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        const string sql = """
            SELECT
                Id,
                CustomerId,
                Model,
                SerialNumber,
                QrToken,
                CreatedAt
            FROM Cycles
            ORDER BY CreatedAt DESC;
            """;

        var cycles = await connection.QueryAsync<GetCycleResponse>(
            new CommandDefinition(
                sql,
                cancellationToken: ct
            )
        );

        await Send.OkAsync(cycles.ToList(), ct);
    }
}

public class GetCycleResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public Guid QrToken { get; set; }

    public DateTime CreatedAt { get; set; }
}
