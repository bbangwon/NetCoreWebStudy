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

            //FUNCTION
            user = _dbContext.Users
                .FromSql($"SELECT * FROM dbo.[ufnUser]({userId}, {password})")
                .FirstOrDefault();

            //STORED PROCEDURE
            //user = _dbContext.Users
            //    .FromSql($"dbo.uspCheckLoginByUserId {userId}, {password}")
            //    .AsEnumerable()
            //    .FirstOrDefault();

            return user;
        }

        private bool CheckTheUserInfo(string userId, string password)
        {
            return GetUesrInfo(userId, password) != null;
        }
        #endregion

        bool IUser.MatchTheUserInfo(LoginInfo loginInfo)
        {
            if (string.IsNullOrEmpty(loginInfo.UserId) || string.IsNullOrEmpty(loginInfo.Password))
                return false;

            return CheckTheUserInfo(loginInfo.UserId, loginInfo.Password);
        }
    }
}
