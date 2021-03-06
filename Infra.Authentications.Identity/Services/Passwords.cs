﻿using Infra.Logs;
using Infra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra.Events;

namespace Infra.Authentications.Identity.Services
{
    public class Passwords : IPasswords
    {
        public async Task ChangeAsync(string userId, string currentPassword, string newPassword)
        {
            var result = await IdentityManagers.UserManager.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));                    
        }

        public async Task<string> CreateAsync(string userId)
        {
            var user = new AuthenticationUser
            {
                Id = userId,
                UserName = userId
            };

            string password = new RandomString(8);
            var result = await IdentityManagers.UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors));

            return password;
        }

        public async Task<string> CreateTokenAsync(string userId)
        {
            var user = await IdentityManagers.GetOrCreateAsync(userId);
            return await IdentityManagers.UserManager.GeneratePasswordResetTokenAsync(userId);            
        }

        public async Task ResetAsync(string userId, string token, string password)
        {
            var result = await IdentityManagers.UserManager.ResetPasswordAsync(userId, token, password);
            if (!result.Succeeded)
                throw new ResetPasswordException(string.Join(", ", result.Errors));
        }
    }
}
