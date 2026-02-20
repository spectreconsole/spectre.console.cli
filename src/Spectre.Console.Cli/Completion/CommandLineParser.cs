using static Spectre.Console.Cli.CommandTreeTokenizer;

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
        var parsedResult = ParseCommandLineArguments(internalModel, settings, arguments);

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

    private static CommandTreeParserResult ParseCommandLineArguments(CommandModel model, ICommandAppSettings settings, IReadOnlyList<string> args)
    {
        CommandTreeParserResult? parsedResult = null;

        try
        {
            (parsedResult, var tokenizerResult) = InternalParseCommandLineArguments(model, settings, args);

            var lastParsedLeaf = parsedResult.Tree?.GetLeafCommand();
            var lastParsedCommand = lastParsedLeaf?.Command;

            if (lastParsedLeaf != null && lastParsedCommand is { IsBranch: true } && !lastParsedLeaf.ShowHelp &&
                lastParsedCommand.DefaultCommand != null)
            {
                // Adjust for any parsed remaining arguments by
                // inserting the default command ahead of them.
                var position = tokenizerResult.Tokens.Position;
                foreach (var parsedRemaining in parsedResult.Remaining.Parsed)
                {
                    position--;
                    position -= parsedRemaining.Count(value => value != null);
                }
                position = position < 0 ? 0 : position;

                // Insert this branch's default command into the command line
                // arguments and try again to see if it will parse.
                var argsWithDefaultCommand = new List<string>(args);
                argsWithDefaultCommand.Insert(position, lastParsedCommand.DefaultCommand.Name);

                (parsedResult, _) = InternalParseCommandLineArguments(model, settings, argsWithDefaultCommand);
            }
        }
        catch (CommandParseException) when (parsedResult == null && GetParsingMode(settings) == ParsingMode.Strict)
        {
            // The parsing exception might be resolved by adding in the default command,
            // but we can't know for sure. Take a brute force approach and try this for
            // every position between the arguments.
            for (var i = 0; i < args.Count; i++)
            {
                var argsWithDefaultCommand = new List<string>(args);
                argsWithDefaultCommand.Insert(args.Count - i, CliConstants.DefaultCommandName);

                try
                {
                    (parsedResult, _) = InternalParseCommandLineArguments(model, settings, argsWithDefaultCommand);
                    break;
                }
                catch (CommandParseException)
                {
                    // Continue.
                }
            }

            if (parsedResult == null)
            {
                // Failed to parse having inserted the default command between each argument.
                // Repeat the parsing of the original arguments to throw the correct exception.
                InternalParseCommandLineArguments(model, settings, args);
            }
        }

        if (parsedResult == null)
        {
            throw CommandParseException.UnknownParsingError();
        }

        return parsedResult;
    }

    private static (CommandTreeParserResult ParserResult, CommandTreeTokenizerResult TokenizerResult) InternalParseCommandLineArguments(
        CommandModel model,
        ICommandAppSettings settings,
        IReadOnlyList<string> args)
    {
        var parsingMode = GetParsingMode(settings);
        var parser = new CommandTreeParser(model, settings.CaseSensitivity, parsingMode, settings.ConvertFlagsToRemainingArguments);

        var parserContext = new CommandTreeParserContext(args, parsingMode);
        var tokenizerResult = CommandTreeTokenizer.Tokenize(args);
        var parsedResult = parser.Parse(parserContext, tokenizerResult);

        return (parsedResult, tokenizerResult);
    }

    private static ParsingMode GetParsingMode(ICommandAppSettings settings)
    {
        return settings.StrictParsing ? ParsingMode.Strict : ParsingMode.Relaxed;
    }
}

