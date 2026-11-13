using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
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
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
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
            throw new UnAuthorizedExceptions("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            throw new UnAuthorizedExceptions("Invalid username or password.");
        }

        var isEmailConfirm = await _userManager.IsEmailConfirmedAsync(user);

        if (!isEmailConfirm)
        {
            throw new UnAuthorizedExceptions("Invalid username or password.");
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            throw new UnAuthorizedExceptions("Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return new AuthServiceResponseModel
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Token = token.Token,
            TokenExpiration = token.TokenExpiration
        };
    }

    private TokenModel GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var utcNow = DateTime.UtcNow;
        var expiration = utcNow.AddDays(_jwtSettings.ExpirationInDays);

        var claims = new List<Claim>
        {
            new("Id", user.Id.ToString()),
            new("FullName", user.FullName),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);

        var jwtToken = jwtTokenHandler.WriteToken(token);

        return new TokenModel
        {
            Token = jwtToken,
            TokenExpiration = new DateTimeOffset(expiration).ToUnixTimeSeconds()
        };
    }

}