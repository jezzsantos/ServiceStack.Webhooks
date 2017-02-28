namespace ServiceStack.Webhooks.Azure.Table
{
    /// <summary>
    ///     Comparison operators for queries in table storage
    /// </summary>
    public enum QueryOperator
    {
        /// <summary>
        ///     Equals
        /// </summary>
        // ReSharper disable once InconsistentNaming
        EQ,

        /// <summary>
        ///     Greater than
        /// </summary>
        // ReSharper disable once InconsistentNaming
        GT,

        /// <summary>
        ///     Greater than or equal to
        /// </summary>
        // ReSharper disable once InconsistentNaming
        GE,

        /// <summary>
        ///     Less than
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LT,

        /// <summary>
        ///     Less than or equal to
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LE,

        /// <summary>
        ///     Not equal
        /// </summary>
        // ReSharper disable once InconsistentNaming
        NE
    }
}