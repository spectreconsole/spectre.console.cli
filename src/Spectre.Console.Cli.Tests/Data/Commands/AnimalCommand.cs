namespace Spectre.Console.Tests.Data;

public abstract class AnimalCommand<TSettings> : Command<TSettings>
    where TSettings : CommandSettings
{
}