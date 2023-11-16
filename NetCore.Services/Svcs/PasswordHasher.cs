using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NetCore.Services.Data;
using NetCore.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NetCore.Services.Svcs
{
    public class PasswordHasher : IPasswordHasher
    {
        private CodeFirstDbContext _context;

        public PasswordHasher(CodeFirstDbContext context)
        {
            _context = context;
        }

        #region Private Methods
        private string GetGUIDSalt()
        {
            return System.Guid.NewGuid().ToString();
        }

        private string GetRNGSalt()
        {
            // generate a 128-bit salt using a secure PRNG
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        private string GetPasswordHash(string userId, string password, string guidSalt, string rngSalt)
        {
            // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: userId + password + guidSalt,
                salt: Encoding.UTF8.GetBytes(rngSalt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 45000,
                numBytesRequested: 256 / 8));
        }

        private bool CheckThePasswordInfo(string userId, string password, string guidSalt, string rngSalt, string passwordHash)
        {
            return GetPasswordHash(userId, password, guidSalt, rngSalt).Equals(passwordHash);
        }
        #endregion

        string IPasswordHasher.GetGUIDSalt()
        {
            return GetGUIDSalt();
        }

        string IPasswordHasher.GetRNGSalt()
        {
            return GetRNGSalt();
        }

        string IPasswordHasher.GetPasswordHash(string userId, string password, string guidSalt, string rngSalt)
        {
            return GetPasswordHash(userId, password, guidSalt, rngSalt);
        }

        public bool MatchThePasswordInfo(string userId, string password)
        {
            var user = _context.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault();
            if(user == null)
                return false;

            string guidSalt = user.GUIDSalt;
            string rngSalt = user.RNGSalt;
            string passwordHash = user.PasswordHash;

            return CheckThePasswordInfo(userId, password, guidSalt, rngSalt, passwordHash);
        }

    }
}
