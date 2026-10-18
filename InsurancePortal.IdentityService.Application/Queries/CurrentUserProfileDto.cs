namespace InsurancePortal.IdentityService.Application.Queries
{
    public record CurrentUserProfileDto(
        Guid Id,
        Guid TenantId,
        Guid? OrganizationId,
        string Email,
        string? Username,
        string? FirstName,
        string? LastName,
        bool IsGuest,
        bool IsActive,
        bool EmailConfirmed,
        IReadOnlyCollection<string> Roles);
}
