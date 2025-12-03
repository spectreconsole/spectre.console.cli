
namespace Spectre.Console.Tests.Unit.Cli.Annotations;

public sealed partial class CommandOptionAttributeTests
{
    [Fact]
    public void Should_Write_Deprecation_Warning()
    {
        //Given, When
        var fixture = new CommandAppTester();
        fixture.Configure(configurator => configurator.AddCommand<DeprecatedOptionCommand>("cmd"));
        var result = fixture.Run("cmd", "-d", "yes");

        // Then
        result.Output.ShouldContain("Warning");
        result.Output.ShouldContain("This option is deprecated and subject to removal.");
    }

    private sealed class DeprecatedOptionSettings : CommandSettings
    {
        [CommandOption("-d|--deprecated <VALUE>", true, "This option is deprecated and subject to removal.")]
        public string? Deprecated { get; set; }
    }

    private sealed class DeprecatedOptionCommand : Command<DeprecatedOptionSettings>
    {
        protected override int Execute(CommandContext context, DeprecatedOptionSettings settings, CancellationToken cancellationToken)
        {
            return 0;
        }
    }
}