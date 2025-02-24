using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

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
                var cleanKey = CleanString(key);
                var cleanValue = CleanString(value);

                sanitizedQuery.Append($"{cleanKey}={cleanValue}&");
            }

            var sanitizedQueryString = sanitizedQuery.ToString().TrimEnd('&');
            context.Request.QueryString = new QueryString($"?{sanitizedQueryString}");
        }

        if (context.Request.ContentType != null && (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put))
        {
            if (context.Request.ContentType.Contains("multipart/form-data"))
            {
                var form = await context.Request.ReadFormAsync();
                var sanitizedForm = new Dictionary<string, StringValues>();

                foreach (var field in form)
                {
                    if (form.Files.Any(f => f.Name == field.Key))
                        sanitizedForm[field.Key] = field.Value;
                    else
                        sanitizedForm[field.Key] = CleanString(field.Value);
                }

                var formCollection = new FormCollection(sanitizedForm, form.Files);
                context.Request.Form = formCollection;
            } 
            else if (context.Request.ContentType.Contains("application/json"))
            {
                context.Request.EnableBuffering();

                var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
                var sanitizedBody = CleanString(body);
                
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(sanitizedBody));
                context.Request.Body = memoryStream;
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }
        }
        
        await _next(context);
    }
    
    private string CleanString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        
        return Regex.Replace(str, @"<script.*?>.*?</script>|on\w+\s*=\s*(['""]).*?\1|\s*href\s*=\s*(['""])javascript:[^'""]*\2", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

    }
}
