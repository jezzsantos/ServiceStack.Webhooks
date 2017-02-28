using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServiceStack.Webhooks.Azure.Table
{
    internal class AzureTableStorage : IAzureTableStorage
    {
        private const int CreationTimeoutSecs = 60;
        private readonly string connectionString;
        private readonly object mutex = new object();
        private readonly string tableName;
        private DateTime lastCreationCheck = DateTime.MinValue;

        public AzureTableStorage(string connectionString, string tableName)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNullOrEmpty(() => tableName, tableName);

            this.tableName = tableName;
            this.connectionString = connectionString;
        }

        protected CloudTable Table { get; private set; }

        public void Add(WebhookSubscriptionEntity subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            EnsureTable();

            Table.Execute(TableOperation.Insert(subscription));
            ResetLastCreationCheckTime();
        }

        public WebhookSubscriptionEntity Get(string id)
        {
            Guard.AgainstNullOrEmpty(() => id, id);

            var entity = Table.Execute(TableOperation.Retrieve<WebhookSubscriptionEntity>(string.Empty, id));
            ResetLastCreationCheckTime();

            return (WebhookSubscriptionEntity) entity.Result;
        }

        public void Update(WebhookSubscriptionEntity subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            EnsureTable();

            Table.Execute(TableOperation.InsertOrReplace(subscription));
            ResetLastCreationCheckTime();
        }

        public void Delete(WebhookSubscriptionEntity subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            EnsureTable();

            Table.Execute(TableOperation.Delete(subscription));
            ResetLastCreationCheckTime();
        }

        public List<WebhookSubscriptionEntity> Find(TableStorageQuery query)
        {
            Guard.AgainstNull(() => query, query);

            EnsureTable();

            var queried = Table.ExecuteQuery(new TableQuery<WebhookSubscriptionEntity>()
                .Where(query.Query));
            ResetLastCreationCheckTime();

            return queried.ToList();
        }

        public void Empty()
        {
            EnsureTable();

            var allEntities = Table.ExecuteQuery(new TableQuery());
            ResetLastCreationCheckTime();

            var batches = new Dictionary<string, TableBatchOperation>();
            foreach (var entity in allEntities)
            {
                TableBatchOperation batch;

                if (batches.TryGetValue(entity.RowKey, out batch) == false)
                {
                    batch = batches[entity.RowKey] = new TableBatchOperation();
                }

                batch.Add(TableOperation.Delete(entity));

                if (batch.Count == 100)
                {
                    Table.ExecuteBatch(batch);
                    batches[entity.RowKey] = new TableBatchOperation();
                }
            }

            foreach (var batch in batches.Values)
                if (batch.Count > 0)
                {
                    Table.ExecuteBatch(batch);
                }

            ResetLastCreationCheckTime();
        }

        private void EnsureTable()
        {
            if (Table == null)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var client = storageAccount.CreateCloudTableClient();
                Table = client.GetTableReference(tableName);
                InitLastCreationCheckTime();
            }

            if (RequiresCreationCheck())
            {
                Table.CreateIfNotExists();
            }
        }

        private void InitLastCreationCheckTime()
        {
            lock (mutex)
            {
                lastCreationCheck = DateTime.MinValue;
            }
        }

        private bool RequiresCreationCheck()
        {
            TimeSpan sinceLast;
            lock (mutex)
            {
                sinceLast = DateTime.UtcNow.Subtract(lastCreationCheck);
            }

            return sinceLast >= TimeSpan.FromSeconds(CreationTimeoutSecs);
        }

        private void ResetLastCreationCheckTime()
        {
            lock (mutex)
            {
                lastCreationCheck = DateTime.UtcNow;
            }
        }
    }
}