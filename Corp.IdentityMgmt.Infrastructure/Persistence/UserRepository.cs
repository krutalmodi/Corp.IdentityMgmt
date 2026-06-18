using Corp.IdentityMgmt.Application.Interfaces;
using Corp.IdentityMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Corp.IdentityMgmt.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        #region Members
        
        private readonly IdentityDbContext _context;

        #endregion

        #region Constructor
        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        #endregion

        #region UserRepository Implementation

        public Task<UserIdentity?> GetUserByIdAsync(Guid userId) => _context.User.FindAsync(userId).AsTask();

        public Task<UserIdentity?> GetUserByEmailAsync(Guid tenantId, string email) => 
            _context.User.FirstOrDefaultAsync(x => x.Email == email && x.TenantId == tenantId);

        public async Task AddUserAsync(UserIdentity user)
        {
            _context.User.Add(user);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region CredentialRepository Implementation

        public async Task AddCredentialAsync(Credential credential)
        {
            _context.Credential.Add(credential);
            await _context.SaveChangesAsync();
        }

        public Task<Credential?> GetCredentialByUserIdAsync(Guid userId) =>
            _context.Credential.FirstOrDefaultAsync(x => x.UserId == userId);

        #endregion

        #region ExternalIdentityRepository Implementation

        public Task<ExternalIdentity?> GetExternalIdentityByProviderAsync(Guid userId, string provider) =>
            _context.ExternalIdentity.FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider);   

        public async Task AddExternalIdentityAsync(ExternalIdentity externalIdentity)
        {
            _context.ExternalIdentity.Add(externalIdentity);
            await _context.SaveChangesAsync();
        }

        #endregion

    }
}
