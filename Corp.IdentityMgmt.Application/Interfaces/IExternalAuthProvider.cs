namespace Corp.IdentityMgmt.Application.Interfaces
{
    internal interface IExternalAuthProvider
    {
        Task<(string ProviderUserId, string Email)?> ValidateAsync(string authCode);
    }
}
