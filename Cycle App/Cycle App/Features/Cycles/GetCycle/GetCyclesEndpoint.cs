
using Cycle_App.Data;
using Dapper;
using FastEndpoints;

namespace CycleMaintenance.Features.Cycles.GetCycle;

public class GetCyclesEndpoint
    : Endpoint<EmptyRequest, List<GetCyclesResponse>>
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
        c.Id,
        c.CustomerId,
        cu.Name AS CustomerName,
        c.Model,
        c.SerialNumber,
        c.QrToken,
        c.CreatedAt
    FROM Cycles c
    LEFT JOIN Customers cu ON cu.Id = c.CustomerId
    ORDER BY cu.Name;
    """;
        var cycles = await connection.QueryAsync<GetCyclesResponse>(sql);

Response = cycles.ToList();

        await Send.OkAsync(Response, ct);
    }
}


