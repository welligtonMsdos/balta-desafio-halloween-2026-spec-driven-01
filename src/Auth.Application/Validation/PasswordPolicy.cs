namespace Auth.Application.Validation;

public static class PasswordPolicy
{
    public static IReadOnlyCollection<string> GetViolations(string? password)
    {
        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(password) || password.Length < 10)
        {
            violations.Add("A senha deve possuir no mínimo 10 caracteres.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsUpper))
        {
            violations.Add("A senha deve conter uma letra maiúscula.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsLower))
        {
            violations.Add("A senha deve conter uma letra minúscula.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsDigit))
        {
            violations.Add("A senha deve conter um número.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(character => !char.IsLetterOrDigit(character)))
        {
            violations.Add("A senha deve conter um caractere especial.");
        }

        return violations;
    }

    public static void EnsureValid(string? password)
    {
        var violations = GetViolations(password);
        if (violations.Count > 0)
        {
            throw new ArgumentException(string.Join(' ', violations), nameof(password));
        }
    }
}
