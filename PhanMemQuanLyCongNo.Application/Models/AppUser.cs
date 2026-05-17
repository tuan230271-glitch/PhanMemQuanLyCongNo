namespace PhanMemQuanLyCongNo.Application.Models;

public class AppUser
{
    public AppUser()
    {
    }

    public AppUser(Guid id, Guid tenantId, string fullName, string email, UserRole role, bool isActive)
    {
        Id = id;
        TenantId = tenantId;
        FullName = fullName;
        Email = email;
        Role = role;
        IsActive = isActive;
    }

    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
}
