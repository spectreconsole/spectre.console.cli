namespace Spectre.Console.Tests.Data;

public class OptionVectorCommand : Command<OptionVectorSettings>
{
    protected override int Execute(CommandContext context, OptionVectorSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}