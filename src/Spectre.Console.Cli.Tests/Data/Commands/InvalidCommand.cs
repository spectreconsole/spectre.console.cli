namespace Spectre.Console.Tests.Data;

public sealed class InvalidCommand : Command<InvalidSettings>
{
    protected override int Execute(CommandContext context, InvalidSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}