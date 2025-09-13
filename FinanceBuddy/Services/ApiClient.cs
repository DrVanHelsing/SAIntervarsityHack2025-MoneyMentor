using System.Net.Http.Json;
using MoneyMentor.Shared.DTOs;
using MoneyMentor.Shared.Models;
using System.Diagnostics;

namespace FinanceBuddy.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly Uri _localBase;
    private readonly Uri _cloudBase = new("https://moneymentorbwbgic-api.azurewebsites.net");

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromSeconds(30);

        string? overrideUrl = Environment.GetEnvironmentVariable("MONEYMENTOR_CLOUD_API");
        if (!string.IsNullOrWhiteSpace(overrideUrl) && Uri.TryCreate(overrideUrl, UriKind.Absolute, out var o))
        {
            _localBase = o; // treat override as primary
        }
        else
        {
#if ANDROID
            _localBase = new Uri("http://10.0.2.2:7000"); // emulator -> host
#else
            _localBase = new Uri("https://localhost:7001");
#endif
        }
        
        Debug.WriteLine($"ApiClient initialized - Local: {_localBase}, Cloud: {_cloudBase}");
    }

    private async Task<T?> GetWithFallbackAsync<T>(string relative, CancellationToken ct = default)
    {
        Debug.WriteLine($"GET {relative} - trying local first");
        // Try local first
        try
        {
            var localUrl = new Uri(_localBase, relative);
            Debug.WriteLine($"Calling local URL: {localUrl}");
            var res = await _http.GetAsync(localUrl, ct);
            Debug.WriteLine($"Local response: {res.StatusCode}");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
                Debug.WriteLine($"Local success: received {typeof(T).Name}");
                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Local request failed: {ex.Message}");
        }

        // Fallback to cloud
        Debug.WriteLine($"Falling back to cloud");
        try
        {
            var cloudUrl = new Uri(_cloudBase, relative);
            Debug.WriteLine($"Calling cloud URL: {cloudUrl}");
            var res = await _http.GetAsync(cloudUrl, ct);
            Debug.WriteLine($"Cloud response: {res.StatusCode}");
            if (res.IsSuccessStatusCode)
            {
                var result = await res.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
                Debug.WriteLine($"Cloud success: received {typeof(T).Name}");
                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cloud request failed: {ex.Message}");
        }
        
        Debug.WriteLine($"Both local and cloud requests failed for {relative}");
        return default;
    }

    private async Task<HttpResponseMessage?> PostWithFallbackAsync<TBody>(string relative, TBody body, CancellationToken ct = default)
    {
        Debug.WriteLine($"POST {relative} - trying local first");
        // local
        try
        {
            var localUrl = new Uri(_localBase, relative);
            Debug.WriteLine($"Posting to local URL: {localUrl}");
            var res = await _http.PostAsJsonAsync(localUrl, body, ct);
            Debug.WriteLine($"Local POST response: {res.StatusCode}");
            if (res.IsSuccessStatusCode) 
            {
                Debug.WriteLine("Local POST successful");
                return res;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Local POST failed: {ex.Message}");
        }
        
        // cloud
        Debug.WriteLine("Falling back to cloud for POST");
        try
        {
            var cloudUrl = new Uri(_cloudBase, relative);
            Debug.WriteLine($"Posting to cloud URL: {cloudUrl}");
            var res = await _http.PostAsJsonAsync(cloudUrl, body, ct);
            Debug.WriteLine($"Cloud POST response: {res.StatusCode}");
            if (res.IsSuccessStatusCode)
            {
                Debug.WriteLine("Cloud POST successful");
            }
            return res; // return even if failure to allow caller to inspect
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Cloud POST failed: {ex.Message}");
            return null;
        }
    }

    // Expenses
    public Task<List<Expense>?> GetExpensesAsync(CancellationToken ct = default) => GetWithFallbackAsync<List<Expense>>("/api/expenses", ct)!;

    public async Task<Expense?> AddExpenseAsync(ExpenseEntryDto dto, CancellationToken ct = default)
    {
        Debug.WriteLine($"Adding expense: {dto.Note} - R{dto.Amount}");
        var resp = await PostWithFallbackAsync("/api/expenses", dto, ct);
        if (resp == null)
        {
            Debug.WriteLine("AddExpenseAsync: No response received");
            return null;
        }
        
        if (!resp.IsSuccessStatusCode)
        {
            var errorContent = await resp.Content.ReadAsStringAsync(ct);
            Debug.WriteLine($"AddExpenseAsync failed: {resp.StatusCode} - {errorContent}");
            return null;
        }
        
        var result = await resp.Content.ReadFromJsonAsync<Expense>(cancellationToken: ct);
        Debug.WriteLine($"AddExpenseAsync success: {result?.ExpenseId}");
        return result;
    }

    // Advice/Chat
    public async Task<AdviceResponseDto?> AskAsync(AdviceRequestDto req, CancellationToken ct = default)
    {
        Debug.WriteLine($"Asking advice question: {req.Question}");
        var resp = await PostWithFallbackAsync("/api/chat/message", req, ct);
        if (resp == null || !resp.IsSuccessStatusCode) 
        {
            Debug.WriteLine($"Ask failed: {resp?.StatusCode}");
            return null;
        }
        var result = await resp.Content.ReadFromJsonAsync<AdviceResponseDto>(cancellationToken: ct);
        Debug.WriteLine($"Ask success: received {result?.Answer?.Length ?? 0} char response");
        return result;
    }
}
