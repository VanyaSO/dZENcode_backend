namespace dZENcode_backend.Helpers;

public static class ErrorHelper
{
    private static Dictionary<string, List<string>> _errors { get; } = new Dictionary<string, List<string>>();

    public static void AddError(string key, string errorMessage)
    {
        if (!_errors.ContainsKey(key))
        {
            _errors[key] = new List<string>();
        }

        _errors[key].Add(errorMessage);
    }

    public static bool HasErrors() => _errors.Any();

    public static void ClearErrors() => _errors.Clear();

    public static object GetNewErrorResponse(string key, string value)
    {
        var error = new Dictionary<string, string[]>
        {
            { key, new[] { value } }
        };
        return new { errors = error };
    }

    public static object GetErrorsResponse() => new { errors = _errors };
}