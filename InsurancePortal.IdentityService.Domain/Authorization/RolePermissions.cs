namespace InsurancePortal.IdentityService.Domain.Authorization
{
    public static class RolePermissions
    {
        private static readonly IReadOnlyDictionary<string, string[]> Map = new Dictionary<string, string[]>
        {
            [Roles.Guest] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.UserConvertOwn,
                Permissions.QuoteCreateGuest,
                Permissions.QuoteReadOwn,
                Permissions.QuoteUpdateOwn,
                Permissions.DashboardPersonalViewOwn
            ],
            [Roles.Registered] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.QuoteReadOwn,
                Permissions.QuoteCreateOwn,
                Permissions.QuoteUpdateOwn,
                Permissions.PaymentReadOwn,
                Permissions.PaymentCreateOwn,
                Permissions.PolicyReadOwn,
                Permissions.ClaimSubmitOwn,
                Permissions.ClaimReadOwn,
                Permissions.ClaimDocumentUploadOwn,
                Permissions.DashboardPersonalViewOwn
            ],
            [Roles.Agents] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.QuoteCreateAssigned,
                Permissions.QuoteUpdateAssigned,
                Permissions.QuoteReadAssigned,
                Permissions.PaymentReadAssigned,
                Permissions.PolicyIssueAssigned,
                Permissions.PolicyReadAssigned,
                Permissions.PolicyUpdateAssigned,
                Permissions.PolicyRenewAssigned,
                Permissions.DashboardPersonalViewOwn,
                Permissions.DashboardAgentViewAssigned
            ],
            [Roles.Associates] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.UserReadTenant,
                Permissions.QuoteReadTenant,
                Permissions.QuoteApproveTenant,
                Permissions.QuoteDeleteTenant,
                Permissions.PaymentReadTenant,
                Permissions.PaymentRefundTenant,
                Permissions.PolicyReadTenant,
                Permissions.PolicyIssueTenant,
                Permissions.PolicyCancelTenant,
                Permissions.ClaimReadAssigned,
                Permissions.ClaimReadTenant,
                Permissions.ClaimReviewTenant,
                Permissions.ClaimApproveTenant,
                Permissions.ClaimSettleTenant,
                Permissions.ClaimAssignTenant,
                Permissions.DashboardPersonalViewOwn,
                Permissions.DashboardOperationsViewTenant
            ],
            [Roles.ProvidersAdmin] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.UserOrgManageOrganization,
                Permissions.PaymentReadOrganization,
                Permissions.ClaimReadOrganization,
                Permissions.ClaimSubmitOrganization,
                Permissions.ClaimStatusUpdateOrganization,
                Permissions.DashboardPersonalViewOwn,
                Permissions.DashboardProviderViewOrganization
            ],
            [Roles.ProvidersStaff] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.PaymentReadOrganization,
                Permissions.ClaimReadOrganization,
                Permissions.ClaimSubmitOrganization,
                Permissions.ClaimStatusUpdateOrganization,
                Permissions.DashboardPersonalViewOwn,
                Permissions.DashboardProviderViewOrganization
            ],
            [Roles.Admin] =
            [
                Permissions.UserProfileReadOwn,
                Permissions.UserProfileUpdateOwn,
                Permissions.UserReadTenant,
                Permissions.UserReadAny,
                Permissions.UserCreateTenant,
                Permissions.UserDisableTenant,
                Permissions.UserRoleAssignTenant,
                Permissions.UserRoleAssignAny,
                Permissions.QuoteReadTenant,
                Permissions.QuoteDeleteTenant,
                Permissions.PaymentReadTenant,
                Permissions.PaymentReconcileTenant,
                Permissions.PolicyReadTenant,
                Permissions.PolicyCancelTenant,
                Permissions.ClaimReadTenant,
                Permissions.ClaimAssignTenant,
                Permissions.DashboardPersonalViewOwn,
                Permissions.DashboardAgentViewAssigned,
                Permissions.DashboardOperationsViewTenant,
                Permissions.DashboardAdminViewAny
            ]
        };

        public static IReadOnlyCollection<string> GetPermissions(string role) =>
            Map.TryGetValue(role, out var permissions) ? permissions : Array.Empty<string>();
    }
}
