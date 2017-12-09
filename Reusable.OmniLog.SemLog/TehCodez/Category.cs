namespace Reusable.OmniLog.SemanticExtensions
{
    public interface ISnapshotCategory { }

    public interface IActionCategory { }

    public abstract class Category
    {
        public static ISnapshotCategory Snapshot => default;

        public static IActionCategory Action => default;
    }
}