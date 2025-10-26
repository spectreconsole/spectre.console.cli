namespace Spectre.Console.Cli;

internal sealed class CommandConfigurator : ICommandConfigurator
{
    private readonly ConfiguredCommand _command;

    public CommandConfigurator(ConfiguredCommand command)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public ICommandConfigurator WithExample(params string[] args)
    {
        _command.Examples.Add(args);
        return this;
    }

    public ICommandConfigurator WithAlias(string alias)
    {
        _command.Aliases.Add(alias);
        return this;
    }

    public ICommandConfigurator WithDescription(string description)
    {
        _command.Description = description;
        return this;
    }

    public ICommandConfigurator WithData(object data)
    {
        _command.Data = data;
        return this;
    }

    public ICommandConfigurator IsHidden()
    {
        _command.IsHidden = true;
        return this;
    }
}