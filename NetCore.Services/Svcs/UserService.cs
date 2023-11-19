using Microsoft.EntityFrameworkCore;
using NetCore.Data.DataModels;
using NetCore.Data.ViewModels;
using NetCore.Services.Data;
using NetCore.Services.Interfaces;

namespace NetCore.Services.Svcs
{
    public class UserService : IUser
    {
        CodeFirstDbContext _dbContext;
        IPasswordHasher _hasher;

        public UserService(CodeFirstDbContext context, IPasswordHasher hasher)
        {
            _dbContext = context;
            _hasher = hasher;
        }

        #region Private Methods
        private IEnumerable<User> GetUserInfos()
        {
            return _dbContext.Users.ToList();
            //return new List<User>()
            //{
            //    new User()
            //    {
            //        UserId = "breadone",
            //        UserName = "빵원",
            //        UserEmail = "bbangwon.chung@gmail.com",
            //        Password = "123456"
            //    }
            //};
        }

        private User? GetUesrInfo(string userId, string password)
        {
            User? user;

            //Lambda Expression
            //user = _dbContext.Users.Where(u => u.UserId!.Equals(userId) && u.Password!.Equals(password)).FirstOrDefault();

            //SQL
            //user = _dbContext.Users
            //    .FromSql($"SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate FROM dbo.[User]")
            //    .Where(u => u.UserId!.Equals(userId) && u.Password!.Equals(password))
            //    .FirstOrDefault();

            //user = _dbContext.Users
            //    .FromSql($"SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate FROM dbo.[User] WHERE UserId = {userId} AND Password = {password}")
            //    .FirstOrDefault();

            //VIEW
            //user = _dbContext.Users
            //    .FromSql($"SELECT UserId, UserName, UserEmail, Password, IsMembershipWithdrawn, JoinedUtcDate FROM dbo.[uvwUser]")
            //    .Where(u => u.UserId!.Equals(userId) && u.Password!.Equals(password))
            //    .FirstOrDefault();

            ////FUNCTION
            //user = _dbContext.Users
            //    .FromSql($"SELECT * FROM dbo.[ufnUser]({userId}, {password})")
            //    .FirstOrDefault();

            //STORED PROCEDURE
            user = _dbContext.Users
                .FromSql($"dbo.uspCheckLoginByUserId {userId}, {password}")
                .AsEnumerable()
                .FirstOrDefault();

            if(user == null)
            {
                int rowAffected = 0;
                //rowAffected = _dbContext.Database.ExecuteSql($"Update dbo.[User] SET AccessFailedCount += 1 WHERE UserId={userId}");

                //STORE PROCEDURE
                rowAffected = _dbContext.Database.ExecuteSql($"dbo.FailedLoginByUserId {userId}");
            }

            return user;
        }

        private bool CheckTheUserInfo(string userId, string password)
        {
            return GetUesrInfo(userId, password) != null;
        }

        private User? GetUserInfo(string userId)
        {
            return _dbContext.Users.Where(u => u.UserId!.Equals(userId)).FirstOrDefault();
        }

        private IEnumerable<UserRolesByUser> GetUserRolesByUserInfos(string userId)
        {
            var userRolesByUserInfos = _dbContext.UserRolesByUsers.Where(u => u.UserId!.Equals(userId)).ToList();

            foreach (var role in userRolesByUserInfos)
            {
                role.UserRole = GetUserRole(role.RoleId!);
            }

            return userRolesByUserInfos.OrderByDescending(uru => uru.UserRole!.RolePriority);
        }

        private UserRole? GetUserRole(string roleId)
        {
            return _dbContext.UserRoles.Where(r => r.RoleId!.Equals(roleId)).FirstOrDefault();
        }

        //아이디에 대해서 대소문자 처리
        private int RegisterUser(RegisterInfo register)
        {
            var utcNow = DateTime.UtcNow;
            var passwordInfo = _hasher.SetPasswordInfo(register.UserId, register.Password);

            var user = new User()
            {
                UserId = register.UserId.ToLower(),
                UserName = register.UserName,
                UserEmail = register.UserEmail,
                GUIDSalt = passwordInfo.GUIDSalt,
                RNGSalt = passwordInfo.RNGSalt,
                PasswordHash = passwordInfo.PasswordHash,
                AccessFailedCount = 0,
                IsMembershipWithdrawn = false,
                JoinedUtcDate = utcNow
            };

            var userRolesByUser = new UserRolesByUser()
            {
                UserId = register.UserId.ToLower(),
                RoleId = "AssociateUser",
                OwnedUtcDate = utcNow
            };

            _dbContext.Add(user);
            _dbContext.Add(userRolesByUser);

            return _dbContext.SaveChanges();
        }

        private UserInfo GetUserInfoForUpdate(string userId)
        {
            var user = GetUserInfo(userId);
            var userInfo = new UserInfo()
            {
                UserId = null,
                Password = null,
                UserName = user!.UserName,
                UserEmail = user!.UserEmail,
                ChangeInfo = new ChangeInfo()
                {
                    UserName = user!.UserName,
                    UserEmail = user!.UserEmail
                }
            };

            return userInfo;
        }

        private int UpdateUser(UserInfo user)
        {
            var userInfo = _dbContext.Users.Where(u => u.UserId.Equals(user.UserId)).FirstOrDefault();
            if (userInfo == null)
                return 0;

            bool check = _hasher.CheckThePasswordInfo(user.UserId!, user.Password!, userInfo.GUIDSalt, userInfo.RNGSalt, userInfo.PasswordHash);

            int rowAffected = 0;

            if(check)
            {
                _dbContext.Update(userInfo);

                userInfo.UserName = user.UserName;
                userInfo.UserEmail = user.UserEmail;

                rowAffected = _dbContext.SaveChanges();
            }

            return rowAffected;
        }

        private bool MatchTheUserInfo(LoginInfo loginInfo)
        {
            var user = _dbContext.Users.Where(u => u.UserId.Equals(loginInfo.UserId)).FirstOrDefault();
            if (user == null)
                return false;

            return _hasher.CheckThePasswordInfo(loginInfo.UserId!, loginInfo.Password!, user.GUIDSalt, user.RNGSalt, user.PasswordHash);
        }

        private bool CompareInfo(UserInfo user)
        {
            return user.ChangeInfo!.Equals(user);
        }

        private int WithdrawnUser(WithdrawnInfo user)
        {
            var userInfo = _dbContext.Users.Where(u => u.UserId.Equals(user.UserId)).FirstOrDefault();
            if (userInfo == null)
                return 0;

            bool check = _hasher.CheckThePasswordInfo(user.UserId!, user.Password!, userInfo.GUIDSalt, userInfo.RNGSalt, userInfo.PasswordHash);
            int rowAffected = 0;

            if (check)
            {
                _dbContext.Remove(userInfo);
                rowAffected = _dbContext.SaveChanges();
            }

            return rowAffected;
        }

        #endregion

        bool IUser.MatchTheUserInfo(LoginInfo loginInfo)
        {
            //if (string.IsNullOrEmpty(loginInfo.UserId) || string.IsNullOrEmpty(loginInfo.Password))
            //    return false;

            //return CheckTheUserInfo(loginInfo.UserId, loginInfo.Password);

            return MatchTheUserInfo(loginInfo);
        }
        User? IUser.GetUserInfo(string userId)
        {
            return GetUserInfo(userId);
        }

        IEnumerable<UserRolesByUser> IUser.GetRolesOwnedByUser(string userId)
        {
            return GetUserRolesByUserInfos(userId);
        }

        int IUser.RegisterUser(RegisterInfo register)
        {
            return RegisterUser(register);
        }

        UserInfo IUser.GetUserInfoForUpdate(string userId)
        {
            return GetUserInfoForUpdate(userId);
        }

        int IUser.UpdateUser(UserInfo user)
        {
            return UpdateUser(user);
        }

        bool IUser.CompareInfo(UserInfo user)
        {
            return CompareInfo(user);
        }

        int IUser.WithdrawnUser(WithdrawnInfo user)
        {
            return WithdrawnUser(user);
        }
    }
}
