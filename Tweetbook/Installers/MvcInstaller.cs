using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Authorization;
using Tweetbook.Filter;
using Tweetbook.Options;
using Tweetbook.Services;

namespace Tweetbook.Installers
{
    public class MvcInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            services.AddScoped<IIdentityService, IdentityService>();

            services
                .AddMvc(options => 
                {
                    options.Filters.Add<ValidationFilter>();
                })
                // регистрируем Плавную проверку
                .AddFluentValidation(mvcConfiguration => mvcConfiguration.RegisterValidatorsFromAssemblyContaining<Startup>());

            var tokenValidationParametrs = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //ValidIssuer = Configuration["Tokens:Issuer"],
                //ValidAudience = Configuration["Tokens:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                //ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = false
            };

            services.AddSingleton(tokenValidationParametrs);

            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = tokenValidationParametrs;
            });

            // настройка авторизации
            services.AddAuthorization(options =>
            {
                //// добавляем политику с именем TagViewer и настраиваем претензию для неё
                //options.AddPolicy("TagViewer", builder => builder.RequireClaim("tags.view", "true"));

                //// добавляем поликику авторицазии пользователя
                options.AddPolicy("MustWorkForChapsas", policy => 
                {
                    policy.AddRequirements(new WorksForCompanyRequirement("chapsas.com"));
                });
            });

            services.AddSingleton<IAuthorizationHandler, WorksFromCompanyHandler>();

            services.AddControllersWithViews();

            services.AddSingleton<IUriService>(provider =>
            {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                return new UriService(absoluteUri);
            });
        }
    }
}
