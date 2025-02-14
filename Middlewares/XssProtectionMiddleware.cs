using System.Text;
using System.Text.RegularExpressions;

namespace dZENcode_backend.Middlewares;

public class XssProtectionMiddleware
{
    private readonly RequestDelegate _next;
    public XssProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.QueryString.HasValue)
        {
            var sanitizedQuery = new StringBuilder();
            var originalQuery = context.Request.Query;

            foreach (var (key, value) in originalQuery)
            {
                var cleanKey = CleanInput(key);
                var cleanValue = CleanInput(value);

                sanitizedQuery.Append($"{cleanKey}={cleanValue}&");
            }

            var sanitizedQueryString = sanitizedQuery.ToString().TrimEnd('&');
            context.Request.QueryString = new QueryString($"?{sanitizedQueryString}");
        }

        if (context.Request.ContentType != null && (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put))
        {
            context.Request.EnableBuffering();

            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var sanitizedBody = CleanInput(body);

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sanitizedBody));
            context.Request.Body = memoryStream;
            context.Request.Body.Seek(0, SeekOrigin.Begin);
        }
        
        await _next(context);
    }
    
    private string CleanInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        return Regex.Replace(input, @"<.*?>|javascript:", string.Empty, RegexOptions.IgnoreCase);
    }
}
