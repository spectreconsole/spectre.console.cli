namespace Spectre.Console.Tests.Data;

public class RequiredOptionsSettings : CommandSettings
{
    [CommandOption("--foo <VALUE>", true)]
    [Description("Foos the bars")]
    public string Foo { get; set; } = null!;
}

public class RequiredOptionsWithoutDescriptionSettings : CommandSettings
{
    [CommandOption("--foo <VALUE>", true)]
    public string Foo { get; set; } = null!;
}