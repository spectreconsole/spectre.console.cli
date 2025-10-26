namespace Spectre.Console.Tests.Data;

[Description("The giraffe command.")]
public sealed class GiraffeCommand : Command<GiraffeSettings>
{
    protected override int Execute(CommandContext context, GiraffeSettings settings, CancellationToken cancellationToken)
    {
        return 0;
    }
}