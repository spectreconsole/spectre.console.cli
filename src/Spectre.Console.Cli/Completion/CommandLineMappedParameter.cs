namespace Spectre.Console.Cli.Completion;

/// <summary>
/// Represents a mapped parameter/value pair from a parsed command line.
/// </summary>
public sealed class CommandLineMappedParameter
{
    /// <summary>
    /// Gets the mapped parameter.
    /// </summary>
    public ICommandParameterInfo Parameter { get; }

    /// <summary>
    /// Gets the mapped value, if any.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineMappedParameter"/> class.
    /// </summary>
    public CommandLineMappedParameter(ICommandParameterInfo parameter, string? value)
    {
        Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        Value = value;
    }
}
