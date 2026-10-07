using BookCatalog.Application.Validation;

namespace BookCatalog.Tests;

public class PublicationYearAttributeTests
{
    private readonly PublicationYearAttribute _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1965)]
    public void IsValid_ReturnsTrue_ForYearInThePast(int year)
    {
        Assert.True(_sut.IsValid(year));
    }

    // The point of the attribute: the boundary follows the calendar instead of being
    // a constant that goes stale on 1 January.
    [Fact]
    public void IsValid_ReturnsTrue_ForTheCurrentYear()
    {
        Assert.True(_sut.IsValid(DateTime.UtcNow.Year));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNextYear()
    {
        Assert.False(_sut.IsValid(DateTime.UtcNow.Year + 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2000)]
    public void IsValid_ReturnsFalse_ForNegativeYear(int year)
    {
        Assert.False(_sut.IsValid(year));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(_sut.IsValid(null));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForWrongType()
    {
        Assert.False(_sut.IsValid("1965"));
    }

    [Fact]
    public void FormatErrorMessage_NamesTheCurrentYear()
    {
        Assert.Contains(DateTime.UtcNow.Year.ToString(), _sut.FormatErrorMessage("PublicationYear"));
    }
}
