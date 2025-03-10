﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCClient {
    public class CookieEventHandler: CookieAuthenticationEvents {
        public CookieEventHandler(LogoutUserManager logoutSessions) {
            LogoutSessions = logoutSessions;
        }

        public LogoutUserManager LogoutSessions { get; }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context) {
            if (context.Principal.Identity.IsAuthenticated) {
                var sub = context.Principal.FindFirst("sub")?.Value;
                var sid = context.Principal.FindFirst("sid")?.Value;

                if (LogoutSessions.IsLoggedOut(sub, sid)) {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync();

                    // todo: if we have a refresh token, it should be revoked here.
                }
            }
        }
    }
}
