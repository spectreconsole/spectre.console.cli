namespace Spectre.Console.Tests.Data;

[Description("The lion command.")]
public class LionCommand : AnimalCommand<LionSettings>
{
    protected override int Execute(CommandContext context, LionSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}