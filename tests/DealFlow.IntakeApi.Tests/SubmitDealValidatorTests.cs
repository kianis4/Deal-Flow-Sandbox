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
            "Semi-Truck (Kenworth T680)", 2022, 185_000, 60, "Transportation", "ON", "CR1");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", 2021, 250_000, 48, "Construction", "ON", "CR2")]        // empty equipment type
    [InlineData("Excavator", 2021, -1, 48, "Construction", "ON", "CR2")]     // negative amount
    [InlineData("Excavator", 2021, 250_000, 48, "Construction", "ON", "X")]  // invalid credit rating
    [InlineData("Excavator", 2021, 250_000, 3, "Construction", "ON", "CR2")] // term too short
    public async Task Invalid_request_fails_validation(
        string type, int year, decimal amount, int term,
        string industry, string province, string creditRating)
    {
        var request = new SubmitDealRequest(type, year, amount, term, industry, province, creditRating);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Credit_rating_must_be_CR1_through_CR5()
    {
        var request = new SubmitDealRequest(
            "Dump Truck (Volvo FH16)", 2021, 320_000, 48, "Construction", "AB", "CR9");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreditRating");
    }
}
