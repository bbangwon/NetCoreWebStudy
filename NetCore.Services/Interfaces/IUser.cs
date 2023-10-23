using NetCore.Data.ViewModels;

namespace NetCore.Services.Interfaces
{
    public interface IUser
    {
        bool MatchTheUserInfo(LoginInfo loginInfo);
    }
}
