namespace InsurancePortal.IdentityService.Application.Interfaces
{
    public record CurrentUserContext(
        Guid UserId,
        string? Email,
        string? Username,
        string? FirstName,
        string? LastName,
        Guid TenantId,
        Guid? OrganizationId,
        IReadOnlyCollection<string> Roles,
        IReadOnlyCollection<string> Permissions,
        bool IsGuest,
        bool IsAuthenticated);

    public interface ICurrentUserContext
    {
        CurrentUserContext? Current { get; }
    }
}
