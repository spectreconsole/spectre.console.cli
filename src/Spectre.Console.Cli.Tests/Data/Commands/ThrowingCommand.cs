namespace Spectre.Console.Tests.Data;

public sealed class ThrowingCommand : Command<ThrowingCommandSettings>
{
    protected override int Execute(CommandContext context, ThrowingCommandSettings settings, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("W00t?");
    }
}