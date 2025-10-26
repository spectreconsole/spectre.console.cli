namespace Spectre.Console.Tests.Data;

public sealed class EmptyCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}