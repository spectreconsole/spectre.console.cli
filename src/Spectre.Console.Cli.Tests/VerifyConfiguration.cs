namespace Spectre.Console.Tests;

public static class VerifyConfiguration
{
    [ModuleInitializer]
    public static void Init()
    {
        // Set the culture to invariant to ensure consistent test results regardless of the environment. (e.g., german host makes spectre.console.cli.tests fail because of i18n help output)
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

        Verifier.DerivePathInfo(Expectations.Initialize);
    }
}