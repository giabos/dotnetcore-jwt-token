using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dncore_auth.Models;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace dncore_auth.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;



        public HomeController(IConfiguration config)
        {
            this._configuration = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [Authorize]
        public IActionResult Protected()
        {
            //Console.WriteLine("1", ClaimsPrincipal.Current);
            //Console.WriteLine("2", ClaimsPrincipal.Current.Claims);

            return Json(User.Claims.Select(c => c.Type + ":" + c.Value).ToList());
        }

         
        [AllowAnonymous]
        [Route("/test")]
        public IActionResult Test()
        {
             return Ok(new { test = _configuration["param1"]});
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("/token")]
        public IActionResult Post([FromBody]LoginViewModel loginViewModel)
        {
            Console.WriteLine("UserName: " + loginViewModel.Username);

            if (ModelState.IsValid)
            {
                //This method returns user id from username and password.
                var userId = GetUserIdFromCredentials(loginViewModel);
                if (userId == -1)
                {
                    return Unauthorized();
                }

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, loginViewModel.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken
                (
                    issuer: _configuration["Issuer"],
                    audience: _configuration["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(60),
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SigningKey"])),
                            SecurityAlgorithms.HmacSha256)
                );

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return BadRequest();
        }

        private int GetUserIdFromCredentials(LoginViewModel loginViewModel)
        {
            return 1;
        }

        public class LoginViewModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}
