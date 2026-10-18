namespace InsurancePortal.IdentityService.Domain.Authorization
{
    public static class Permissions
    {
        // User
        public const string UserProfileReadOwn = "user.profile.read.own";
        public const string UserProfileUpdateOwn = "user.profile.update.own";
        public const string UserConvertOwn = "user.convert.own";
        public const string UserReadTenant = "user.read.tenant";
        public const string UserReadAny = "user.read.any";
        public const string UserCreateTenant = "user.create.tenant";
        public const string UserDisableTenant = "user.disable.tenant";
        public const string UserRoleAssignTenant = "user.role.assign.tenant";
        public const string UserRoleAssignAny = "user.role.assign.any";
        public const string UserOrgManageOrganization = "user.org.manage.organization";

        // Quote
        public const string QuoteCreateGuest = "quote.create.guest";
        public const string QuoteReadOwn = "quote.read.own";
        public const string QuoteCreateOwn = "quote.create.own";
        public const string QuoteUpdateOwn = "quote.update.own";
        public const string QuoteCreateAssigned = "quote.create.assigned";
        public const string QuoteUpdateAssigned = "quote.update.assigned";
        public const string QuoteReadAssigned = "quote.read.assigned";
        public const string QuoteReadTenant = "quote.read.tenant";
        public const string QuoteApproveTenant = "quote.approve.tenant";
        public const string QuoteDeleteTenant = "quote.delete.tenant";

        // Payment
        public const string PaymentReadOwn = "payment.read.own";
        public const string PaymentCreateOwn = "payment.create.own";
        public const string PaymentReadAssigned = "payment.read.assigned";
        public const string PaymentReadTenant = "payment.read.tenant";
        public const string PaymentRefundTenant = "payment.refund.tenant";
        public const string PaymentReconcileTenant = "payment.reconcile.tenant";
        public const string PaymentReadOrganization = "payment.read.organization";

        // Policy
        public const string PolicyReadOwn = "policy.read.own";
        public const string PolicyIssueAssigned = "policy.issue.assigned";
        public const string PolicyReadAssigned = "policy.read.assigned";
        public const string PolicyUpdateAssigned = "policy.update.assigned";
        public const string PolicyRenewAssigned = "policy.renew.assigned";
        public const string PolicyReadTenant = "policy.read.tenant";
        public const string PolicyIssueTenant = "policy.issue.tenant";
        public const string PolicyCancelTenant = "policy.cancel.tenant";

        // Claim
        public const string ClaimSubmitOwn = "claim.submit.own";
        public const string ClaimReadOwn = "claim.read.own";
        public const string ClaimDocumentUploadOwn = "claim.document.upload.own";
        public const string ClaimReadAssigned = "claim.read.assigned";
        public const string ClaimReadTenant = "claim.read.tenant";
        public const string ClaimReviewTenant = "claim.review.tenant";
        public const string ClaimApproveTenant = "claim.approve.tenant";
        public const string ClaimSettleTenant = "claim.settle.tenant";
        public const string ClaimAssignTenant = "claim.assign.tenant";
        public const string ClaimReadOrganization = "claim.read.organization";
        public const string ClaimSubmitOrganization = "claim.submit.organization";
        public const string ClaimStatusUpdateOrganization = "claim.status.update.organization";

        // Dashboard
        public const string DashboardPersonalViewOwn = "dashboard.personal.view.own";
        public const string DashboardAgentViewAssigned = "dashboard.agent.view.assigned";
        public const string DashboardOperationsViewTenant = "dashboard.operations.view.tenant";
        public const string DashboardProviderViewOrganization = "dashboard.provider.view.organization";
        public const string DashboardAdminViewAny = "dashboard.admin.view.any";

        public static readonly IReadOnlyCollection<string> All = typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();
    }
}
