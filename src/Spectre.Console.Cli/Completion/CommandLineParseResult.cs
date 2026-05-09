namespace Spectre.Console.Cli.Completion;

/// <summary>
/// Represents the result of parsing a command line against a command model.
/// </summary>
public sealed class CommandLineParseResult
{
    /// <summary>
    /// Gets the parsed command context (leaf command), if any.
    /// </summary>
    public Help.ICommandInfo? Command { get; }

    /// <summary>
    /// Gets the parsed command type, if any.
    /// </summary>
    public Type? CommandType { get; }

    /// <summary>
    /// Gets the parsed settings type, if any.
    /// </summary>
    public Type? SettingsType { get; }

    /// <summary>
    /// Gets the mapped parameters for the leaf command.
    /// </summary>
    public IReadOnlyList<CommandLineMappedParameter> MappedParameters { get; }

    /// <summary>
    /// Gets the remaining arguments.
    /// </summary>
    public IRemainingArguments RemainingArguments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineParseResult"/> class.
    /// </summary>
    public CommandLineParseResult(
        Help.ICommandInfo? command,
        Type? commandType,
        Type? settingsType,
        IReadOnlyList<CommandLineMappedParameter> mappedParameters,
        IRemainingArguments remainingArguments)
    {
        Command = command;
        CommandType = commandType;
        SettingsType = settingsType;
        MappedParameters = mappedParameters ?? throw new ArgumentNullException(nameof(mappedParameters));
        RemainingArguments = remainingArguments ?? throw new ArgumentNullException(nameof(remainingArguments));
    }
}
