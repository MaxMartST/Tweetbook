using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Data;
using Tweetbook.Domain;
using Tweetbook.Options;

namespace Tweetbook.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly DataContext _context;
        public IdentityService(UserManager<IdentityUser> userManager, JwtSettings jwtSettings, TokenValidationParameters tokenValidationParameters, DataContext context)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _context = context;
        }

        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User does not exists" }
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!userHasValidPassword)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User/password combination is wrong" }
                };
            }

            return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> RegisterAsync(string email, string password)
        {
            // создаём пользователя и токен
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Errors = new[] { "User with this email address already exists" }
                };
            }

            var newUserId = Guid.NewGuid();
            var newUser = new IdentityUser
            { 
                Id = newUserId.ToString(),
                Email = email,
                UserName = email
            };

            var createdUser = await _userManager.CreateAsync(newUser, password);

            if (!createdUser.Succeeded)
            {
                return new AuthenticationResult
                { 
                    Errors = createdUser.Errors.Select(x => x.Description)
                };
            }

            // добавляем претензию в _userManager для нового пользователя
            // указыаем имя претензии и значение
            await _userManager.AddClaimAsync(newUser, new Claim("tags.view", "true"));

            // добавляем созданному пользователю роль - Poster
            await _userManager.AddToRoleAsync(newUser, "Poster");

            return await GenerateAuthenticationResultForUserAsync(newUser);
        }


        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            // токен безопасности
            var tokenHanlder = new JwtSecurityTokenHandler();

            try
            {
                // получить принцип безопастности
                var principal = tokenHanlder.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                // проверить безопастность алгоритма
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            // убедиться в типе токена и алгоритма безопасности 
            return (validatedToken is JwtSecurityToken jwtSecurityToken) 
                && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                    StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            // создаём список претензии
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

            // берем список претензии пользователя
            var userClaims = await _userManager.GetClaimsAsync(user);
            // и добавляем в список претензий
            claims.AddRange(userClaims);

            // получить роли пользователя
            var roles = await _userManager.GetRolesAsync(user);
            // добавляем список ролей пользователя в претензии
            claims.AddRange(roles
                .Select(role => 
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            // описание токена
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // добавить время через сколько истечёт токен
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token   
            };
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {
            // получить подтверждённый токен
            var validateToken = GetPrincipalFromToken(token);

            // проверить действительность токена
            if (validateToken == null)
            {
                return new AuthenticationResult { 
                    Errors = new[] { "Invalid Token" } 
                };
            }

            var expiryDateUnix = long.Parse(validateToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            // проверить срок действительности токена
            if (expiryDateUtc > DateTime.UtcNow)
            {
                // срок действия ещё не истёк
                return new AuthenticationResult { 
                    Errors = new[] { "This token hasn't expired yet" } 
                };
            }

            var jti = validateToken.Claims
                .Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(x => x.Token == refreshToken);

            // подтвердить существование токена из БД
            if (storedRefreshToken == null)
            {
                // такой токин не существует
                return new AuthenticationResult { 
                    Errors = new[] { "This refresh token does not exist" } 
                };
            }

            // сравнить срок действия с записью из БД
            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                // Срок действия этого токена обновления истек
                return new AuthenticationResult { 
                    Errors = new[] { "This refresh token has expired" } 
                };
            }

            // проверить действительность токена
            if (storedRefreshToken.Invalidated) 
            {
                // Этот токен обновления был признан недействительным
                return new AuthenticationResult { 
                    Errors = new[] { "This refresh token has been invalidated" } 
                };
            }

            if (storedRefreshToken.Used)
            {
                // Этот токен обновления был использован
                return new AuthenticationResult { 
                    Errors = new[] { "This refresh token has been used" } 
                };
            }

            if (storedRefreshToken.JwtId != jti)
            {
                // Этот токен обновления не соответствует этому JWT
                return new AuthenticationResult { 
                    Errors = new[] { "This refresh token does not match this JWT" } 
                };
            }

            // токен используется
            storedRefreshToken.Used = true;
            _context.RefreshTokens.Update(storedRefreshToken);

            await _context.SaveChangesAsync();

            // получить пользователя по id, полученным из токена
            var user = await _userManager
                .FindByIdAsync(validateToken.Claims.Single(x => x.Type == "id").Value);

            return await GenerateAuthenticationResultForUserAsync(user);
        }
    }
}
