namespace GZipArchiver.Lib.Enums
{
    /// <summary>
    /// The state of <see cref="Operations.OperationResult" />.
    /// </summary>
    public enum ProcessState
    {
        /// <summary>
        /// If the process is waiting
        /// </summary>
        Waiting = 0,

        /// <summary>
        /// If a user cancelled the process
        /// </summary>
        Cancelled = 1,

        /// <summary>
        /// If the process successfully finished
        /// </summary>
        Finished = 2,

        /// <summary>
        /// If an exception occured
        /// </summary>
        Error = 3
    }
}
