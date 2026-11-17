public class UpdateUserRequest
{
    // User ID from the URL
    public Guid Id { get; set; }

    // User details
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    // Account status
    public bool IsActive { get; set; }



    public List<Guid>? RoleIds { get; set; }

    public List<Guid>? GroupIds { get; set; }
}