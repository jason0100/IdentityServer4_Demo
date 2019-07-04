using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using IdentityModel.Client;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MVCClient {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();


            services.AddHttpClient();

            services.AddSingleton<IDiscoveryCache>(r => {
                var factory = r.GetRequiredService<IHttpClientFactory>();
                return new DiscoveryCache("http://localhost:5000", () => factory.CreateClient());
            });

            services.AddTransient<CookieEventHandler>();
            services.AddSingleton<LogoutUserManager>();


            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";

            })
                .AddCookie(options => {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.Name = "mvchybrid";
                    options.EventsType = typeof(CookieEventHandler);
                })
                .AddOpenIdConnect("oidc", options => {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    //options.ClientId = "mvc";
                    //options.ClientSecret = "secret";
                    //options.ResponseType = "code id_token";//which basically means “use hybrid flow"

                    options.ClientId = "js";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("api1");
                    options.Scope.Add("offline_access");

                    options.ClaimActions.MapJsonKey("website", "website");//To keep the website claim in our mvc client identity we need to explicitly map the claim using ClaimActions.

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseDeveloperExceptionPage();
            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
