using ARF.Identite.API.Data;
using ARF.Identity.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

namespace ARF.Identite.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();
            
            //La configuration JWT
            var appSettingsSection = Configuration.GetSection("AppConfiguration");
            services.Configure<AppConfiguration>(appSettingsSection);

            var appConfiguration = appSettingsSection.Get<AppConfiguration>();
            var cle = Encoding.ASCII.GetBytes(appConfiguration.Cle);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.RequireHttpsMetadata = true;
                bearerOptions.SaveToken = true;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(cle),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appConfiguration.Couverture,
                    ValidIssuer = appConfiguration.Emetteur
                };
            });

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("auth1", new OpenApiInfo
                {
                    Title = "ARF Identity API",
                    Description = "Cette API est responsable de l'enregistrement et de la connexion des utilisateurs.",
                    Contact = new OpenApiContact() { Name = "Joseleno", Email = "joselenomoreira@hotmail.com" },
                    License = new OpenApiLicense() { Name = "LGPL-2.0", Url = new Uri("https://opensource.org/licenses/LGPL-2.0") }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/auth1/swagger.json", "auth1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}