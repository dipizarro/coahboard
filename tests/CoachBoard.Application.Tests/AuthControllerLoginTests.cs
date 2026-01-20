using CoachBoard.Api.Controllers;
using CoachBoard.Application.DTOs;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoachBoard.Application.Tests;

public class AuthControllerLoginTests
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAuthResponseWithToken()
    {
        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"),
            Role = "Coach"
        };
        var coach = new Coach
        {
            Id = 10,
            UserId = user.Id,
            Name = "Test Coach",
            Specialty = "General"
        };
        var loginRequest = new LoginRequest("USER@example.com", "P@ssw0rd");

        var users = new Mock<IUserRepository>();
        var coaches = new Mock<ICoachRepository>();
        var jwt = new Mock<IJwtService>();

        users.Setup(repo => repo.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        coaches.Setup(repo => repo.GetByUserIdAsync(user.Id))
            .ReturnsAsync(coach);
        jwt.Setup(service => service.GenerateToken(user.Email, user.Role, coach.Id))
            .Returns("token-value");

        var controller = new AuthController(users.Object, coaches.Object, jwt.Object);

        var actionResult = await controller.Login(loginRequest);

        actionResult.Result.Should().BeOfType<OkObjectResult>();
        var ok = actionResult.Result as OkObjectResult;
        ok?.Value.Should().BeOfType<AuthResponse>();
        var response = ok?.Value as AuthResponse;
        response?.Token.Should().Be("token-value");
        response?.Email.Should().Be(user.Email);
        response?.Role.Should().Be(user.Role);
        response?.CoachId.Should().Be(coach.Id);

        users.Verify(repo => repo.GetByEmailAsync("user@example.com"), Times.Once);
        coaches.Verify(repo => repo.GetByUserIdAsync(user.Id), Times.Once);
        jwt.Verify(service => service.GenerateToken(user.Email, user.Role, coach.Id), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var user = new User
        {
            Id = 2,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            Role = "Coach"
        };
        var loginRequest = new LoginRequest("user@example.com", "WrongPassword");

        var users = new Mock<IUserRepository>();
        var coaches = new Mock<ICoachRepository>();
        var jwt = new Mock<IJwtService>();

        users.Setup(repo => repo.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(user);

        var controller = new AuthController(users.Object, coaches.Object, jwt.Object);

        var actionResult = await controller.Login(loginRequest);

        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
        coaches.Verify(repo => repo.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        jwt.Verify(service => service.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequest("missing@example.com", "AnyPassword");

        var users = new Mock<IUserRepository>();
        var coaches = new Mock<ICoachRepository>();
        var jwt = new Mock<IJwtService>();

        users.Setup(repo => repo.GetByEmailAsync("missing@example.com"))
            .ReturnsAsync((User?)null);

        var controller = new AuthController(users.Object, coaches.Object, jwt.Object);

        var actionResult = await controller.Login(loginRequest);

        actionResult.Result.Should().BeOfType<UnauthorizedObjectResult>();
        coaches.Verify(repo => repo.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        jwt.Verify(service => service.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
    }
}
