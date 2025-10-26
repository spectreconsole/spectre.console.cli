namespace Spectre.Console.Tests.Data;

[Description("The horse command.")]
public class HorseCommand : AnimalCommand<HorseSettings>
{
    protected override int Execute(CommandContext context, HorseSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}