namespace Reusable.OmniLog.SemLog
{
    public enum Result
    {
        /// <summary>
        ///  Event not run. E.g. invalid parameters. Warning.
        /// </summary>
        Undefined,

        /// <summary>
        /// Event run and did its job. Information
        /// </summary>
        Success, 

        /// <summary>
        /// Event not run because conditions not met. No errors. Information.
        /// </summary>
        Completed,

        /// <summary>
        /// Event failed because of an error. Error.
        /// </summary>
        Failure, 
    }
}