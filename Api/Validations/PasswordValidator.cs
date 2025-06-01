using System.Text.RegularExpressions;

namespace Api.Validations;

public static class PasswordValidator
{
    public static bool IsValid(string password)
    {
        return Regex.IsMatch(password, @"[a-zA-Z]") && Regex.IsMatch(password, @"\d");
    }
}
