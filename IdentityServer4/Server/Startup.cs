using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;

namespace Server {
    public class Startup {
        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment environment) {
            Environment = environment;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

       
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=C:\\Users\\jason\\IdentityServer4_Test.db"));


            //services.AddDefaultIdentity<ApplicationUser>()
            //                .AddDefaultUI(UIFramework.Bootstrap4)
            //                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultUI(UIFramework.Bootstrap4)
               .AddDefaultTokenProviders();


            var builder = services.AddIdentityServer()
               .AddInMemoryIdentityResources(Config.GetIdentityResources())
               .AddInMemoryApiResources(Config.GetApis())
               .AddInMemoryClients(Config.GetClients())
               //.AddTestUsers(Config.GetUsers());
               .AddAspNetIdentity<ApplicationUser>();


            if (Environment.IsDevelopment()) {
                builder.AddDeveloperSigningCredential();
            } else {
                throw new Exception("need to configure key material");
            }

           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
