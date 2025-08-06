using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelClasses;
using ModelClasses.ViewModel;
using Stripe;

namespace GreenOasisAll.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> MyAccount()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = new MyAccountVM
            {
                ApplicationUser = (ApplicationUser)user,
                StatusMessage = "Thông tin tài khoản của bạn"
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            LoginVM vm = new LoginVM();
            ViewData["returnUrl"] = returnUrl;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    login.LoginStatus = "Login successfull. Thank you!";
                    if (returnUrl != null && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            login.LoginStatus = "Unsuccessful login. Please try again";

            return View(login);
        }

        public IActionResult Register()
        {
            RegisterVM vm = new RegisterVM();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM register)
        {
            var user = new ApplicationUser
            {
                FirstName = register.applicationUser.FirstName,
                LastName = register.applicationUser.LastName,
                Email = register.Email,
                UserName = register.UserName,
                Address = register.applicationUser.Address,
                City = register.applicationUser.City,
            };

            var Registration = await _userManager.CreateAsync(user, register.Password);

            if (Registration.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                register.StatusMessage = "Register successfully";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                register.StatusMessage = "Unsuccessful to register";
            }
            return View(register);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }



        //[HttpGet]
        //public IActionResult Login(string returnUrl = null)
        //{
        //    LoginVM vm = new LoginVM();
        //    ViewData["returnUrl"] = returnUrl;
        //    return View(vm);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Login(LoginVM login, string returnUrl = null)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: true);
        //        if (result.Succeeded)
        //        {
        //            login.LoginStatus = "Login successfull. Thank you!";
        //            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
        //            {
        //                return Redirect(returnUrl);
        //            }
        //            else
        //            {
        //                return RedirectToAction("Index", "Home");
        //            }
        //        }
        //    }
            
        //    login.LoginStatus = "Unsuccessful login. Please try again";
           
        //    return View(login);
        //}

        //public IActionResult Register()
        //{
        //    RegisterVM vm = new RegisterVM();
        //    return View(vm);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Register(RegisterVM register)
        //{
        //    var user = new ApplicationUser
        //    {
        //        FirstName = register.applicationUser.FirstName,
        //        LastName = register.applicationUser.LastName,
        //        Email = register.Email,
        //        UserName = register.UserName,
        //        Address = register.applicationUser.Address,
        //        City = register.applicationUser.City,
        //    };

        //    var Registration = await _userManager.CreateAsync(user, register.Password);

        //    if (Registration.Succeeded)
        //    {
        //        await _signInManager.SignInAsync(user, isPersistent: false);
        //        register.StatusMessage = "Register successfully";
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        register.StatusMessage = "Unsuccessful to register";
        //    }
        //    return View(register);
        //}

        //public async Task<IActionResult> Logout()
        //{
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction("Index", "Home");
        //}

    }
}

