using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Stockpilot.Application.Exceptions;
using Stockpilot.Identity.Models;
using System.Security.Claims;
using Xunit;

namespace Stockpilot.Identity.UnitTests;

public class AuthenticationServiceTests
{
    [Fact]
    public async Task Authentication_WhenEmailIsProvided_ShouldReturnAuthenticatedUserAndValidToken()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        response.UserId.Should().Be(user.Id);
        response.UserName.Should().Be(user.UserName);
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.TokenExpiration.Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        jwt.Claims.Should().Contain(c => c.Type == "Id" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "FullName" && c.Value == user.FullName);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Aud && c.Value == "StockPilot.Tests");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iss && c.Value == "StockPilot.Tests");
        await fixture.UserManager.Received(1).FindByEmailAsync(user.Email!);
        await fixture.UserManager.Received(1).IsEmailConfirmedAsync(user);
        await fixture.SignInManager.Received(1).PasswordSignInAsync(user.UserName!, request.Password, false, false);
        await fixture.UserManager.DidNotReceive().FindByNameAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Authentication_WhenUsernameIsProvided_ShouldReturnAuthenticatedUserAndValidToken()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.UserName!, Password = "Password123!" };
        fixture.UserManager.FindByNameAsync(user.UserName!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        response.UserId.Should().Be(user.Id);
        response.UserName.Should().Be(user.UserName);
        response.Token.Should().NotBeNullOrWhiteSpace();
        response.TokenExpiration.Should().BeGreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        jwt.Claims.Should().Contain(c => c.Type == "Id" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "FullName" && c.Value == user.FullName);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Aud && c.Value == "StockPilot.Tests");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iss && c.Value == "StockPilot.Tests");
        await fixture.UserManager.Received(1).FindByNameAsync(user.UserName!);
        await fixture.UserManager.Received(1).IsEmailConfirmedAsync(user);
        await fixture.SignInManager.Received(1).PasswordSignInAsync(user.UserName!, request.Password, false, false);
        await fixture.UserManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Authentication_WhenExpirationIsConfigured_ShouldUseConfiguredExpirationInDays()
    {
        var fixture = new AuthenticationServiceFixture();
        fixture.JwtSettings.Value.ExpirationInDays = 3;
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        var issuedAt = long.Parse(jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Iat).Value);
        var expiration = new DateTimeOffset(jwt.ValidTo).ToUnixTimeSeconds();

        (expiration - issuedAt).Should().BeInRange(
            (long)TimeSpan.FromDays(3).TotalSeconds - 2,
            (long)TimeSpan.FromDays(3).TotalSeconds + 2);
    }

    [Fact]
    public async Task Authentication_WhenUserDoesNotExist_ShouldThrowUnauthorizedException()
    {
        var fixture = new AuthenticationServiceFixture();
        var request = new AuthServiceRequestModel { UserKey = "missing@example.com", Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(request.UserKey).Returns((ApplicationUser?)null);

        var act = () => fixture.CreateService().Authentication(request);

        var exception = await act.Should().ThrowAsync<UnAuthorizedExceptions>();
        exception.Which.Message.Should().Be("Invalid username or password.");
        await fixture.UserManager.DidNotReceive().IsEmailConfirmedAsync(Arg.Any<ApplicationUser>());
        await fixture.SignInManager.DidNotReceive().PasswordSignInAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Authentication_WhenUserIsInactive_ShouldRejectAuthentication()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        user.IsActive = false;
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);

        var act = () => fixture.CreateService().Authentication(request);

        var exception = await act.Should().ThrowAsync<UnAuthorizedExceptions>();
        exception.Which.Message.Should().Be("Invalid username or password.");
        await fixture.UserManager.DidNotReceive().IsEmailConfirmedAsync(Arg.Any<ApplicationUser>());
        await fixture.SignInManager.DidNotReceive().PasswordSignInAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Authentication_WhenEmailIsNotConfirmed_ShouldThrowUnauthorizedException()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(false);

        var act = () => fixture.CreateService().Authentication(request);

        var exception = await act.Should().ThrowAsync<UnAuthorizedExceptions>();
        exception.Which.Message.Should().Be("Invalid username or password.");
        await fixture.SignInManager.DidNotReceive().PasswordSignInAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<bool>());
    }

    [Fact]
    public async Task Authentication_WhenPasswordSignInFails_ShouldThrowUnauthorizedException()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "WrongPassword!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Failed);

        var act = () => fixture.CreateService().Authentication(request);

        var exception = await act.Should().ThrowAsync<UnAuthorizedExceptions>();
        exception.Which.Message.Should().Be("Invalid username or password.");
        await fixture.SignInManager.Received(1).PasswordSignInAsync(user.UserName!, request.Password, false, false);
    }

    [Fact]
    public async Task Authentication_WhenUserIsAdministrator_ShouldIncludeAdministratorRoleClaim()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);
        fixture.UserManager.GetRolesAsync(user).Returns(new List<string> { "Administrator" });

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Administrator");
        await fixture.UserManager.Received(1).GetRolesAsync(user);
    }

    [Fact]
    public async Task Authentication_WhenUserIsStaff_ShouldIncludeStaffRoleClaim()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);
        fixture.UserManager.GetRolesAsync(user).Returns(new List<string> { "Staff" });

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Staff");
        await fixture.UserManager.Received(1).GetRolesAsync(user);
    }

    [Fact]
    public async Task Authentication_WhenUserHasMultipleRoles_ShouldIncludeAllRoleClaims()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);
        fixture.UserManager.GetRolesAsync(user).Returns(new List<string> { "Administrator", "Staff" });

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
        var roleClaims = jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();

        roleClaims.Should().BeEquivalentTo(new[] { "Administrator", "Staff" });
        await fixture.UserManager.Received(1).GetRolesAsync(user);
    }

    [Fact]
    public async Task Authentication_WhenUserHasNoRoles_ShouldAuthenticateWithoutRoleClaims()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "Password123!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Success);
        fixture.UserManager.GetRolesAsync(user).Returns(new List<string>());

        var response = await fixture.CreateService().Authentication(request);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        response.Token.Should().NotBeNullOrWhiteSpace();
        jwt.Claims.Should().NotContain(c => c.Type == "role");
        await fixture.UserManager.Received(1).GetRolesAsync(user);
    }

    [Fact]
    public async Task Authentication_WhenPasswordSignInFails_ShouldNotRetrieveRoles()
    {
        var fixture = new AuthenticationServiceFixture();
        var user = AuthenticationServiceFixture.CreateUser();
        var request = new AuthServiceRequestModel { UserKey = user.Email!, Password = "WrongPassword!" };
        fixture.UserManager.FindByEmailAsync(user.Email!).Returns(user);
        fixture.UserManager.IsEmailConfirmedAsync(user).Returns(true);
        fixture.SignInManager.PasswordSignInAsync(user.UserName!, request.Password, false, false)
            .Returns(SignInResult.Failed);

        var act = () => fixture.CreateService().Authentication(request);

        await act.Should().ThrowAsync<UnAuthorizedExceptions>();
        await fixture.UserManager.DidNotReceive().GetRolesAsync(Arg.Any<ApplicationUser>());
    }
}
