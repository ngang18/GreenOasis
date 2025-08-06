using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelClasses.ViewModel;
using ModelClasses;

namespace GreenOasisAll.Controllers
{
    public class ExternalAuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public ExternalAuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> SelectProvider()
        {
            var externalloginVM = new ExternalLoginVM()
            {
                Schemes = await _signInManager.GetExternalAuthenticationSchemesAsync()
            };
            return View(externalloginVM);
        }
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl = "")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "ExternalAuth", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "", string remoteError = "")
        {
            var externalloginVM = new ExternalLoginVM()
            {
                Schemes = await _signInManager.GetExternalAuthenticationSchemesAsync()
            };

            if (!string.IsNullOrEmpty(remoteError))
            {
                ModelState.AddModelError("", $"Error from external provider: {remoteError}");
                return RedirectToAction("Login", "Account");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true
            );

            if (result.Succeeded)
            {
                return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        ModelState.AddModelError("", "Error creating user");
                        return RedirectToAction("Login", "Account");
                    }
                }

                // Liên kết login ngoài
                await _userManager.AddLoginAsync(user, info);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
            }

            ModelState.AddModelError("", "Something went wrong");
            return RedirectToAction("Login", "Account");
        }
    }
}
