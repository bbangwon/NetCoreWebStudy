using Microsoft.AspNetCore.Mvc;

namespace NetCore.Web.Controllers
{
    public class MembershipController : Controller
    {
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
        public IActionResult Login(Models.LoginInfo loginInfo)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                string userId = "breadone";
                string password = "123456";

                if(loginInfo.UserId.Equals(userId) && loginInfo.Password.Equals(password))
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
