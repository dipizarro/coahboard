using System;
using System.Threading.Tasks;
using CoachBoard.Application.Interfaces;
using CoachBoard.Domain.Entities;
using CoachBoard.Infrastructure.Persistence;
using CoachBoard.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CoachBoard.Application.Tests;

public class RepositoryTenantTests
{
    private readonly CoachBoardDbContext _context;
    private readonly Mock<ICurrentTenant> _currentTenantMock;

    public RepositoryTenantTests()
    {
        var options = new DbContextOptionsBuilder<CoachBoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoachBoardDbContext(options);
        _currentTenantMock = new Mock<ICurrentTenant>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByTenant()
    {
        // Arrange
        _context.Clients.AddRange(new List<Client>
        {
            new Client { Id = 1, FullName = "Client T1", TenantId = 1, CoachId = 1 },
            new Client { Id = 2, FullName = "Client T2", TenantId = 2, CoachId = 1 }
        });
        await _context.SaveChangesAsync();

        _currentTenantMock.Setup(x => x.TenantId).Returns(1);
        var repo = new Repository<Client>(_context, _currentTenantMock.Object);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().TenantId.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBelongsToOtherTenant()
    {
        // Arrange
        _context.Clients.Add(new Client { Id = 10, FullName = "Other Tenant", TenantId = 2, CoachId = 1 });
        await _context.SaveChangesAsync();

        _currentTenantMock.Setup(x => x.TenantId).Returns(1);
        var repo = new Repository<Client>(_context, _currentTenantMock.Object);

        // Act
        var result = await repo.GetByIdAsync(10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAssignTenantId()
    {
        // Arrange
        _currentTenantMock.Setup(x => x.TenantId).Returns(5);
        var repo = new Repository<Client>(_context, _currentTenantMock.Object);
        var client = new Client { FullName = "New Client", CoachId = 1 };

        // Act
        await repo.AddAsync(client);
        await repo.SaveChangesAsync();

        // Assert
        client.TenantId.Should().Be(5);
        var dbClient = await _context.Clients.FindAsync(client.Id);
        dbClient!.TenantId.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUpdatingOtherTenantEntity()
    {
        // Arrange
        var client = new Client { Id = 20, FullName = "B", TenantId = 2, CoachId = 1 };
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        _context.Entry(client).State = EntityState.Detached;

        _currentTenantMock.Setup(x => x.TenantId).Returns(1);
        var repo = new Repository<Client>(_context, _currentTenantMock.Object);

        // Act & Assert
        var act = async () => await repo.UpdateAsync(client);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
