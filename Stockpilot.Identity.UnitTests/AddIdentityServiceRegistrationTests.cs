using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Stockpilot.Identity.UnitTests;

public class AddIdentityServiceRegistrationTests
{
    [Fact]
    public async Task AddIdentityServices_WhenSecretKeyIsMissing_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:SecretKey", null));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:SecretKey*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenSecretKeyIsEmpty_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:SecretKey", string.Empty));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:SecretKey*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenSecretKeyIsWhitespace_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:SecretKey", "   "));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:SecretKey*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenSecretKeyIsShorterThan32Utf8Bytes_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:SecretKey", "short-key"));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*at least 32 bytes*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenIssuerIsMissing_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:Issuer", null));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:Issuer*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenIssuerIsEmpty_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:Issuer", string.Empty));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:Issuer*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenAudienceIsMissing_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:Audience", null));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:Audience*");
    }

    [Fact]
    public async Task AddIdentityServices_WhenAudienceIsEmpty_ShouldThrowInvalidOperationException()
    {
        var configuration = CreateConfiguration(("JwtSettings:Audience", string.Empty));

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddIdentityServices(configuration))
            .Build();

        var act = () => host.StartAsync();

        await act.Should().ThrowAsync<OptionsValidationException>()
            .WithMessage("*JwtSettings:Audience*");
    }

    [Fact]
    public void AddIdentityServices_WhenJwtSettingsAreValid_ShouldCompleteServiceRegistration()
    {
        var configuration = CreateConfiguration();
        var services = new ServiceCollection();

        var act = () => services.AddIdentityServices(configuration);

        act.Should().NotThrow();
        services.Should().NotBeEmpty();
    }

    private static IConfiguration CreateConfiguration(
        params (string Key, string? Value)[] overrides)
    {
        var settings = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "StockPilot-Test-Secret-Key-Sufficiently-Long-For-Hmac-Sha256-1234567890",
            ["JwtSettings:Issuer"] = "StockPilot.Tests",
            ["JwtSettings:Audience"] = "StockPilot.Tests"
        };

        foreach (var (key, value) in overrides)
        {
            settings[key] = value;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }
}
