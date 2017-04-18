using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CoreIdentityTrialRun.Models;
using Microsoft.AspNetCore.Authorization;

namespace CoreIdentityTrialRun.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //
        // GET: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.RequiresTwoFactor)
            {
                return Ok(true);
            }

            return NotFound();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }

            // If we got this far, something failed, redisplay form
            return NotFound();
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [Route("SendCode")]
        public async Task<IActionResult> SendCode()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return Ok(false);
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            if (string.IsNullOrWhiteSpace(code))
            {
                return Ok(false);
            }

            var message = "Your security code is: " + code;
            //if (model.SelectedProvider == "Email")
            //{
            //    await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            //}
            //else if (model.SelectedProvider == "Phone")
            //{
            //    await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            //}

            return Ok(new { message = message });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [Route("VerifyCode")]
        public async Task<IActionResult> VerifyCode([FromBody] string code)
        {
            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync("Phone", code, false, false);
            if (result.Succeeded)
            {
                return Ok(true);
            }
            if (result.IsLockedOut)
            {
                return Ok("Lockout");
            }
            else
            {
                return Ok(false);
            }
        }
    }
}