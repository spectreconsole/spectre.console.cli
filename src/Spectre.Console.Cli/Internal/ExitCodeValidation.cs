namespace Spectre.Console.Cli;

internal static class ExitCodeValidation
{
    public static string ValidateAndNormalize(
        int exitCode,
        string description,
        IReadOnlyDictionary<int, string> existing,
        string? commandName,
        bool isDefaultCommand = false)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw CommandConfigurationException.InvalidExitCodeDescription();
        }

        if (existing.ContainsKey(exitCode))
        {
            throw CommandConfigurationException.DuplicateExitCode(exitCode, commandName, isDefaultCommand);
        }

        return description.Trim();
    }
}
