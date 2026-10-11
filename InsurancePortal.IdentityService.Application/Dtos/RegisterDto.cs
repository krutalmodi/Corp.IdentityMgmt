using System;
using System.Collections.Generic;
using System.Text;

namespace InsurancePortal.IdentityService.Application.DTOs
{
    public record RegisterDto(string Email, string Password);
}
