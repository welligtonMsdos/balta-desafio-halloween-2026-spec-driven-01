using System.Net.Mail;

namespace Auth.Application.Validation;

public sealed record EmailAddress(string Value, string NormalizedValue)
{
    public static EmailAddress Create(string? value)
    {
        var trimmedValue = value?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            throw new ArgumentException("E-mail é obrigatório.", nameof(value));
        }

        try
        {
            var parsedAddress = new MailAddress(trimmedValue);
            if (!string.Equals(parsedAddress.Address, trimmedValue, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("E-mail possui formato inválido.", nameof(value));
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException("E-mail possui formato inválido.", nameof(value));
        }

        return new EmailAddress(trimmedValue, trimmedValue.ToLowerInvariant());
    }
}
