namespace Reusable;

public static class SemanticVersionExtensions
{
    public static SemanticVersion IncrementMajor(this SemanticVersion version, int major)
    {
        return new SemanticVersion
        (
            version.Major + major,
            version.Minor,
            version.Patch,
            version.Labels
        );
    }

    public static SemanticVersion IncrementMinor(this SemanticVersion version, int minor)
    {
        return new SemanticVersion
        (
            version.Major,
            version.Minor + minor,
            version.Patch,
            version.Labels
        );
    }

    public static SemanticVersion IncrementPatch(this SemanticVersion version, int patch)
    {
        return new SemanticVersion
        (
            version.Major,
            version.Minor,
            version.Patch + patch,
            version.Labels
        );
    }
}