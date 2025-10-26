namespace Spectre.Console.Tests.Data;

public sealed class VersionCommand : Command<VersionSettings>
{
    private readonly IAnsiConsole _console;

    public VersionCommand(IAnsiConsole console)
    {
        _console = console;
    }

    protected override int Execute(CommandContext context, VersionSettings settings, CancellationToken cancellationToken)
    {
        _console.WriteLine($"VersionCommand ran, Version: {settings.Version ?? string.Empty}");

        return 0;
    }
}