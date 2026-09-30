namespace Spectre.Console.Cli;

internal sealed class TypeResolverAccessor : ITypeResolverAccessor
{
    public ITypeResolver? Resolver { get; set; }
}