using System;

namespace ServiceStack.Webhooks
{
    public static class DataFormats
    {
        public static readonly DataFormat EntityIdentifier =
            new DataFormat(
                @"^[\{]{0,1}[A-Fa-f0-9]{8}[\-]{0,1}[A-Fa-f0-9]{4}[\-]{0,1}[A-Fa-f0-9]{4}[\-]{0,1}[A-Fa-f0-9]{4}[\-]{0,1}[A-Fa-f0-9]{12}[\}]{0,1}$",
                32, 38);

        public static DataFormat DescriptiveName(int min = 1, int max = 100)
        {
            return
                new DataFormat(@"^[\d\w\'\`\#\(\)\-\'\,\.\/ ]{{{0},{1}}}$".Fmt(min,
                    max), min, max);
        }

        public static DataFormat FreeformText(int min = 1, int max = 1000)
        {
            return
                new DataFormat(
                    @"^[\d\w\`\~\!\@\#\$\%\:\&\*\(\)\-\+\=\:\;\'\""\<\,\>\.\?\/ ]{{{0},{1}}}$".Fmt(min,
                        max), min, max);
        }

        public static DataFormat Base64Text()
        {
            return
                new DataFormat(@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$", 0, 0);
        }

        public static string CreateEntityIdentifier()
        {
            return Guid.NewGuid().ToString("D");
        }

        public static class Subscriptions
        {
            public static DataFormat Name = FreeformText(4, 100);
            public static DataFormat Event = new DataFormat(@"^[\d\w\-_]{4,100}$", 4, 100);
            public static DataFormat Secret = Base64Text();
        }
    }

    public class DataFormat
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DataFormat" /> class.
        /// </summary>
        public DataFormat(string expression, int minLength = 0, int maxLength = 0)
        {
            Expression = expression;
            MinLength = minLength;
            MaxLength = maxLength;
        }

        /// <summary>
        ///     Gets the regular expression
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        ///     Gets the maximum string length
        /// </summary>
        public int MaxLength { get; private set; }

        /// <summary>
        ///     Gets the minimum string length
        /// </summary>
        public int MinLength { get; private set; }
    }
}