using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Stockpilot.Identity.Models;

namespace Stockpilot.Identity.UnitTests;

public sealed class AuthenticationServiceFixture
{
    public UserManager<ApplicationUser> UserManager { get; }
    public SignInManager<ApplicationUser> SignInManager { get; }
    public IOptions<JwtSettings> JwtSettings { get; }

    public AuthenticationServiceFixture()
    {
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        UserManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore,
            Options.Create(new IdentityOptions()),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());
        UserManager.GetRolesAsync(Arg.Any<ApplicationUser>()).Returns(new List<string>());

        var contextAccessor = Substitute.For<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        SignInManager = Substitute.For<SignInManager<ApplicationUser>>(
            UserManager,
            contextAccessor,
            claimsFactory,
            Options.Create(new IdentityOptions()),
            Substitute.For<ILogger<SignInManager<ApplicationUser>>>(),
            Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<ApplicationUser>>());

        JwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "StockPilot-Test-Secret-Key-Sufficiently-Long-For-Hmac-Sha256-1234567890",
            Audience = "StockPilot.Tests",
            Issuer = "StockPilot.Tests"
        });
    }

    public AuthenticationService CreateService() => new(
        UserManager,
        SignInManager,
        JwtSettings);

    public static ApplicationUser CreateUser()
    {
        var userId = Guid.NewGuid();
        return new ApplicationUser
        {
            Id = userId,
            UserName = "test.user",
            Email = "test.user@example.com",
            FullName = "Test User",
            EmailConfirmed = true
        };
    }
}
