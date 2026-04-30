using Spectre.Console.Cli.Completion;

namespace Spectre.Console.Tests.Unit.Cli;

public sealed class TypeResolverAccessorTests
{
    [Fact]
    public void Should_Inject_CommandModel_Settings_And_ResolverAccessor()
    {
        // Given
        var sink = new CaptureSink();
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.Settings.Registrar.RegisterInstance(sink);

            config.AddBranch<AnimalSettings>("animal", animal =>
            {
                animal.SetDefaultCommand<CatCommand>();
            });

            config.AddCommand<CaptureCommand>("capture");
        });

        // When
        var result = app.Run("capture");

        // Then
        result.ExitCode.ShouldBe(0);

        sink.Model.ShouldNotBeNull();
        sink.Settings.ShouldNotBeNull();
        sink.Accessor.ShouldNotBeNull();

        sink.ResolverDuringExecution.ShouldNotBeNull();
        sink.Accessor!.Resolver.ShouldBeNull();

        sink.ResolvedModelViaAccessor.ShouldNotBeNull();
        sink.ResolvedSettingsViaAccessor.ShouldNotBeNull();

        sink.ParseResult.ShouldNotBeNull();
        sink.ParseResult!.CommandType.ShouldBe(typeof(CatCommand));
        sink.ParseResult.SettingsType.ShouldBe(typeof(CatSettings));
        sink.ParseResult.Command.ShouldNotBeNull();
        sink.ParseResult.Command!.IsDefaultCommand.ShouldBeTrue();
        GetMappedValue(sink.ParseResult, nameof(MammalSettings.Name)).ShouldBe("Kitty");
    }

    [Fact]
    public void Should_Reset_ResolverAccessor_When_Command_Throws()
    {
        // Given
        var sink = new CaptureSink();
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.Settings.Registrar.RegisterInstance(sink);
            config.AddCommand<ThrowingCaptureCommand>("throw");
        });

        // When
        var exception = Record.Exception(() => app.Run("throw"));

        // Then
        exception.ShouldBeOfType<InvalidOperationException>();
        sink.Accessor.ShouldNotBeNull();
        sink.ResolverDuringExecution.ShouldNotBeNull();
        sink.Accessor!.Resolver.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_And_Clear_ResolverAccessor_On_Multiple_Runs()
    {
        // Given
        var sink = new CaptureSink();
        var app = new CommandAppTester();
        app.Configure(config =>
        {
            config.PropagateExceptions();
            config.Settings.Registrar.RegisterInstance(sink);
            config.AddBranch<AnimalSettings>("animal", animal =>
            {
                animal.SetDefaultCommand<CatCommand>();
            });
            config.AddCommand<CaptureCommand>("capture");
        });

        // When
        app.Run("capture");
        app.Run("capture");

        // Then
        sink.Executions.ShouldBe(2);
        sink.ResolverNonNullExecutions.ShouldBe(2);
        sink.Accessor.ShouldNotBeNull();
        sink.Accessor!.Resolver.ShouldBeNull();
    }

    private static string? GetMappedValue(CommandLineParseResult result, string propertyName)
    {
        return result.MappedParameters
            .Single(p => p.Parameter.PropertyName == propertyName)
            .Value;
    }

    public sealed class CaptureCommand : Command<EmptyCommandSettings>
    {
        private readonly ICommandModel _model;
        private readonly ICommandAppSettings _settings;
        private readonly ITypeResolverAccessor _resolverAccessor;
        private readonly CaptureSink _sink;

        public CaptureCommand(ICommandModel model, ICommandAppSettings settings, ITypeResolverAccessor resolverAccessor, CaptureSink sink)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _resolverAccessor = resolverAccessor ?? throw new ArgumentNullException(nameof(resolverAccessor));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
        {
            _sink.Executions++;
            _sink.Model = _model;
            _sink.Settings = _settings;
            _sink.Accessor = _resolverAccessor;

            _sink.ResolverDuringExecution = _resolverAccessor.Resolver;
            if (_resolverAccessor.Resolver != null)
            {
                _sink.ResolverNonNullExecutions++;
                _sink.ResolvedModelViaAccessor = _resolverAccessor.Resolver.Resolve(typeof(ICommandModel));
                _sink.ResolvedSettingsViaAccessor = _resolverAccessor.Resolver.Resolve(typeof(ICommandAppSettings));
            }

            _sink.ParseResult = CommandLineParser.Parse(_model, _settings, ["animal", "4", "--name", "Kitty"]);
            return 0;
        }
    }

    public sealed class ThrowingCaptureCommand : Command<EmptyCommandSettings>
    {
        private readonly ITypeResolverAccessor _resolverAccessor;
        private readonly CaptureSink _sink;

        public ThrowingCaptureCommand(ITypeResolverAccessor resolverAccessor, CaptureSink sink)
        {
            _resolverAccessor = resolverAccessor ?? throw new ArgumentNullException(nameof(resolverAccessor));
            _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        }

        protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
        {
            _sink.Accessor = _resolverAccessor;
            _sink.ResolverDuringExecution = _resolverAccessor.Resolver;
            throw new InvalidOperationException("Boom");
        }
    }

    public sealed class CaptureSink
    {
        public int Executions { get; set; }
        public int ResolverNonNullExecutions { get; set; }

        public ICommandModel? Model { get; set; }
        public ICommandAppSettings? Settings { get; set; }
        public ITypeResolverAccessor? Accessor { get; set; }

        public ITypeResolver? ResolverDuringExecution { get; set; }
        public object? ResolvedModelViaAccessor { get; set; }
        public object? ResolvedSettingsViaAccessor { get; set; }

        public CommandLineParseResult? ParseResult { get; set; }
    }
}
