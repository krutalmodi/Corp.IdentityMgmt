using System;
using System.Collections.Generic;
using System.Text;

namespace InsurancePortal.IdentityService.Application.Interfaces
{
    public interface IPasswordHasher
    {
        PasswordHashResult Hash(string password);
        bool Verify(string password, string hash, string salt);
    }

    public record PasswordHashResult(string Hash, string Salt);
}
