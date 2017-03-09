namespace ServiceStack.Webhooks
{
    public static class WebHookExtensions
    {
        /// <summary>
        ///     Create the initial schema for Data Stores that require it
        /// </summary>
        /// <param name="store"></param>
        public static void InitSchema(this ISubscriptionStore store)
        {
            var requiresSchema = store as IRequiresSchema;
            if (requiresSchema != null)
            {
                requiresSchema.InitSchema();
            }
        }
    }
}