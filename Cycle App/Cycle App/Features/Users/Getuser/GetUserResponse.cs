
namespace CycleMaintenance.Features.Users.GetUser;
public class GetUserResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<RoleResponse> Roles { get; set; } = new();
    public List<GroupResponse> Groups { get; set; } = new();
}

public class RoleResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserGroupMappingResponse
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}


public class UserRoleMappingResponse
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}