namespace Spectre.Console.Cli;

internal sealed class CommandOption : CommandParameter, ICommandOption
{
    public IReadOnlyList<string> LongNames { get; }
    public IReadOnlyList<string> ShortNames { get; }
    public string? ValueName { get; }
    public bool ValueIsOptional { get; }
    public bool IsShadowed { get; set; }
    public bool IsDeprecated => DeprecationMessage != null;
    public string? DeprecationMessage { get; }

    public CommandOption(
        Type parameterType, ParameterKind parameterKind, PropertyInfo property, string? description,
        TypeConverterAttribute? converter, PairDeconstructorAttribute? deconstructor,
        CommandOptionAttribute optionAttribute, ParameterValueProviderAttribute? valueProvider,
        IEnumerable<ParameterValidationAttribute> validators,
        DefaultValueAttribute? defaultValue, bool valueIsOptional,
        string? deprecationMessage)
        : base(parameterType, parameterKind, property, description, converter,
            defaultValue, deconstructor, valueProvider, validators,
            optionAttribute.IsRequired, optionAttribute.IsHidden)
    {
        LongNames = optionAttribute.LongNames;
        ShortNames = optionAttribute.ShortNames;
        ValueName = optionAttribute.ValueName;
        ValueIsOptional = valueIsOptional;
        DeprecationMessage = deprecationMessage;
    }

    public string GetOptionName()
    {
        return LongNames.Count > 0 ? LongNames[0] : ShortNames[0];
    }
}