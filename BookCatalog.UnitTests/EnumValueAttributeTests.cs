using BookCatalog.Application.Validation;
using BookCatalog.Domain.Enums;

namespace BookCatalog.Tests;

public class EnumValueAttributeTests
{
    private readonly EnumValueAttribute<Genre> _sut = new();

    [Theory]
    [InlineData(Genre.Fantasy)]
    [InlineData(Genre.Horror)]
    [InlineData(Genre.Romance)]
    public void IsValid_ReturnsTrue_ForDefinedGenre(Genre genre)
    {
        Assert.True(_sut.IsValid(genre));
    }

    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    public void IsValid_ReturnsFalse_ForUndefinedGenre(int rawValue)
    {
        Assert.False(_sut.IsValid((Genre)rawValue));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForNull()
    {
        Assert.False(_sut.IsValid(null));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForWrongType()
    {
        Assert.False(_sut.IsValid("Fantasy"));
    }
}
