using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Stockpilot.Application.Exceptions;
using Stockpilot.Identity.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Stockpilot.Identity;

public class AuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public AuthenticationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, RoleManager<IdentityRole<Guid>> roleManager, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthServiceResponseModel> Authentication(AuthServiceRequestModel request)
    {
        // Check if the identifier is an email or username
        var isEmail = request.UserKey.Contains("@");

        ApplicationUser? user;

        // Find the user by email or username
        if (isEmail)
        {
            user = await _userManager.FindByEmailAsync(request.UserKey);
        }
        else
        {
            user = await _userManager.FindByNameAsync(request.UserKey);
        }

        if (user is null)
        {
            throw new NotFoundExceptions($"{request.UserKey}");
        }

        var isEmailConfirm = await _userManager.IsEmailConfirmedAsync(user);

        if (!isEmailConfirm)
        {
            throw new UnAuthorizedExceptions("Please Confirm Your Email To Login");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            throw new UnAuthorizedExceptions($"Credentials for '{request.UserKey} aren't valid'.");
        }

        var token = GenerateToken(user);

        return new AuthServiceResponseModel
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Token = token.Token,
            TokenExpiration = token.TokenExpiration
        };
    }

    private TokenModel GenerateToken(ApplicationUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings:SecretKey").Value!);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim("Id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JwtSettings:Audience").Value!),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JwtSettings:Issuer").Value!),
            }),

            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);

        var jwtToken = jwtTokenHandler.WriteToken(token);

        DateTimeOffset dateTimeOffset = new DateTimeOffset(tokenDescriptor.Expires.GetValueOrDefault());

        return new TokenModel
        {
            Token = jwtToken,
            TokenExpiration = dateTimeOffset.ToUnixTimeSeconds()
        };
    }

}