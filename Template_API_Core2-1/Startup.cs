using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using Template_API_Core2_1.Interfaces;
using Template_API_Core2_1.Options;
using Template_API_Core2_1.Services;

namespace Template_API_Core2_1
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            // Get the JwtOptions section of the appsettings.json
            IConfigurationSection jwtOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // Set up the signing key for the token
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtIssuerOptions:SecretKey"]));

            // Configure the token settings
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtOptions["Issuer"];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            // Ensure we have something for ClockSkew
            double.TryParse(jwtOptions["ClockSkew"], out double clock);
            if (0.0D >= clock)
            {
                clock = 5;
            }

            // Set all Audiences (maybe one, can be more), audiences are defined as a comma seperated string eg. "audience1,audience2,audience3"
            string audienceValues = jwtOptions["Audience"];
            IEnumerable<string> audiences;

            if (audienceValues.Contains(','))
            {
                audiences = audienceValues.Split(',');
            }
            else
            {
                audiences = new List<string>() { audienceValues };
            }

            // With the options all in place set up the Authentication and validation rules
            services.AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions["Issuer"],

                        ValidateAudience = true,
                        ValidAudiences = audiences,

                        RequireExpirationTime = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(clock),
                    };
                });

            #region Dependancy Injection
            services.TryAddSingleton<ITokenService, TokenService>();
            #endregion

            // Finally setup the MVC API with nice JSON formatting
            services.AddMvc().AddJsonOptions(options => { options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Ensure the App knows to use Authentication
            app.UseAuthentication();

            // Add the route information
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "api/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
