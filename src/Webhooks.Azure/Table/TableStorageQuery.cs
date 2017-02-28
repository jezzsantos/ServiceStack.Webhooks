using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServiceStack.Webhooks.Azure.Table
{
    internal class TableStorageQuery
    {
        public TableStorageQuery(string propertyName, QueryOperator operation, object value)
        {
            Parts = new List<QueryPart>
            {
                new QueryPart(propertyName, operation, value)
            };
        }

        public TableStorageQuery(IEnumerable<QueryPart> parts)
        {
            Guard.AgainstNull(() => parts, parts);

            Parts = new List<QueryPart>(parts);
        }

        public List<QueryPart> Parts { get; private set; }

        public string Query
        {
            get { return ToQuery(Parts); }
        }

        private static string ToQuery(List<QueryPart> parts)
        {
            if (!parts.Any())
            {
                return null;
            }

            if (parts.Count == 1)
            {
                return parts.First().ToQuery();
            }

            var totalQuery = string.Empty;
            parts.ForEach(part =>
            {
                var query = part.ToQuery();

                totalQuery = totalQuery.HasValue() ? TableQuery.CombineFilters(totalQuery, "and", query) : query;
            });

            return totalQuery;
        }
    }

    internal class QueryPart
    {
        public QueryPart(string propertyName, QueryOperator operation, object value)
        {
            Guard.AgainstNullOrEmpty(() => propertyName, propertyName);

            PropertyName = propertyName;
            Operation = operation;
            Value = value;
        }

        public string PropertyName { get; set; }

        public QueryOperator Operation { get; set; }

        public object Value { get; set; }

        public string ToQuery()
        {
            if (Value is DateTime)
            {
                var givenDate = (DateTime) Value;
                if (!givenDate.HasValue())
                {
                    givenDate = DateTimeExtensions.MinAzureDateTime;
                }

                return TableQuery.GenerateFilterConditionForDate(PropertyName, Operation.ToString().ToLowerInvariant(),
                    new DateTimeOffset(givenDate));
            }

            var givenString = Value != null ? Value.ToString() : null;
            return TableQuery.GenerateFilterCondition(PropertyName, Operation.ToString().ToLowerInvariant(), givenString);
        }
    }
}