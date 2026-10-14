using Devlivery.Common.Errors;
using Devlivery.Domain.Aggregates.Users;
using Devlivery.Features.Auth.Commands.Login;
using Devlivery.Infrastructure.Identity.Abstractions;
using Devlivery.Infrastructure.Persistence.Context;
using Devlivery.Tests.Common.Builders;
using Devlivery.Tests.Common.Mocks;

using FluentResults;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Auth.Commands.Login;

[Trait("Category", "Unit Tests")]
public sealed class LoginHandlerTests : IDisposable
{
    private const string Email = "maria@pizzaria.com";
    private const string Password = "senha-segura";

    private readonly ApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        // Login runs before any tenant is known: the context's tenant never matches the user's establishment
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options, TestMocks.TenantAccessor());
        _handler = new LoginHandler(NullLogger<LoginHandler>.Instance, _dbContext, _identityService, _tokenService);
    }

    [Fact]
    public async Task GivenUnknownEmail_WhenLoggingIn_ThenReturnsUnauthorizedWithoutCheckingPassword()
    {
        // Arrange
        var command = new LoginCommand("desconhecido@pizzaria.com", Password);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<UnauthorizedError>()
            .Message.ShouldBe("Usuário ou senha inválidos.");
        await _identityService.DidNotReceiveWithAnyArgs().SignInAsync(default!, default!, default);
    }

    [Fact]
    public async Task GivenWrongPassword_WhenLoggingIn_ThenReturnsSameUnauthorizedErrorWithoutIssuingToken()
    {
        // Arrange
        await SeedUserAsync();
        _identityService.SignInAsync(Email, "senha-errada", Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Invalid credentials"));

        // Act
        var result = await _handler.HandleAsync(new LoginCommand(Email, "senha-errada"), CancellationToken.None);

        // Assert
        result.Errors.ShouldHaveSingleItem().ShouldBeOfType<UnauthorizedError>()
            .Message.ShouldBe("Usuário ou senha inválidos.");
        await _tokenService.DidNotReceiveWithAnyArgs().GenerateTokenAsync(default!, default);
    }

    [Fact]
    public async Task GivenValidCredentials_WhenLoggingIn_ThenIssuesTokenForUserAndTheirEstablishment()
    {
        // Arrange
        var user = await SeedUserAsync();
        _identityService.SignInAsync(Email, Password, Arg.Any<CancellationToken>()).Returns(Result.Ok());
        _tokenService.GenerateTokenAsync(
                new TokenRequest(user.Id.ToString(), user.EstablishmentId.ToString()),
                Arg.Any<CancellationToken>())
            .Returns("jwt-token");

        // Act
        var result = await _handler.HandleAsync(new LoginCommand(Email, Password), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(new LoginResponse(user.Id, "Maria Souza", "jwt-token"));
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task<User> SeedUserAsync()
    {
        var user = new UserBuilder().WithName("Maria Souza").WithEmail(Email).Build();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
}