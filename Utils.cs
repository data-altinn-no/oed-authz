using oed_authz.Models;

namespace oed_authz;
public static class Utils
{
    public static bool IsValidSsn(string estateSsnOnly)
    {
        if (estateSsnOnly.Length != 11) return false;

        foreach (var t in estateSsnOnly)
        {
            if (t is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    public static string GetEstateSsnFromCloudEvent(CloudEvent daEvent)
    {
        if (daEvent.Subject == null)
        {
            throw new ArgumentNullException(nameof(daEvent.Subject));
        }

        var subject = daEvent.Subject.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (subject.Length != 2 || subject[0] != "person" || !IsValidSsn(subject[1]))
        {
            throw new ArgumentException(nameof(daEvent.Subject) + " must be SSN with '/person/' prefix");
        }

        return subject[1];
    }
}
