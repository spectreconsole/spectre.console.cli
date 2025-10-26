namespace Spectre.Console.Cli;

internal sealed class ComponentRegistration
{
    private readonly Type _implementationType;

    public ComponentActivator Activator { get; }
    public IReadOnlyList<Type> RegistrationTypes { get; }

    public ComponentRegistration(Type type, ComponentActivator activator, IEnumerable<Type>? registrationTypes = null)
    {
        _implementationType = type ?? throw new ArgumentNullException(nameof(type));

        var registrations = new List<Type>(registrationTypes ?? []);
        if (registrations.Count == 0)
        {
            // Every registration needs at least one registration type.
            registrations.Add(type);
        }

        RegistrationTypes = registrations;
        Activator = activator ?? throw new ArgumentNullException(nameof(activator));
    }

    public ComponentRegistration CreateCopy()
    {
        return new ComponentRegistration(_implementationType, Activator.CreateCopy(), RegistrationTypes);
    }
}