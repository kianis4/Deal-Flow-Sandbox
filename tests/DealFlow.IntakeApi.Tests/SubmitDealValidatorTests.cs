using DealFlow.IntakeApi.Models;
using DealFlow.IntakeApi.Validators;
using FluentAssertions;

namespace DealFlow.IntakeApi.Tests;

public class SubmitDealValidatorTests
{
    private readonly SubmitDealValidator _validator = new();

    [Fact]
    public async Task Valid_request_passes_validation()
    {
        var request = new SubmitDealRequest(
            "Excavator", 2021, 250_000, 48, "Construction", "ON", "A");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", 2021, 250_000, 48, "Construction", "ON", "A")]
    [InlineData("Excavator", 2021, -1, 48, "Construction", "ON", "A")]
    [InlineData("Excavator", 2021, 250_000, 48, "Construction", "ON", "X")]
    [InlineData("Excavator", 2021, 250_000, 3, "Construction", "ON", "A")]
    public async Task Invalid_request_fails_validation(
        string type, int year, decimal amount, int term,
        string industry, string province, string tier)
    {
        var request = new SubmitDealRequest(type, year, amount, term, industry, province, tier);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Vendor_tier_must_be_A_B_or_C()
    {
        var request = new SubmitDealRequest("Forklift", 2020, 100_000, 24, "Logistics", "BC", "D");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VendorTier");
    }
}
