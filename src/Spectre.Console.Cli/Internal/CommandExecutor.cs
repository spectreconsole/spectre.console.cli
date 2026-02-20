namespace Spectre.Console.Cli;

internal sealed class CommandExecutor
{
    private readonly ITypeRegistrar _registrar;

    public CommandExecutor(ITypeRegistrar registrar)
    {
        _registrar = registrar ?? throw new ArgumentNullException(nameof(registrar));
        _registrar.Register(typeof(DefaultPairDeconstructor), typeof(DefaultPairDeconstructor));
    }

    public async Task<int> ExecuteAsync(IConfiguration configuration, IEnumerable<string> args, CancellationToken cancellationToken)
    {
        CommandTreeParserResult parsedResult;

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var arguments = args.ToSafeReadOnlyList();

        _registrar.RegisterInstance(typeof(IConfiguration), configuration);
        _registrar.RegisterInstance(typeof(ICommandAppSettings), configuration.Settings);
        _registrar.RegisterLazy(typeof(IAnsiConsole), () => configuration.Settings.Console.GetConsole());

        var resolverAccessor = new TypeResolverAccessor();
        _registrar.RegisterInstance(typeof(ITypeResolverAccessor), resolverAccessor);

        // Create the command model.
        var model = CommandModelBuilder.Build(configuration);
        _registrar.RegisterInstance(typeof(CommandModel), model);
        _registrar.RegisterInstance(typeof(Spectre.Console.Cli.Help.ICommandModel), model);
        _registrar.RegisterDependencies(model);

        // Got at least one argument?
        var firstArgument = arguments.FirstOrDefault();
        if (firstArgument != null)
        {
            // Asking for version?
            if (firstArgument.Equals("-v", StringComparison.OrdinalIgnoreCase) ||
                firstArgument.Equals("--version", StringComparison.OrdinalIgnoreCase))
            {
                if (configuration.Settings.ApplicationVersion != null)
                {
                    // We need to check if the command has a version option on its setting class.
                    // Do this by first parsing the command line args and checking the remaining args.
                    try
                    {
                        // Parse and map the model against the arguments.
                        parsedResult = CommandLineArgumentParser.ParseWithDefaults(model, configuration.Settings, arguments);
                    }
                    catch (Exception)
                    {
                        // Something went wrong with parsing the command line arguments,
                        // however we know the first argument is a version option.
                        var console = configuration.Settings.Console.GetConsole();
                        console.MarkupLine(configuration.Settings.ApplicationVersion);
                        return 0;
                    }

                    // Check the parsed remaining args for the version options.
                    if ((firstArgument.Equals("-v", StringComparison.OrdinalIgnoreCase) && parsedResult.Remaining.Parsed.Contains("-v")) ||
                        (firstArgument.Equals("--version", StringComparison.OrdinalIgnoreCase) && parsedResult.Remaining.Parsed.Contains("--version")))
                    {
                        // The version option is not a member of the command settings.
                        var console = configuration.Settings.Console.GetConsole();
                        console.MarkupLine(configuration.Settings.ApplicationVersion);
                        return 0;
                    }
                }
            }

            // OpenCLI?
            if (firstArgument.Equals(CliConstants.DumpHelpOpenCliOption, StringComparison.OrdinalIgnoreCase))
            {
                // Replace all arguments with the opencli command
                arguments = ["cli", "opencli"];
            }
        }

        // Parse and map the model against the arguments.
        parsedResult = CommandLineArgumentParser.ParseWithDefaults(model, configuration.Settings, arguments);

        // Register the arguments with the container.
        _registrar.RegisterInstance(typeof(CommandTreeParserResult), parsedResult);
        _registrar.RegisterInstance(typeof(IRemainingArguments), parsedResult.Remaining);

        // Create the resolver.
        using (var resolver = new TypeResolverAdapter(_registrar.Build()))
        {
            resolverAccessor.Resolver = resolver;
            try
            {
                // Get the registered help provider, falling back to the default provider
                // if no custom implementations have been registered.
                var helpProviders = resolver.Resolve(typeof(IEnumerable<IHelpProvider>)) as IEnumerable<IHelpProvider>;
                var helpProvider = helpProviders?.LastOrDefault() ?? new HelpProvider(configuration.Settings);

                // Currently the root?
                if (parsedResult.Tree == null)
                {
                    // Display help.
                    configuration.Settings.Console.SafeRender(helpProvider.Write(model, null));
                    return 0;
                }

                // Get the command to execute.
                var leaf = parsedResult.Tree.GetLeafCommand();
                if (leaf.Command.IsBranch || leaf.ShowHelp)
                {
                    // Branches can't be executed. Show help.
                    configuration.Settings.Console.SafeRender(helpProvider.Write(model, leaf.Command));
                    return leaf.ShowHelp ? 0 : 1;
                }

                // Is this the default and is it called without arguments when there are required arguments?
                if (leaf.Command.IsDefaultCommand && arguments.Count == 0 && leaf.Command.Parameters.Any(p => p.IsRequired))
                {
                    // Display help for default command.
                    configuration.Settings.Console.SafeRender(helpProvider.Write(model, leaf.Command));
                    return 1;
                }

                // Create the content.
                var context = new CommandContext(
                    arguments,
                    parsedResult.Remaining,
                    leaf.Command.Name,
                    leaf.Command.Data);

                // Execute the command tree.
                return await ExecuteAsync(leaf, parsedResult.Tree, context, resolver, configuration, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                resolverAccessor.Resolver = null;
            }
        }
    }

    private static async Task<int> ExecuteAsync(
        CommandTree leaf,
        CommandTree tree,
        CommandContext context,
        ITypeResolver resolver,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        try
        {
            // Bind the command tree against the settings.
            var settings = CommandBinder.Bind(tree, leaf.Command.SettingsType, resolver);
            var interceptors =
                ((IEnumerable<ICommandInterceptor>?)resolver.Resolve(typeof(IEnumerable<ICommandInterceptor>))
                ?? Array.Empty<ICommandInterceptor>()).ToList();
#pragma warning disable CS0618 // Type or member is obsolete
            if (configuration.Settings.Interceptor != null)
            {
                interceptors.Add(configuration.Settings.Interceptor);
            }
#pragma warning restore CS0618 // Type or member is obsolete
            foreach (var interceptor in interceptors)
            {
                interceptor.Intercept(context, settings);
            }

            // Create and validate the command.
            var command = leaf.CreateCommand(resolver);
            var validationResult = command.Validate(context, settings);
            if (!validationResult.Successful)
            {
                throw CommandRuntimeException.ValidationFailed(validationResult);
            }

            // Execute the command.
            var result = await command.ExecuteAsync(context, settings, cancellationToken);
            foreach (var interceptor in interceptors)
            {
                interceptor.InterceptResult(context, settings, ref result);
            }

            return result;
        }
        catch (Exception ex) when (configuration.Settings is { ExceptionHandler: not null, PropagateExceptions: false })
        {
            return configuration.Settings.ExceptionHandler(ex, resolver);
        }
    }
}
