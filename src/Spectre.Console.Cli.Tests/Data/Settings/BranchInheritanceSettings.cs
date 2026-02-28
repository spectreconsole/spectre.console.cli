namespace Spectre.Console.Tests.Data;

public class BranchInheritanceSettings : CommandSettings
{
    [CommandOption("--my-value <VALUE>")]
    public string? MyValue { get; set; }
}

public sealed class BranchInheritedCommandSettings : BranchInheritanceSettings
{
}
