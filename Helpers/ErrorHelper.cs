namespace dZENcode_backend.Helpers;

public class ErrorHelper
{
    private Dictionary<string, List<string>> _errors { get; } = new Dictionary<string, List<string>>();

    public void AddError(string key, string errorMessage)
    {
        if (!_errors.ContainsKey(key))
        {
            _errors[key] = new List<string>();
        }

        _errors[key].Add(errorMessage);
    }

    public bool HasErrors() => _errors.Any();

    public void ClearErrors() => _errors.Clear();

    public object GetNewErrorResponse(string key, string value)
    {
        var error = new Dictionary<string, string[]>
        {
            { key, new[] { value } }
        };
        return new { errors = error };
    }

    public Dictionary<string, List<string>> GetErrors() => _errors;
}