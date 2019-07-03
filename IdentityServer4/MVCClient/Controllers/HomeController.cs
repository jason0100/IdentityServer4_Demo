using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCClient.Models;
using Newtonsoft.Json.Linq;

namespace MVCClient.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return View();
        }

        [Authorize]
        public IActionResult Secure() {
            ViewData["Message"] = "Secure page.";

            return View();
        }

    

        public IActionResult Logout() {
            return SignOut("Cookies", "oidc");
        }

        public IActionResult Error() {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> CallApi() {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = await client.GetStringAsync("http://localhost:5001/identity");

            ViewBag.Json = JArray.Parse(content).ToString();
            return View("json");
        }

        public async Task<ActionResult<string>> CallTest() {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            ViewBag.data = await client.GetStringAsync("http://localhost:5001/test");
            return View();
        }

    }
}
