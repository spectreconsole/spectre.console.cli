namespace Spectre.Console.Cli.Completion;

/// <summary>
/// Parses command line arguments using the Spectre.Console.Cli command model.
/// </summary>
public static class CommandLineParser
{
    /// <summary>
    /// Parses the command line against the specified model.
    /// </summary>
    /// <remarks>
    /// This parser supports models created by Spectre.Console.Cli itself.
    /// </remarks>
    public static CommandLineParseResult Parse(Help.ICommandModel model, ICommandAppSettings settings, IEnumerable<string> args)
    {
        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var internalModel = model as CommandModel;
        if (internalModel == null)
        {
            throw new ArgumentException("The command model must be created by Spectre.Console.Cli.", nameof(model));
        }

        var arguments = args.ToSafeReadOnlyList();
        var parsedResult = CommandLineArgumentParser.ParseWithDefaults(internalModel, settings, arguments);

        var leaf = parsedResult.Tree?.GetLeafCommand();
        var command = (Help.ICommandInfo?)leaf?.Command;
        var mapped = leaf?.Mapped
            .Select(p => new CommandLineMappedParameter(p.Parameter, p.Value))
            .ToList()
            ?? new List<CommandLineMappedParameter>();

        return new CommandLineParseResult(
            command,
            leaf?.Command.CommandType,
            leaf?.Command.SettingsType,
            mapped.AsReadOnly(),
            parsedResult.Remaining);
    }


}

