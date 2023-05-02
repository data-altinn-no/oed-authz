namespace oed_authz.Settings;

public static class Constants
{
    public const string ConfigurationSectionSecrets = "Secrets";
    public const string ConfigurationSectionGeneralSettings = "GeneralSettings";

    public const string AuthenticationSchemeInternal = "Altinn";
    public const string AuthenticationSchemeExternal = "Maskinporten";

    public const string AuthorizationPolicyForPlatformAuthorization = "PlatformAuthorization";
    public const string AuthorizationPolicyForEvents = "PlatformEvents";
    public const string AuthorizationPolicyForExternals = "Externals";

    public const string TokenClaimTypeApp = "urn:altinn:app";
    public const string TokenClaimTypeScope = "scope";

    public const string AppPlatformAuthorization = "platform.authorization";

    public const string ScopeProbateOnly = "altinn:dd:authlookup:probateonly";
    public const string ScopeAllRoles = "altinn:dd:authlookup:allroles";
    public const string ProbateRoleCode = "urn:digitaltdodsbo:skifteattest";
}

