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

        public UserService(CodeFirstDbContext context)
        {
            _dbContext = context;            
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
        #endregion

        bool IUser.MatchTheUserInfo(LoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(loginInfo.UserId) || string.IsNullOrEmpty(loginInfo.Password))
                return false;

            return CheckTheUserInfo(loginInfo.UserId, loginInfo.Password);
        }
        User? IUser.GetUserInfo(string userId)
        {
            return GetUserInfo(userId);
        }

        IEnumerable<UserRolesByUser> IUser.GetRolesOwnedByUser(string userId)
        {
            return GetUserRolesByUserInfos(userId);
        }
    }
}
