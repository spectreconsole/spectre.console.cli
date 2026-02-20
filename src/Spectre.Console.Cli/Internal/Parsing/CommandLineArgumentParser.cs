using static Spectre.Console.Cli.CommandTreeTokenizer;

namespace Spectre.Console.Cli;

internal static class CommandLineArgumentParser
{
    internal static CommandTreeParserResult ParseWithDefaults(CommandModel model, ICommandAppSettings settings, IReadOnlyList<string> args)
    {
        CommandTreeParserResult? parsedResult = null;

        try
        {
            (parsedResult, var tokenizerResult) = InternalParse(model, settings, args);

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

                (parsedResult, _) = InternalParse(model, settings, argsWithDefaultCommand);
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
                    (parsedResult, _) = InternalParse(model, settings, argsWithDefaultCommand);
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
                InternalParse(model, settings, args);
            }
        }

        if (parsedResult == null)
        {
            throw CommandParseException.UnknownParsingError();
        }

        return parsedResult;
    }

    internal static (CommandTreeParserResult ParserResult, CommandTreeTokenizerResult TokenizerResult) InternalParse(
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

    internal static ParsingMode GetParsingMode(ICommandAppSettings settings)
    {
        return settings.StrictParsing ? ParsingMode.Strict : ParsingMode.Relaxed;
    }
}

