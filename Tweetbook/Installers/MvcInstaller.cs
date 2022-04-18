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
                // укзывает, будет ли валидироваться издатель при валидации токена
                ValidateIssuer = false,
                // будет ли валидироваться потребитель токена
                ValidateAudience = false,
                // будет ли валидироваться время существования
                ValidateLifetime = true,
                // строка, представляющая издателя
                //ValidIssuer = Configuration["Tokens:Issuer"],
                // установка потребителя токена
                //ValidAudience = Configuration["Tokens:Issuer"],
                // установка ключа безопасности, которым подписывается токен
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                // валидация ключа безопасности
                ValidateIssuerSigningKey = true,
                //ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = false
            };

            services.AddSingleton(tokenValidationParametrs);

            // добавим аудентификацию
            services.AddAuthentication(x => {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // исп. схему по уиолчании от jwt
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // дефолтная схема от jwt
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // дефолтная схема вызова от jwt
            }).AddJwtBearer(x =>
            {
                // добавляется конфигурация токена
                x.SaveToken = true;
                // если равно false, то SSL при отправке токена не используется
                // x.RequireHttpsMetadata = false;
                // параметры валидации токена
                x.TokenValidationParameters = tokenValidationParametrs;
            });

            // настройка авторизации
            services.AddAuthorization(options =>
            {
                // добавляем политику с именем TagViewer и настраиваем претензию для неё
                options.AddPolicy("TagViewer", builder => builder.RequireClaim("tags.view", "true"));

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
