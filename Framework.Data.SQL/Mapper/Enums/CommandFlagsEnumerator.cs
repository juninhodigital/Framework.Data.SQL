using System;

namespace Framework.Data.SQL
{
    /// <summary>
    /// Additional state flags that control command behavior
    /// </summary>
    [Flags]
    public enum CommandFlagsEnumerator
    {
        /// <summary>
        /// No additional flags
        /// </summary>
        None = 0,
        /// <summary>
        /// Should data be buffered before returning?
        /// </summary>
        Buffered = 1,
        /// <summary>
        /// Can async queries be pipelined?
        /// </summary>
        Pipelined = 2,
        /// <summary>
        /// Should the plan cache be bypassed?
        /// </summary>
        NoCache = 4,
    }
}
