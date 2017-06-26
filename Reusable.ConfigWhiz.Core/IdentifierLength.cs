namespace Reusable.ConfigWhiz
{
    public enum IdentifierLength
    {
        None = 0,

        /// <summary>
        /// Setting
        /// </summary>
        Short = 1,

        /// <summary>
        /// Container.Setting
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Consumer.Container.Setting
        /// </summary>
        Long = 3,

        /// <summary>
        /// Namespace.Consumer.Container.Setting
        /// </summary>
        Unique = 4
    }
}