namespace Spectre.Console.Tests.Data;

public class CatCommand : AnimalCommand<CatSettings>
{
    protected override int Execute(CommandContext context, CatSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}