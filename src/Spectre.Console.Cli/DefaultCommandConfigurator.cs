namespace Spectre.Console.Cli.Internal.Configuration;

/// <summary>
/// Fluent configurator for the default command.
/// </summary>
public sealed class DefaultCommandConfigurator
{
    private readonly ConfiguredCommand _defaultCommand;

    internal DefaultCommandConfigurator(ConfiguredCommand defaultCommand)
    {
        _defaultCommand = defaultCommand;
    }

    /// <summary>
    /// Sets the description of the default command.
    /// </summary>
    /// <param name="description">The default command description.</param>
    /// <returns>The same <see cref="DefaultCommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public DefaultCommandConfigurator WithDescription(string description)
    {
        _defaultCommand.Description = description;
        return this;
    }

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="data">The data to pass to the default command.</param>
    /// <returns>The same <see cref="DefaultCommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    public DefaultCommandConfigurator WithData(object data)
    {
        _defaultCommand.Data = data;
        return this;
    }

    /// <summary>
    /// Adds an exit code that can be returned by the default command.
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="description">The description of the exit code.</param>
    /// <returns>The same <see cref="DefaultCommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// This documents the exit code in help output. It does not validate the value returned by the default command.
    /// Descriptions are trimmed before being stored.
    /// Help combines global and default command exit codes; the default command's description
    /// takes precedence for duplicate codes.
    /// </remarks>
    public DefaultCommandConfigurator WithExitCode(int exitCode, string description)
    {
        var normalized = ExitCodeValidation.ValidateAndNormalize(exitCode, description, _defaultCommand.ExitCodes, _defaultCommand.Name, isDefaultCommand: true);
        _defaultCommand.ExitCodes.Add(exitCode, normalized);
        return this;
    }
}