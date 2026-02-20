using Spectre.Console.Cli.Completion;

namespace Spectre.Console.Tests.Unit.Cli;

public sealed class CommandLineParserTests
{
    [Fact]
    public void Should_Throw_If_Model_Is_Null()
    {
        // Given
        var settings = new FakeCommandAppSettings();

        // When
        var exception = Record.Exception(() => CommandLineParser.Parse(null!, settings, []));

        // Then
        exception.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("model");
    }

    [Fact]
    public void Should_Throw_If_Settings_Is_Null()
    {
        // Given
        var model = new FakeCommandModel();

        // When
        var exception = Record.Exception(() => CommandLineParser.Parse(model, null!, []));

        // Then
        exception.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("settings");
    }

    [Fact]
    public void Should_Throw_If_Model_Is_Not_Created_By_Spectre_Console_Cli()
    {
        // Given
        var model = new FakeCommandModel();
        var settings = new FakeCommandAppSettings();

        // When
        var exception = Record.Exception(() => CommandLineParser.Parse(model, settings, []));

        // Then
        exception.ShouldBeOfType<ArgumentException>()
            .ParamName.ShouldBe("model");
    }

    [Fact]
    public void Should_Handle_Null_Args_As_Empty()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.SetDefaultCommand<DogCommand>();
        });

        // When
        var result = CommandLineParser.Parse(model, settings, args: null!);

        // Then
        result.Command.ShouldNotBeNull();
        result.Command.IsDefaultCommand.ShouldBeTrue();
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.SettingsType.ShouldBe(typeof(DogSettings));
    }

    [Fact]
    public void Should_Return_Null_Command_For_Help()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });

        // When
        var result = CommandLineParser.Parse(model, settings, ["-h"]);

        // Then
        result.Command.ShouldBeNull();
        result.CommandType.ShouldBeNull();
        result.SettingsType.ShouldBeNull();
        result.MappedParameters.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Return_Types_And_Mapped_Parameters_For_Leaf_Command()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });

        // When
        var result = CommandLineParser.Parse(model, settings, new[]
        {
            "dog", "12", "4",
            "--good-boy",
            "--name", "Rufus",
            "--alive",
        });

        // Then
        result.Command.ShouldNotBeNull();
        result.Command!.Name.ShouldBe("dog");
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.SettingsType.ShouldBe(typeof(DogSettings));

        GetMappedValue(result, nameof(AnimalSettings.Legs)).ShouldBe("12");
        GetMappedValue(result, nameof(DogSettings.Age)).ShouldBe("4");
        GetMappedValue(result, nameof(DogSettings.GoodBoy)).ShouldBe("true");
        GetMappedValue(result, nameof(MammalSettings.Name)).ShouldBe("Rufus");
        GetMappedValue(result, nameof(AnimalSettings.IsAlive)).ShouldBe("true");

        result.RemainingArguments.Parsed.Count.ShouldBe(0);
        result.RemainingArguments.Raw.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Select_Default_Command_At_Root_When_No_Arguments()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.SetDefaultCommand<DogCommand>();
            config.AddCommand<CatCommand>("cat");
        });

        // When
        var result = CommandLineParser.Parse(model, settings, []);

        // Then
        result.Command.ShouldNotBeNull();
        result.Command!.IsDefaultCommand.ShouldBeTrue();
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.SettingsType.ShouldBe(typeof(DogSettings));
    }

    [Fact]
    public void Should_Put_Unknown_Options_In_Remaining_When_Relaxed()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = false;

        // When
        var result = CommandLineParser.Parse(model, settings, new[]
        {
            "dog", "12", "4",
            "--unknown", "value",
        });

        // Then
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.RemainingArguments.Parsed.Contains("--unknown").ShouldBeTrue();
        result.RemainingArguments.Parsed["--unknown"].ShouldContain("value");
        result.RemainingArguments.Raw.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Throw_On_Unknown_Options_When_Strict()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = true;

        // When / Then
        Should.Throw<CommandParseException>(() =>
            CommandLineParser.Parse(model, settings, ["dog", "12", "4", "--unknown", "value"]));
    }

    [Fact]
    public void Should_Allow_Unknown_Options_After_Delimiter_When_Strict()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = true;

        // When
        var result = CommandLineParser.Parse(model, settings, ["dog", "12", "4", "--", "--unknown", "value"]);

        // Then
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.RemainingArguments.Parsed.Contains("--unknown").ShouldBeTrue();
        result.RemainingArguments.Parsed["--unknown"].ShouldContain("value");
        result.RemainingArguments.Raw.Count.ShouldBe(2);
        result.RemainingArguments.Raw.ShouldBe(["--unknown", "value"]);
    }

    [Fact]
    public void Should_Infer_The_Default_Command_On_A_Branch_When_Relaxed()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddBranch<AnimalSettings>("animal", animal =>
            {
                animal.SetDefaultCommand<CatCommand>();
            });
        });
        settings.StrictParsing = false;

        // When
        var result = CommandLineParser.Parse(model, settings, new[]
        {
            "animal", "4", "-a", "false",
            "--name", "Kitty",
            "--agility", "four",
            "--nick-name", "Felix",
        });

        // Then
        result.Command.ShouldNotBeNull();
        result.Command!.IsDefaultCommand.ShouldBeTrue();
        result.CommandType.ShouldBe(typeof(CatCommand));
        result.SettingsType.ShouldBe(typeof(CatSettings));

        GetMappedValue(result, nameof(MammalSettings.Name)).ShouldBe("Kitty");
        GetMappedValue(result, nameof(CatSettings.Agility)).ShouldBe("four");

        result.RemainingArguments.Parsed.Contains("--nick-name").ShouldBeTrue();
        result.RemainingArguments.Parsed["--nick-name"].ShouldContain("Felix");
        result.RemainingArguments.Raw.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Infer_The_Default_Command_On_A_Branch_When_Strict()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddBranch<AnimalSettings>("animal", animal =>
            {
                animal.SetDefaultCommand<CatCommand>();
            });
        });
        settings.StrictParsing = true;

        // When
        var result = CommandLineParser.Parse(model, settings, new[]
        {
            "animal", "4", "-a", "false",
            "--name", "Kitty",
            "--agility", "four",
            "--", "--nick-name", "Felix",
        });

        // Then
        result.Command.ShouldNotBeNull();
        result.Command!.IsDefaultCommand.ShouldBeTrue();
        result.CommandType.ShouldBe(typeof(CatCommand));
        result.SettingsType.ShouldBe(typeof(CatSettings));

        GetMappedValue(result, nameof(MammalSettings.Name)).ShouldBe("Kitty");
        GetMappedValue(result, nameof(CatSettings.Agility)).ShouldBe("four");

        result.RemainingArguments.Parsed.Contains("--nick-name").ShouldBeTrue();
        result.RemainingArguments.Parsed["--nick-name"].ShouldContain("Felix");
        result.RemainingArguments.Raw.Count.ShouldBe(2);
        result.RemainingArguments.Raw.ShouldBe(["--nick-name", "Felix"]);
    }

    [Fact]
    public void Should_Throw_When_Assigning_A_Value_To_A_Flag_And_Conversion_Is_Disabled()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = false;
        settings.ConvertFlagsToRemainingArguments = false;

        // When / Then
        Should.Throw<CommandParseException>(() =>
            CommandLineParser.Parse(model, settings, ["dog", "--alive=indeterminate", "12", "4"]));
    }

    [Fact]
    public void Should_Convert_Flag_With_Invalid_Value_To_Remaining_When_Enabled()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = false;
        settings.ConvertFlagsToRemainingArguments = true;

        // When
        var result = CommandLineParser.Parse(model, settings, ["dog", "--alive=indeterminate", "12", "4"]);

        // Then
        result.CommandType.ShouldBe(typeof(DogCommand));
        result.MappedParameters.Any(p => p.Parameter.PropertyName == nameof(AnimalSettings.IsAlive)).ShouldBeFalse();
        result.RemainingArguments.Parsed.Contains("--alive").ShouldBeTrue();
        result.RemainingArguments.Parsed["--alive"].ShouldContain("indeterminate");
    }

    [Fact]
    public void Should_Respect_Command_Case_Sensitivity()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = true;

        settings.CaseSensitivity = CaseSensitivity.None;
        CommandLineParser.Parse(model, settings, ["DOG", "12", "4"]).CommandType.ShouldBe(typeof(DogCommand));

        settings.CaseSensitivity = CaseSensitivity.Commands;
        Should.Throw<CommandParseException>(() =>
            CommandLineParser.Parse(model, settings, ["DOG", "12", "4"]));
    }

    [Fact]
    public void Should_Respect_Long_Option_Case_Sensitivity()
    {
        // Given
        var (model, settings) = BuildModel(config =>
        {
            config.AddCommand<DogCommand>("dog");
        });
        settings.StrictParsing = true;

        // Long options are case-sensitive by default.
        settings.CaseSensitivity = CaseSensitivity.All;
        Should.Throw<CommandParseException>(() =>
            CommandLineParser.Parse(model, settings, ["dog", "12", "4", "--NAME", "Rufus"]));

        // Allow long options to be case-insensitive.
        settings.CaseSensitivity = CaseSensitivity.Commands;
        var result = CommandLineParser.Parse(model, settings, ["dog", "12", "4", "--NAME", "Rufus"]);
        GetMappedValue(result, nameof(MammalSettings.Name)).ShouldBe("Rufus");
    }

    private static string? GetMappedValue(CommandLineParseResult result, string propertyName)
    {
        return result.MappedParameters
            .Single(p => p.Parameter.PropertyName == propertyName)
            .Value;
    }

    private static (ICommandModel Model, CommandAppSettings Settings) BuildModel(Action<Configurator> configure)
    {
        var registrar = new FakeTypeRegistrar();
        var config = new Configurator(registrar);
        configure(config);

        var model = CommandModelBuilder.Build(config);
        return (model, config.Settings);
    }

    private sealed class FakeCommandModel : ICommandModel
    {
        public string ApplicationName => "fake";
        public string? ApplicationVersion => null;
        public IReadOnlyList<string[]> Examples => Array.Empty<string[]>();
        public IReadOnlyList<ICommandInfo> Commands => Array.Empty<ICommandInfo>();
        public ICommandInfo? DefaultCommand => null;
    }

    private sealed class FakeCommandAppSettings : ICommandAppSettings
    {
        public CultureInfo? Culture { get; set; }
        public string? ApplicationName { get; set; }
        public string? ApplicationVersion { get; set; }
        public int MaximumIndirectExamples { get; set; }
        public bool ShowOptionDefaultValues { get; set; }
        public bool TrimTrailingPeriod { get; set; }
        public HelpProviderStyle? HelpProviderStyles { get; set; }
        public IAnsiConsole? Console { get; set; }
        public ICommandInterceptor? Interceptor { get; set; }
        public ITypeRegistrarFrontend Registrar { get; } = new TypeRegistrar(new FakeTypeRegistrar());
        public CaseSensitivity CaseSensitivity { get; set; } = CaseSensitivity.All;
        public bool StrictParsing { get; set; }
        public bool ConvertFlagsToRemainingArguments { get; set; }
        public bool PropagateExceptions { get; set; }
        public int CancellationExitCode { get; set; }
        public bool ValidateExamples { get; set; }
        public Func<Exception, ITypeResolver?, int>? ExceptionHandler { get; set; }
    }
}
