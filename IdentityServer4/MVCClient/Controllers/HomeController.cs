using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MVCClient.Models;
using Newtonsoft.Json.Linq;

namespace MVCClient.Controllers {
    public class HomeController : Controller {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDiscoveryCache _discoveryCache;

        public HomeController(IHttpClientFactory httpClientFactory, IDiscoveryCache discoveryCache) {
            _httpClientFactory = httpClientFactory;
            _discoveryCache = discoveryCache;
        }

        public IActionResult Index() {
            return View();
        }

        [Authorize]
        public IActionResult Secure() {
            ViewData["Message"] = "Secure page.";

            return View();
        }



        public IActionResult Logout() {
            return new SignOutResult(new[] { "Cookies", "oidc" });
        }

        //public async Task Logout() {
        //    await HttpContext.SignOutAsync("Cookies");
        //    await HttpContext.SignOutAsync("oidc");
        //    //return new SignOutResult(new[] { "Cookies", "oidc" });
        //}

        //public IActionResult Logout() {
        //    SignOut("Cookies", "oidc");
        //    return Redirect("http://localhost:5000/Logoff");
        //}



        //public async Task<IActionResult> FrontChannelLogout(string sid) {
        //    if (User.Identity.IsAuthenticated) {
        //        var currentSid = User.FindFirst("sid")?.Value ?? "";
        //        if (string.Equals(currentSid, sid, StringComparison.Ordinal)) {
        //            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //        }
        //    }

        //    return NoContent();
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> BackChannelLogout(string logout_token) {
        //    Response.Headers.Add("Cache-Control", "no-cache, no-store");
        //    Response.Headers.Add("Pragma", "no-cache");

        //    try {
        //        var user = await ValidateLogoutToken(logout_token);

        //        // these are the sub & sid to signout
        //        var sub = user.FindFirst("sub")?.Value;
        //        var sid = user.FindFirst("sid")?.Value;

        //        return Ok();
        //    } catch { }

        //    return BadRequest();
        //}

        //private async Task<ClaimsPrincipal> ValidateLogoutToken(string logoutToken) {
        //    var claims = await ValidateJwt(logoutToken);

        //    if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null) throw new Exception("Invalid logout token");

        //    var nonce = claims.FindFirstValue("nonce");
        //    if (!String.IsNullOrWhiteSpace(nonce)) throw new Exception("Invalid logout token");

        //    var eventsJson = claims.FindFirst("events")?.Value;
        //    if (String.IsNullOrWhiteSpace(eventsJson)) throw new Exception("Invalid logout token");

        //    var events = JObject.Parse(eventsJson);
        //    var logoutEvent = events.TryGetValue("http://schemas.openid.net/event/backchannel-logout");
        //    if (logoutEvent == null) throw new Exception("Invalid logout token");

        //    return claims;
        //}

        //private static async Task<ClaimsPrincipal> ValidateJwt(string jwt) {
        //    // read discovery document to find issuer and key material
        //    var client = new HttpClient();
        //    var disco = await client.GetDiscoveryDocumentAsync("http://localhost:5000/.well-known/openid-configuration");

        //    var keys = new List<SecurityKey>();
        //    foreach (var webKey in disco.KeySet.Keys) {
        //        var e = Base64Url.Decode(webKey.E);
        //        var n = Base64Url.Decode(webKey.N);

        //        var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n }) {
        //            KeyId = webKey.Kid
        //        };

        //        keys.Add(key);
        //    }

        //    var parameters = new TokenValidationParameters {
        //        ValidIssuer = disco.Issuer,
        //        ValidAudience = "mvc.manual",
        //        IssuerSigningKeys = keys,

        //        NameClaimType = JwtClaimTypes.Name,
        //        RoleClaimType = JwtClaimTypes.Role,

        //        RequireSignedTokens = true
        //    };

        //    var handler = new JwtSecurityTokenHandler();
        //    handler.InboundClaimTypeMap.Clear();

        //    var user = handler.ValidateToken(jwt, parameters, out var _);
        //    return user;
        //}

        public IActionResult Error() {
            return View();
        }
        public async Task<IActionResult> RenewTokens() {
            var disco = await _discoveryCache.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            var rt = await HttpContext.GetTokenAsync("refresh_token");
            var tokenClient = _httpClientFactory.CreateClient();

            var tokenResult = await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = "mvc.hybrid",
                ClientSecret = "secret",
                RefreshToken = rt
            });

            if (!tokenResult.IsError) {
                var old_id_token = await HttpContext.GetTokenAsync("id_token");
                var new_access_token = tokenResult.AccessToken;
                var new_refresh_token = tokenResult.RefreshToken;
                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);

                var info = await HttpContext.AuthenticateAsync("Cookies");

                info.Properties.UpdateTokenValue("refresh_token", new_refresh_token);
                info.Properties.UpdateTokenValue("access_token", new_access_token);
                info.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));

                await HttpContext.SignInAsync("Cookies", info.Principal, info.Properties);
                return Redirect("~/Home/Secure");
            }

            ViewData["Error"] = tokenResult.Error;
            return View("Error");
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

        [Authorize]
        public async Task<ActionResult<string>> CallTest() {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var content = await client.GetStringAsync("http://localhost:5001/test");
            ViewBag.data = content;
            return View();
        }

    }
}
