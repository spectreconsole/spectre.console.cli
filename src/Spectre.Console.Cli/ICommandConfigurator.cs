namespace Spectre.Console.Cli;

/// <summary>
/// Represents a command configurator.
/// </summary>
public interface ICommandConfigurator
{
    /// <summary>
    /// Adds an example of how to use the command.
    /// </summary>
    /// <param name="args">The example arguments.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    ICommandConfigurator WithExample(params string[] args);

    /// <summary>
    /// Adds an alias (an alternative name) to the command being configured.
    /// </summary>
    /// <param name="name">The alias to add to the command being configured.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    ICommandConfigurator WithAlias(string name);

    /// <summary>
    /// Sets the description of the command.
    /// </summary>
    /// <param name="description">The command description.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    ICommandConfigurator WithDescription(string description);

    /// <summary>
    /// Sets data that will be passed to the command via the <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="data">The data to pass to the command.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    ICommandConfigurator WithData(object data);

    /// <summary>
    /// Marks the command as hidden.
    /// Hidden commands do not show up in help documentation or
    /// generated XML models.
    /// </summary>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    ICommandConfigurator IsHidden();

    /// <summary>
    /// Adds an exit code that can be returned by the command.
    /// </summary>
    /// <param name="exitCode">The exit code.</param>
    /// <param name="description">The description of the exit code.</param>
    /// <returns>The same <see cref="ICommandConfigurator"/> instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// This documents the exit code in help output. It does not validate the value returned by the command.
    /// Descriptions are trimmed before being stored.
    /// Help combines global and command-specific exit codes; this command's description
    /// takes precedence for duplicate codes.
    /// </remarks>
    ICommandConfigurator WithExitCode(int exitCode, string description);
}