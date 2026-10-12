using System;

namespace InsurancePortal.IdentityService.Application.DTOs
{
    /// <summary>
    /// Response DTO for authenticated user endpoint
    /// </summary>
    public record AuthenticatedUserResponseDto(
        string Message,
        string AuthenticatedUser,
        DateTime Timestamp,
        string Description);
}
