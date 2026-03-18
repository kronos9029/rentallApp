namespace RentalApp.Domain.Entities;

public sealed class UserRole
{
    public string UserId { get; set; } = string.Empty;

    public string RoleId { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }

    public AppUser User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
