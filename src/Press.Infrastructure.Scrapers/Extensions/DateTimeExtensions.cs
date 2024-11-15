namespace Press.Infrastructure.Scrapers.Extensions;

public static class DateTimeExtensions
{
    public static string GetMonthNamePT_BR(this DateTime date) => date.Month switch
    {
        01 => "janeiro",
        02 => "fevereiro",
        03 => "marco",
        04 => "abril",
        05 => "maio",
        06 => "junho",
        07 => "julho",
        08 => "agosto",
        09 => "setembro",
        10 => "outubro",
        11 => "novembro",
        12 => "dezembro",
        _ => throw new ArgumentOutOfRangeException(nameof(date), date, null)
    };
}