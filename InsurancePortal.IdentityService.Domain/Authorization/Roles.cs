namespace InsurancePortal.IdentityService.Domain.Authorization
{
    public static class Roles
    {
        public const string Guest = "Guest";
        public const string Registered = "Registered";
        public const string Admin = "Admin";
        public const string Associates = "Associates";
        public const string Agents = "Agents";
        public const string ProvidersAdmin = "Providers Admin";
        public const string ProvidersStaff = "Providers Staff";

        public static readonly IReadOnlyCollection<string> All = new[]
        {
            Guest, Registered, Admin, Associates, Agents, ProvidersAdmin, ProvidersStaff
        };
    }
}
