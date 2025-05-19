using backend.DTOs.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend.Services.JWT
{
    public class jwtService
    {
        private readonly IConfiguration _config;

        public jwtService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateToken(UserDTO user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = _config["JWTSettings:securityKey"];

            if (string.IsNullOrEmpty(securityKey))
            {
                throw new ArgumentNullException(nameof(securityKey), "La clave de seguridad no puede ser nula o vacía.");
            }

            var key = Encoding.UTF8.GetBytes(securityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("username", user.Username),
                    new Claim("email", user.Email),
                    new Claim("givennames", user.GivenNames),
                    new Claim("psurname", user.PSurname),
                    new Claim("msurname", user.MSurname ?? string.Empty),
                    new Claim("phonenumber", user.PhoneNumber),
                    new Claim("createdat", user.CreatedAt.ToString("o")),
                    new Claim("lastlogin", user.LastLogin?.ToString("o") ?? string.Empty)
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["JWTSettings:expiryInMinutes"] ?? throw new ArgumentNullException("JWTSettings:expiryInMinutes", "El valor no puede ser nulo o vacío."))),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JWTSettings:validIssuer"],
                Audience = _config["JWTSettings:validAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string RefreshToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = _config["JWTSettings:securityKey"];

            if (string.IsNullOrEmpty(securityKey))
            {
                throw new ArgumentNullException(nameof(securityKey), "La clave de seguridad no puede ser nula o vacía.");
            }

            var key = Encoding.UTF8.GetBytes(securityKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWTSettings:validIssuer"],
                    ValidAudience = _config["JWTSettings:validAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Evita márgenes de tiempo  
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var id = jwtToken.Claims.First(x => x.Type == "id").Value;
                var username = jwtToken.Claims.First(x => x.Type == "username").Value;
                var email = jwtToken.Claims.First(x => x.Type == "email").Value;
                var givenNames = jwtToken.Claims.First(x => x.Type == "givennames").Value;
                var pSurname = jwtToken.Claims.First(x => x.Type == "psurname").Value;
                var mSurname = jwtToken.Claims.First(x => x.Type == "msurname").Value;
                var phoneNumber = jwtToken.Claims.First(x => x.Type == "phonenumber").Value;
                var createdAtStr = jwtToken.Claims.First(x => x.Type == "createdat").Value;
                var lastLoginStr = jwtToken.Claims.First(x => x.Type == "lastlogin").Value;

                var user = new UserDTO
                {
                    Id = int.Parse(id),
                    Username = username,
                    Email = email,
                    GivenNames = givenNames,
                    PSurname = pSurname,
                    MSurname = string.IsNullOrEmpty(mSurname) ? null : mSurname,
                    PhoneNumber = phoneNumber,
                    CreatedAt = DateTime.Parse(createdAtStr),
                    LastLogin = string.IsNullOrEmpty(lastLoginStr) ? null : DateTime.Parse(lastLoginStr)
                };

                return CreateToken(user);
            }
            catch
            {
                throw new SecurityTokenException("Token inválido o expirado.");
            }
        }

        public string CreateEmailVerificationToken(int userId, string userEmail)
        {
            // Reutiliza la lógica de CreateToken, pero con claims mínimos y tiempo corto
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWTSettings:securityKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("type", "VerifyEmail"),
                    new Claim("userId", userId.ToString()),
                    new Claim("email", userEmail)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JWTSettings:validIssuer"],
                Audience = _config["JWTSettings:validAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreatePasswordResetToken(int userId, string userEmail)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWTSettings:securityKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("type", "ResetPassword"),
            new Claim("userId", userId.ToString()),
            new Claim("email", userEmail)
        }),
                Expires = DateTime.UtcNow.AddHours(1), // Menos tiempo para seguridad
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JWTSettings:validIssuer"],
                Audience = _config["JWTSettings:validAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreateRegistrationToken(
    string username,
    string email,
    string givenNames,
    string pSurname,
    string mSurname,
    string phoneNumber,
    string passwordHash,
    string salt)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWTSettings:securityKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("type", "Registration"),
            new Claim("username", username),
            new Claim("email", email),
            new Claim("givenNames", givenNames),
            new Claim("pSurname", pSurname),
            new Claim("mSurname", mSurname ?? ""),
            new Claim("phoneNumber", phoneNumber),
            new Claim("passwordHash", passwordHash),
            new Claim("passwordSalt", salt)
        }),
                Expires = DateTime.UtcNow.AddHours(24), // 24 horas para verificar
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["JWTSettings:validIssuer"],
                Audience = _config["JWTSettings:validAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}