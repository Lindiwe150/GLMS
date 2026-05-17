using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace GLMS.Tests;

// ─── Currency Tests ───────────────────────────────────────────────────────────

public class CurrencyServiceTests
{
    private readonly CurrencyService _sut;

    public CurrencyServiceTests()
    {
        // CurrencyService.Convert does not use HttpClient, so we can pass null safely.
        _sut = new CurrencyService(null!, null!);
    }

    [Fact]
    public void Convert_CorrectlyMultipliesUsdByRate()
    {
        var result = _sut.Convert(100m, 18.50m);
        Assert.Equal(1850.00m, result);
    }

    [Fact]
    public void Convert_ZeroUsd_ReturnsZero()
    {
        Assert.Equal(0m, _sut.Convert(0m, 18.50m));
    }

    [Fact]
    public void Convert_RoundsToTwoDecimalPlaces()
    {
        // 10 * 18.333 = 183.33
        var result = _sut.Convert(10m, 18.333m);
        Assert.Equal(183.33m, result);
    }

    [Theory]
    [InlineData(50, 19.0, 950.00)]
    [InlineData(1, 18.75, 18.75)]
    [InlineData(200, 17.5, 3500.00)]
    public void Convert_VariousAmounts_ReturnsCorrectZar(decimal usd, decimal rate, decimal expected)
    {
        Assert.Equal(expected, _sut.Convert(usd, rate));
    }
}

// ─── File Validation Tests ────────────────────────────────────────────────────

public class FileServiceTests
{
    private readonly FileService _sut = new();

    private static IFormFile MakeFakeFile(string fileName, string contentType)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(fileName);
        mock.Setup(f => f.ContentType).Returns(contentType);
        return mock.Object;
    }

    [Fact]
    public void IsPdf_ValidPdf_ReturnsTrue()
    {
        var file = MakeFakeFile("agreement.pdf", "application/pdf");
        Assert.True(_sut.IsPdf(file));
    }

    [Fact]
    public void IsPdf_ExeFile_ReturnsFalse()
    {
        var file = MakeFakeFile("malware.exe", "application/octet-stream");
        Assert.False(_sut.IsPdf(file));
    }

    [Fact]
    public void IsPdf_WordDoc_ReturnsFalse()
    {
        var file = MakeFakeFile("contract.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        Assert.False(_sut.IsPdf(file));
    }

    [Fact]
    public void IsPdf_PdfExtensionWrongMime_ReturnsFalse()
    {
        // Extension is .pdf but MIME is wrong — should still fail
        var file = MakeFakeFile("trick.pdf", "application/octet-stream");
        Assert.False(_sut.IsPdf(file));
    }

    [Fact]
    public async Task SavePdfAsync_NonPdf_ThrowsInvalidOperationException()
    {
        var file = MakeFakeFile("virus.exe", "application/octet-stream");
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SavePdfAsync(file, Path.GetTempPath()));
    }
}

// ─── Workflow Logic Tests ─────────────────────────────────────────────────────

public class WorkflowLogicTests
{
    /// <summary>
    /// Mirrors the guard logic in ServiceRequestController.Create (POST).
    /// Extracted here as a pure function so it can be unit-tested without HTTP context.
    /// </summary>
    private static bool CanCreateServiceRequest(Contract contract) =>
        contract.Status != ContractStatus.Expired &&
        contract.Status != ContractStatus.OnHold;

    [Fact]
    public void ActiveContract_AllowsServiceRequest()
    {
        var contract = new Contract { Status = ContractStatus.Active };
        Assert.True(CanCreateServiceRequest(contract));
    }

    [Fact]
    public void DraftContract_AllowsServiceRequest()
    {
        var contract = new Contract { Status = ContractStatus.Draft };
        Assert.True(CanCreateServiceRequest(contract));
    }

    [Fact]
    public void ExpiredContract_BlocksServiceRequest()
    {
        var contract = new Contract { Status = ContractStatus.Expired };
        Assert.False(CanCreateServiceRequest(contract));
    }

    [Fact]
    public void OnHoldContract_BlocksServiceRequest()
    {
        var contract = new Contract { Status = ContractStatus.OnHold };
        Assert.False(CanCreateServiceRequest(contract));
    }

    [Theory]
    [InlineData(ContractStatus.Expired, false)]
    [InlineData(ContractStatus.OnHold, false)]
    [InlineData(ContractStatus.Active, true)]
    [InlineData(ContractStatus.Draft, true)]
    public void AllStatuses_WorkflowGuardBehavesCorrectly(ContractStatus status, bool expected)
    {
        var contract = new Contract { Status = status };
        Assert.Equal(expected, CanCreateServiceRequest(contract));
    }
}
