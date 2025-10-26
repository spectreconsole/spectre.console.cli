namespace Spectre.Console.Cli;

internal sealed class BranchConfigurator : IBranchConfigurator
{
    private readonly ConfiguredCommand _command;

    public BranchConfigurator(ConfiguredCommand command)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public IBranchConfigurator WithAlias(string alias)
    {
        _command.Aliases.Add(alias);
        return this;
    }
}