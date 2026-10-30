namespace Spectre.Console.Tests.Unit.Cli;

public sealed partial class CommandAppTests
{
    public sealed class Configuration
    {
        [Fact]
        public void Should_Throw_When_Global_Exit_Code_Is_Defined_Twice()
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.Configure(config => config
                    .AddExitCode(1, "First")
                    .AddExitCode(1, "Second")));

            // Then
            result.Message.ShouldContain("already been defined globally");
        }

        [Fact]
        public void Should_Throw_When_Command_Exit_Code_Is_Defined_Twice()
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.Configure(config => config
                    .AddCommand<DogCommand>("dog")
                    .WithExitCode(1, "First")
                    .WithExitCode(1, "Second")));

            // Then
            result.Message.ShouldContain("already been defined for command 'dog'");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Should_Throw_When_Global_Exit_Code_Description_Is_Missing(string? description)
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.Configure(config => config.AddExitCode(1, description!)));

            // Then
            result.Message.ShouldContain("exit code description");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Should_Throw_When_Command_Exit_Code_Description_Is_Missing(string? description)
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.Configure(config => config
                    .AddCommand<DogCommand>("dog")
                    .WithExitCode(1, description!)));

            // Then
            result.Message.ShouldContain("exit code description");
        }

        [Fact]
        public void Should_Throw_When_Default_Command_Exit_Code_Is_Defined_Twice()
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.SetDefaultCommand<EmptyCommand>()
                    .WithExitCode(1, "First")
                    .WithExitCode(1, "Second"));

            // Then
            result.Message.ShouldContain("defined for the default command");
        }

        [Fact]
        public void Should_Trim_Exit_Code_Descriptions()
        {
            // Given
            var fixture = new CommandAppTester();
            fixture.Configure(config => config
                .AddExitCode(0, "  Command completed successfully  ")
                .AddCommand<DogCommand>("dog")
                .WithExitCode(2, "  Dog validation failed  "));

            // When
            var result = fixture.Run("dog", "--help");

            // Then
            var lines = result.Output.ReplaceLineEndings("\n").Split('\n');
            lines.ShouldContain("    0    Command completed successfully");
            lines.ShouldContain("    2    Dog validation failed");
        }

        [Fact]
        public void Should_Throw_When_Default_Command_Exit_Code_Description_Is_Missing()
        {
            // Given
            var app = new CommandApp();

            // When
            var result = Should.Throw<CommandConfigurationException>(() =>
                app.SetDefaultCommand<DogCommand>()
                    .WithExitCode(1, " "));

            // Then
            result.Message.ShouldContain("exit code description");
        }
    }
}
