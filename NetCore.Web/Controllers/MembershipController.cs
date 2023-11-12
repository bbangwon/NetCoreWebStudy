using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NetCore.Data.ViewModels;
using NetCore.Services.Interfaces;
using System.Security.Claims;

namespace NetCore.Web.Controllers
{
    public class MembershipController : Controller
    {
        private IUser _user;
        private HttpContext? _context;

        public MembershipController(IHttpContextAccessor accessor, IUser user)
        {
            _context = accessor.HttpContext;
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
        public async Task<IActionResult> LoginAsync(LoginInfo loginInfo)
        {
            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if(_user.MatchTheUserInfo(loginInfo))
                {
                    //신원보증과 승인권한
                    var userInfo = _user.GetUserInfo(loginInfo.UserId!);
                    var roles = _user.GetRolesOwnedByUser(loginInfo.UserId!);
                    var userTopRole = roles.FirstOrDefault();   //권한

                    //신원보증
                    var identity = new ClaimsIdentity(claims: new[]
                    {
                        new Claim(type: ClaimTypes.Name, value: userInfo!.UserName),
                        new Claim(type: ClaimTypes.Role, value: userTopRole!.RoleId + "|" + userTopRole.UserRole!.RoleName + "|" + userTopRole.UserRole.RolePriority.ToString())
                    }, authenticationType: CookieAuthenticationDefaults.AuthenticationScheme);

                    await _context!.SignInAsync(scheme: CookieAuthenticationDefaults.AuthenticationScheme,
                                        principal: new ClaimsPrincipal(identity: identity), //본인
                                        properties: new AuthenticationProperties
                                        {
                                            //지속여부
                                            IsPersistent = loginInfo.RememberMe,
                                            //만료
                                            ExpiresUtc = loginInfo.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(30)
                                        }
                        );

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
            return View("Login", loginInfo);
        }

        public async Task<IActionResult> LogoutAsync()
        {
            await _context!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "로그아웃에 성공했습니다. <br />웹사이트를 원할하게 이용하시려면 로그인하세요.";
            return RedirectToAction("Index", "Membership");
        }
    }

}
