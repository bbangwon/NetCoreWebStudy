using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NetCore.Data.ViewModels;
using NetCore.Services.Interfaces;
using System.Security.Claims;

namespace NetCore.Web.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Roles = "AssociateUser, GeneralUser, SuperUser, SystemUser")]
    public class MembershipController : Controller
    {
        private IUser _user;
        private IPasswordHasher _hasher;
        private HttpContext? _context;

        public MembershipController(IHttpContextAccessor accessor, IPasswordHasher hasher, IUser user)
        {
            _context = accessor.HttpContext;
            _hasher = hasher;
            _user = user;
        }

        #region private methods

        /// <summary>
        /// 로컬URL인지 외부URL인지 체크
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if(Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Index), "Membership");
            }
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(RegisterInfo register, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            string message = string.Empty;
            if(ModelState.IsValid)
            {
                //사용자 가입 서비스
                if(_user.RegisterUser(register) > 0)
                {
                    TempData["Message"] = "사용자 가입이 성공적으로 이루어졌습니다.";
                    return RedirectToAction("Login", "Membership");
                }
                else
                {
                    message = "사용자가 가입되지 않았습니다.";
                }
            }
            else
            {
                message = "사용자 가입을 위한 정보를 올바르게 입력하세요.";
            }

            ModelState.AddModelError(string.Empty, message);
            return View();
        }

        [HttpPost]        
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        //위조방지토큰을 통해 View로부터 받은 Post Data가 유효한지 검증합니다.
        public async Task<IActionResult> LoginAsync(LoginInfo loginInfo, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            string message = string.Empty;
            if (ModelState.IsValid)
            {
                if (_user.MatchTheUserInfo(loginInfo))
                {
                    //신원보증과 승인권한
                    var userInfo = _user.GetUserInfo(loginInfo.UserId!);
                    var roles = _user.GetRolesOwnedByUser(loginInfo.UserId!);
                    var userTopRole = roles.FirstOrDefault();   //권한

                    string userDataInfo = userTopRole!.UserRole!.RoleName + "|" +
                        userTopRole!.UserRole.RolePriority.ToString() + "|" +
                        userInfo!.UserName + "|" +
                        userInfo!.UserEmail;

                    //신원보증
                    var identity = new ClaimsIdentity(claims: new[]
                    {
                        new Claim(type: ClaimTypes.Name, value: userInfo!.UserId),
                        new Claim(type: ClaimTypes.Role, value: userTopRole!.RoleId),
                        new Claim(type: ClaimTypes.UserData, value: userDataInfo)
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
                    return RedirectToLocal(returnUrl);
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

        [HttpGet]
        public IActionResult UpdateInfo()
        {
            UserInfo user = _user.GetUserInfoForUpdate(_context!.User.Identity!.Name!);   //서비스

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateInfo(UserInfo user)
        {
            string message = string.Empty;
            if(ModelState.IsValid)
            {
                //변경대상 값들을 비교 서비스
                if(_user.CompareInfo(user))
                {
                    message = "하나 이상의 값이 변경되어야 정보수정이 가능합니다.";
                    ModelState.AddModelError(string.Empty, message);
                    return View(user);
                }

                //정보수정 서비스
                if(_user.UpdateUser(user) > 0)
                {
                    TempData["Message"] = "사용자 정보수정이 성공적으로 이루어졌습니다.";
                    return RedirectToAction("UpdateInfo", "Membership");
                }
                else
                {
                    message = "사용자 정보가 수정되지 않았습니다.";
                }

            }
            else
            {
                message = "사용자 정보수정을 위한 정보를 올바르게 입력하세요.";
            }

            ModelState.AddModelError(string.Empty, message);
            return View(user);
        }



        public async Task<IActionResult> LogoutAsync()
        {
            await _context!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "로그아웃에 성공했습니다. <br />웹사이트를 원할하게 이용하시려면 로그인하세요.";
            return RedirectToAction("Index", "Membership");
        }

        [HttpGet]
        public IActionResult Forbidden()
        {
            StringValues paramReturnUrl;
            bool exists = _context!.Request.Query.TryGetValue("ReturnUrl", out paramReturnUrl);
            paramReturnUrl = exists ? _context.Request.Host.Value + paramReturnUrl[0] : string.Empty;

            ViewData["Message"] = $"귀하는 {paramReturnUrl} 경로로 접근하려고 했습니다만, <br />" +
                "인증된 사용자도 접근하지 못하는 페이지가 있습니다. <br />" +
                "담당자에게 해당페이지의 접근권한에 대해 문의하세요.";

            return View();
        }
    }

}
