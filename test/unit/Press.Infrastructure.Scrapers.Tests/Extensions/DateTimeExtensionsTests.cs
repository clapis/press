using Press.Infrastructure.Scrapers.Extensions;

namespace Press.Infrastructure.Scrapers.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Theory(DisplayName = "Return the expected month name without diacritics")]
    [InlineData(01, "janeiro")]
    [InlineData(02, "fevereiro")]
    [InlineData(03, "marco")]
    [InlineData(04, "abril")]
    [InlineData(05, "maio")]
    [InlineData(06, "junho")]
    [InlineData(07, "julho")]
    [InlineData(08, "agosto")]
    [InlineData(09, "setembro")]
    [InlineData(10, "outubro")]
    [InlineData(11, "novembro")]
    [InlineData(12, "dezembro")]
    public void Test01(int month, string expected)
    {
        var date = new DateTime(2020, month, 01);
        
        var actual = date.GetMonthNamePT_BR();
        
        Assert.Equal(expected, actual);
    }
}