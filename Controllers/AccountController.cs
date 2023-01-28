using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Data;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using System;

namespace RunGroopWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ApplicationDBContext _context;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ApplicationDBContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _context = context;
        }

        public IActionResult Login()
        {
            var response = new LoginViewModel();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (!ModelState.IsValid) return View(loginModel);

            var user = await userManager.FindByEmailAsync(loginModel.EmailAddress);

            if (user != null)
            {
                var passwordCheck = await userManager.CheckPasswordAsync(user, loginModel.Password);
                if (passwordCheck)
                {
                    var result = await signInManager.PasswordSignInAsync(user, loginModel.Password, false, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Race");
                    }
                    TempData["Error"] = "Wrong credentials. Please try again";
                    return View(loginModel);
                }
            }
            TempData["Error"] = "Wrong credentials. Please try again";
            return View(loginModel);
        }

        public IActionResult Register()
        {
            var response = new RegisterViewModel();
            return View(response);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerModel)
        {
            if (!ModelState.IsValid) return View(registerModel);

            var user = await userManager.FindByEmailAsync(registerModel.EmailAddress);
            if (user != null)
            {
                TempData["Error"] = "This email address is already in use";
                return View(registerModel);
            }
            var newUser = new AppUser
            {
                Email = registerModel.EmailAddress,
                UserName = registerModel.EmailAddress
            };
            var newUserResponse = await userManager.CreateAsync(newUser,registerModel.Password);

            if (newUserResponse.Succeeded) 
                await userManager.AddToRoleAsync(newUser, UserRoles.User);

            return RedirectToAction("Index", "Race");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Race");
        }
    }
}
