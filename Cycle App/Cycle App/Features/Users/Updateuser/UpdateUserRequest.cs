
namespace CycleMaintenance.Features.Users.UpdateUser;

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

    // Assigned roles
    public List<Guid> RoleIds { get; set; } = new();

    // Assigned groups
    public List<Guid> GroupIds { get; set; } = new();
}
