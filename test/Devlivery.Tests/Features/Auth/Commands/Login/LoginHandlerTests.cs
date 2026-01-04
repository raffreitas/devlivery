using Devlivery.Features.Auth.Commands.Login;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Identity.Abstractions;

using FluentResults;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shouldly;

namespace Devlivery.Tests.Features.Auth.Commands.Login;

[Trait("Category", "Unit Tests")]
public sealed class LoginHandlerTests(AuthUnitTestFixture fixture) : IClassFixture<AuthUnitTestFixture>
{
    [Fact]
    public async Task Handle_Should_Return_UnauthorizedError_When_User_Does_Not_Exist()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoginHandler>>();
        var dbContext = fixture.CreateDbContextMock();
        var identityService = fixture.CreateIdentityServiceMock();
        var tokenService = fixture.CreateTokenServiceMock();

        var handler = new LoginHandler(logger, dbContext, identityService, tokenService);

        var command = new LoginCommand(
            Email: fixture.Faker.Internet.Email(),
            Password: fixture.Faker.Internet.Password()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is UnauthorizedError);
        await identityService.DidNotReceiveWithAnyArgs().SignInAsync(default!, default!, default);
    }

    [Fact]
    public async Task Handle_Should_Return_UnauthorizedError_When_SignIn_Fails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoginHandler>>();
        var dbContext = fixture.CreateDbContextMock();
        var identityService = fixture.CreateIdentityServiceMock();
        var tokenService = fixture.CreateTokenServiceMock();

        var user = fixture.CreateUser(
            email: "user@example.com"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "user@example.com",
            Password: "wrongpassword"
        );

        // Mock SignIn to fail
        identityService.SignInAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Fail("Invalid credentials"));

        var handler = new LoginHandler(logger, dbContext, identityService, tokenService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is UnauthorizedError);
        await tokenService.DidNotReceiveWithAnyArgs().GenerateTokenAsync(default!, default);
    }

    [Fact]
    public async Task Handle_Should_Return_LoginResponse_When_Credentials_Are_Valid()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoginHandler>>();
        var dbContext = fixture.CreateDbContextMock();
        var identityService = fixture.CreateIdentityServiceMock();
        var tokenService = fixture.CreateTokenServiceMock();

        var user = fixture.CreateUser(
            name: "Test User",
            email: "user@example.com"
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var command = new LoginCommand(
            Email: "user@example.com",
            Password: "validpassword"
        );

        var expectedToken = "generated-jwt-token";

        // Mock SignIn to succeed
        identityService.SignInAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result.Ok());

        // Mock token generation
        tokenService.GenerateTokenAsync(
            Arg.Is<TokenRequest>(r =>
                r.SubjectId == user.Id.ToString() &&
                r.TenantId == user.EstablishmentId.ToString()),
            Arg.Any<CancellationToken>())
            .Returns(expectedToken);

        var handler = new LoginHandler(logger, dbContext, identityService, tokenService);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.UserId.ShouldBe(user.Id);
        result.Value.UserName.ShouldBe(user.Name);
        result.Value.Token.ShouldBe(expectedToken);

        await identityService.Received(1).SignInAsync(command.Email, command.Password, Arg.Any<CancellationToken>());
        await tokenService.Received(1).GenerateTokenAsync(
            Arg.Is<TokenRequest>(r =>
                r.SubjectId == user.Id.ToString() &&
                r.TenantId == user.EstablishmentId.ToString()),
            Arg.Any<CancellationToken>());
    }
}
