using FluentAssertions;
using Moq;
using PulsePoll.Application.Exceptions;
using PulsePoll.Application.Interfaces;
using PulsePoll.Application.Services;
using PulsePoll.Domain.Entities;
using Xunit;

namespace PulsePoll.Tests;

public class BankServiceTests
{
    private readonly Mock<IBankRepository> _bankRepositoryMock = new();
    private readonly BankService _sut;

    public BankServiceTests()
    {
        _sut = new BankService(_bankRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ThrowsBusinessException_WhenBankCodeIsNotFiveDigits()
    {
        await _sut.Invoking(s => s.CreateOrUpdateAsync(0, "Test Bank", "TB", "12A4", true, null, null))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("*5 haneli sayısal*");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_ThrowsBusinessException_WhenBankCodeAlreadyExists()
    {
        _bankRepositoryMock.Setup(r => r.ExistsByNameAsync("Test Bank", 0)).ReturnsAsync(false);
        _bankRepositoryMock.Setup(r => r.ExistsByBankCodeAsync("12345", 0)).ReturnsAsync(true);

        await _sut.Invoking(s => s.CreateOrUpdateAsync(0, "Test Bank", "TB", "12345", true, null, null))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("*banka koduna sahip başka bir banka*");
    }

    [Fact]
    public async Task CreateOrUpdateAsync_NormalizesAndPersistsBankCode_WhenValid()
    {
        Bank? added = null;
        _bankRepositoryMock.Setup(r => r.ExistsByNameAsync("Test Bank", 0)).ReturnsAsync(false);
        _bankRepositoryMock.Setup(r => r.ExistsByBankCodeAsync("12345", 0)).ReturnsAsync(false);
        _bankRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Bank>()))
            .Callback<Bank>(b => added = b)
            .Returns(Task.CompletedTask);
        _bankRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Bank>()))
            .Returns(Task.CompletedTask);

        await _sut.CreateOrUpdateAsync(0, " Test Bank ", "tb", " 12345 ", true, null, null);

        added.Should().NotBeNull();
        added!.Name.Should().Be("Test Bank");
        added.Code.Should().Be("TB");
        added.BankCode.Should().Be("12345");
    }
}
