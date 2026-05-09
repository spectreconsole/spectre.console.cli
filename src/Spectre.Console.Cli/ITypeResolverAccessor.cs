namespace Spectre.Console.Cli;

/// <summary>
/// Provides access to the current <see cref="ITypeResolver"/> during command execution.
/// </summary>
public interface ITypeResolverAccessor
{
    /// <summary>
    /// Gets the current resolver, if available.
    /// </summary>
    ITypeResolver? Resolver { get; }
}