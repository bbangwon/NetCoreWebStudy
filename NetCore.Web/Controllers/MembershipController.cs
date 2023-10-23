using Microsoft.AspNetCore.Mvc;
using NetCore.Data.ViewModels;
using NetCore.Services.Interfaces;

namespace NetCore.Web.Controllers
{
    public class MembershipController : Controller
    {
        private IUser _user;

        public MembershipController(IUser user)
        {
            _user = user;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]        
        [ValidateAntiForgeryToken]
        //위조방지토큰을 통해 View로부터 받은 Post Data가 유효한지 검증합니다.
        public IActionResult Login(LoginInfo loginInfo)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if(_user.MatchTheUserInfo(loginInfo))
                {
                    TempData["Message"] = "로그인에 성공했습니다.";
                    return RedirectToAction("Index", "Membership");
                }
                else
                {
                    message = "로그인에 실패했습니다.";
                }
            }
            else
            {
                message = "로그인 정보가 올바르지 않습니다.";
            }

            ModelState.AddModelError(string.Empty, message);
            return View(loginInfo);
        }       
    }
}
