using System.Resources;

namespace Spectre.Console.Cli.Help;

/// <summary>
/// A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
internal class HelpProviderResources
{
    private readonly ResourceManager _resourceManager;
    private readonly CultureInfo? _resourceCulture;

    public HelpProviderResources(CultureInfo? culture)
    {
        _resourceCulture = culture;
        _resourceManager = new ResourceManager(
            "Spectre.Console.Cli.Resources.HelpProvider", typeof(HelpProvider).Assembly);
    }

    /// <summary>
    /// Gets the localized string for "ARGUMENTS".
    /// </summary>
    internal string Arguments
    {
        get
        {
            return _resourceManager.GetString("Arguments", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "COMMAND".
    /// </summary>
    internal string Command
    {
        get
        {
            return _resourceManager.GetString("Command", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "COMMANDS".
    /// </summary>
    internal string Commands
    {
        get
        {
            return _resourceManager.GetString("Commands", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "DEFAULT".
    /// </summary>
    internal string Default
    {
        get
        {
            return _resourceManager.GetString("Default", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "DESCRIPTION".
    /// </summary>
    internal string Description
    {
        get
        {
            return _resourceManager.GetString("Description", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "EXAMPLES".
    /// </summary>
    internal string Examples
    {
        get
        {
            return _resourceManager.GetString("Examples", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "OPTIONS".
    /// </summary>
    internal string Options
    {
        get
        {
            return _resourceManager.GetString("Options", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "Prints help information".
    /// </summary>
    internal string PrintHelpDescription
    {
        get
        {
            return _resourceManager.GetString("PrintHelpDescription", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "Prints version information".
    /// </summary>
    internal string PrintVersionDescription
    {
        get
        {
            return _resourceManager.GetString("PrintVersionDescription", _resourceCulture) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the localized string for "USAGE".
    /// </summary>
    internal string Usage
    {
        get
        {
            return _resourceManager.GetString("Usage", _resourceCulture) ?? string.Empty;
        }
    }
}