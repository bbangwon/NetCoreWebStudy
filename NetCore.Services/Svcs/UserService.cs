using NetCore.Data.DataModels;
using NetCore.Data.ViewModels;
using NetCore.Services.Interfaces;

namespace NetCore.Services.Svcs
{
    public class UserService : IUser
    {
        #region Private Methods
        private IEnumerable<User> GetUserInfos()
        {
            return new List<User>()
            {
                new User()
                {
                    UserId = "breadone",
                    UserName = "빵원",
                    UserEmail = "bbangwon.chung@gmail.com",
                    Password = "123456"
                }
            };
        }

        private bool CheckTheUserInfo(string userId, string password)
        {
            return GetUserInfos().Where(u => u.UserId.Equals(userId) && u.Password.Equals(password)).Any();
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
