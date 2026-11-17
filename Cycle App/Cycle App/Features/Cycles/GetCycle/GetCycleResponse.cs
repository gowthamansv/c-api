public class GetCyclesResponse
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public Guid QrToken { get; set; }

    public DateTime CreatedAt { get; set; }
}