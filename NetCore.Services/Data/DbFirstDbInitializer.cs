using NetCore.Data.DataModels;
using NetCore.Services.Interfaces;

namespace NetCore.Services.Data
{
    public class DbFirstDbInitializer
    {
        private CodeFirstDbContext _context;
        private IPasswordHasher _hasher;
        public DbFirstDbInitializer(CodeFirstDbContext context, IPasswordHasher hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        /// <summary>
        /// 초기 데이터를 심는다.
        /// </summary>
        public int PlantSeedData()
        {
            int rowsAffected = 0;
            
            string userId = "breadone";
            string password = "123456";
            var utcNow = DateTime.UtcNow;

            var passwordInfo = _hasher.SetPasswordInfo(userId, password);

            _context.Database.EnsureCreated();

            if(!_context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        UserId = userId.ToLower(),
                        UserName = "관리자",
                        UserEmail = "bbangwon.chung@gmail.com",
                        GUIDSalt = passwordInfo.GUIDSalt,
                        RNGSalt = passwordInfo.RNGSalt,
                        PasswordHash = passwordInfo.PasswordHash,
                        AccessFailedCount = 0,
                        IsMembershipWithdrawn = false,
                        JoinedUtcDate = utcNow
                    }
                };

                _context.Users.AddRange(users);
                rowsAffected += _context.SaveChanges();
            }

            if (!_context.UserRoles.Any())
            {
                var userRoles = new List<UserRole>()
                {
                    new UserRole
                    {
                        RoleId = "AssociateUser",
                        RoleName = "준사용자",
                        RolePriority = 1,
                        ModifiedUtcDate = utcNow
                    },
                    new UserRole
                    {
                        RoleId = "GeneralUser",
                        RoleName = "일반사용자",
                        RolePriority = 2,
                        ModifiedUtcDate = utcNow
                    },
                    new UserRole
                    {
                        RoleId = "SuperUser",
                        RoleName = "향상된 사용자",
                        RolePriority = 3,
                        ModifiedUtcDate = utcNow
                    },
                    new UserRole
                    {
                        RoleId = "SystemUser",
                        RoleName = "시스템 사용자",
                        RolePriority = 4,
                        ModifiedUtcDate = utcNow
                    },
                };

                _context.UserRoles.AddRange(userRoles);
                rowsAffected += _context.SaveChanges();
            }

            if (!_context.UserRolesByUsers.Any())
            {
                var userRolesByUsers = new List<UserRolesByUser>()
                {
                    new UserRolesByUser
                    {
                        UserId = userId.ToLower(),
                        RoleId = "GeneralUser",
                        OwnedUtcDate = utcNow
                    },
                    new UserRolesByUser
                    {
                        UserId = userId.ToLower(),
                        RoleId = "SuperUser",
                        OwnedUtcDate = utcNow
                    },
                    new UserRolesByUser
                    {
                        UserId = userId.ToLower(),
                        RoleId = "SystemUser",
                        OwnedUtcDate = utcNow
                    },
                };

                _context.UserRolesByUsers.AddRange(userRolesByUsers);
                rowsAffected += _context.SaveChanges();
            }

            return rowsAffected;
        }
    }
}
