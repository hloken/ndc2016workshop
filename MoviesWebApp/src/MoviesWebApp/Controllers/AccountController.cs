﻿using Microsoft.AspNetCore.Mvc;
using MoviesLibrary;
using MoviesWebApp.ViewModels;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;

namespace MoviesWebApp.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly MovieIdentityService _identityService;

        public AccountController(MovieIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.Username == model.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim("sub", model.Username)
                };
                claims.AddRange(_identityService.GetClaimsForUser(model.Username));

                var ci = new ClaimsIdentity(claims, "password", "name", "role" );
                var cp = new ClaimsPrincipal(ci);

                await HttpContext.Authentication.SignInAsync("Cookies", cp);

                if (model.ReturnUrl != null)
                {
                    LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Denied()
        {
            return View();
        }
    }
}
