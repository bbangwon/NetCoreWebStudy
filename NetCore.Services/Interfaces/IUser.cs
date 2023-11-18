using NetCore.Data.DataModels;
using NetCore.Data.ViewModels;

namespace NetCore.Services.Interfaces
{
    public interface IUser
    {
        bool MatchTheUserInfo(LoginInfo loginInfo);
        User? GetUserInfo(string userId);
        IEnumerable<UserRolesByUser> GetRolesOwnedByUser(string userId);

        /// <summary>
        /// [사용자 가입]
        /// </summary>
        /// <param name="register">사용자 가입용 뷰모델</param>
        /// <returns></returns>
        int RegisterUser(RegisterInfo register);
    }
}
