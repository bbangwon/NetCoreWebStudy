using NetCore.Data.DataModels;
using NetCore.Data.ViewModels;

namespace NetCore.Services.Interfaces
{
    public interface IUser
    {
        bool MatchTheUserInfo(LoginInfo loginInfo);
        User? GetUserInfo(string userId);
        IEnumerable<UserRolesByUser> GetRolesOwnedByUser(string userId);
    }
}
