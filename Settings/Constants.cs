namespace oed_authz.Settings;

public static class Constants
{
    public const string ConfigurationSectionSecrets = "Secrets";
    public const string ConfigurationSectionGeneralSettings = "GeneralSettings";

    public const string MaskinportenAuthentication = "Maskinporten";
    public const string MaskinportenAuxillaryAuthentication = "MaskinportenAuxillary";

    public const string AuthorizationPolicyInternal = "Internal";
    public const string AuthorizationPolicyForEvents = "Events";
    public const string AuthorizationPolicyExternal = "Externals";

    public const string ScopeInternal = "altinn:dd:internal";
    public const string ScopeProbateOnly = "altinn:dd:authlookup:probateonly";
    public const string ScopeAllRoles = "altinn:dd:authlookup:allroles";

    public const string ProbateRoleCode = "urn:digitaltdodsbo:skifteattest";
}

