using System.Text.Json;

namespace GLMS.Web.Services;

public interface ICurrencyService
{
    Task<decimal> GetUsdToZarRateAsync();
    decimal Convert(decimal usdAmount, decimal rate);
}

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public CurrencyService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        var url = _config["CurrencyApi:BaseUrl"]!;
        var response = await _http.GetStringAsync(url);
        using var doc = JsonDocument.Parse(response);
        var rate = doc.RootElement.GetProperty("rates").GetProperty("ZAR").GetDecimal();
        return rate;
    }

    public decimal Convert(decimal usdAmount, decimal rate) => Math.Round(usdAmount * rate, 2);
}
