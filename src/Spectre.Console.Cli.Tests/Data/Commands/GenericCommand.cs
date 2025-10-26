namespace Spectre.Console.Tests.Data;

public sealed class GenericCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings
{
    protected override int Execute(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}