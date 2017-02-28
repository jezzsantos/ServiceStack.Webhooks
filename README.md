[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [![Build status](https://ci.appveyor.com/api/projects/status/j2a8skqibee6d7vt/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/servicestack-webhooks/branch/master) [![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.svg?label=ServiceStack.Webhooks)](https://www.nuget.org/packages/ServiceStack.Webhooks) [![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.Azure.svg?label=ServiceStack.Webhooks.Azure)](https://www.nuget.org/packages/ServiceStack.Webhooks.Azure) 

# ServiceStack.Webhooks
Add Webhooks to your ServiceStack services

# Overview

This project aims to make it very easy to expose your own Webhooks to your ServiceStack services, and to manage 3rd party subscriptions to those webhooks. 

The project has these aims:

1. To deliver this capability in a small set of nuget packages. For example: The `ServiceStack.Webhooks` package would deliver the `AppHost` plugin (`WebhookFeature`) and the ability to manage subscriptions (built-in services, operations etc.), and the components to raise custom events from within your service (i.e. `IWebhooks.Publish(string eventName, TDto data)`). Then other packages like `ServiceStack.Webhooks.Azure` or `ServiceStack.Webhooks.WebHost` would deliver various distributed technologiess with components responsible for relaying/delivering events to registered subscribers over HTTP.
2. Make it simple to configure the Webhooks in your service (i.e. in your `AppHost` just add the `WebhookFeature` plugin, and configure it  with your chosen technology components).
3. Make it each component extensible, allowing you to use your favorite data repositories (for subsciption management), and to integrate the pub/sub mechanics with your favorite technologies in your host architecture. (i.e. buses, queues, functions, etc)

For example: In one service, you may want to store the Webhook subscriptions in a MongoDB database, and have an Azure worker role relay the events to subscribers from a (reliable) cloud queue.
In another service, you may want to store subscriptions in Ormlite SQL database, and relay events to subscribers directly from within the same service on a background thread.

The choice should be yours. If you cant find the component you want for your architecture, it should be easy to add your own and plug it in.

# Contribute!

Want to get involved? Want to help add this capability to your services? just send us a message or pull-request!

# Getting Started

[**This project is still in its infancy and is not yet operational end to end (any version less than 1.0.0)**]

Install from NuGet:
```
Install-Package Servicestack.Webhooks
```

Simply add the `WebhookFeature` in your `AppHost.Configure()` method:

```
public override void Configure(Container container)
{
    Plugins.Add(new WebhookFeature();
}
```

By default, the `WebhookFeature` uses a `MemoryWebhookSubscriptionStore` to store all subscriptions and a `MemoryWebhookEventStore` to store raised events from your service, but these will need replacing in production systems with stores that are more appropriate to more persistent storage, like an OrmLiteStore or RedisStore, or a stores using a database of your choice. 

The plugin automatically installs a subscription management service in your service on these routes:
* POST /webhooks/subscriptions - creates a new subscription (for the current user)
* GET /webhooks/subscriptions - lists all subscriptions (for the current user)
* GET /webhooks/subscriptions/{Id} - gets the details of the subscription
* PUT /webhooks/subscriptions/{Id} - updates the subscription
* DELETE /webhooks/subscriptions/{Id} - deletes the subscription

This allows any users of your web service to create web hook registrations (subscribers to webhook events). Webhooks are associated with the ID of the user using this service.

## Customizing

There are various components of the webhook architecture that you can extend with your own pieces to suit your needs:

### Subscription Store

Subscriptons for webhooks need to be stored (`IWebhookSubscriptionStore`), once a user of your service registers a webhook (using the registration API: POST /webhooks/subscriptions)

You specify your store by registering it in the container.
If you specify no store, the default `MemoryWebhookSubscriptionStore` will be used, but beware that your subscriptions will be lost whenever your service is restarted.

```
public override void Configure(Container container)
{
    // Register your own subscription store
    container.Register<IWebhookSubscriptionStore>(new MyDbSubscriptionStore());

    Plugins.Add(new WebhookFeature();
}
```

### Events Store

When events are raised they are stored (temporarily, until relayed to all registered subscribers) in an event store (`IwebhookEventStore`).

You specify your store by registering it in the container.
If you specify no store, the default `MemoryWebhookEventStore` will be used, but beware that your events will be lost whenever your service is restarted.

```
public override void Configure(Container container)
{
    // Register your own event store
    container.Register<IWebhookEventStore>(new MyDbEventStore());

    Plugins.Add(new WebhookFeature();
}
```

## Azure Extensions

If you deploy your web service to Microsoft Azure, you can use Azure storage Tables and Queues to implement the various components of the webhooks.

Subscriptions can be stored in Azure table storage, and events can be queued and relayed from an Azure queue.

Install from NuGet:
```
Install-Package Servicestack.Webhooks.Azure
```

Add the `WebhookFeature` in your `AppHost.Configure()` method, and register the Azure components:

```
public override void Configure(Container container)
{
    container.Register<IWebhookEventStore>(new AzureQueueWebhookEventStore());
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore());

    Plugins.Add(new WebhookFeature();
}
```

By default, these services will connect to the local Azure Emulator (UseDevelopmentStorage=true) which might be fine for testing you service, but after you have deployed to your cloud, you will want to provide different storage connection strings.

### Configuring Azure Storage Credentials

If you use the overload constructors, and pass in the `IAppSettings`, like this:

```
public override void Configure(Container container)
{
    container.Register<IWebhookEventStore>(new AzureQueueWebhookEventStore(appSettings));
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore(appSettings));

    Plugins.Add(new WebhookFeature();
}
```
then:

* `AzureTableWebhookSubscriptionStore` will try to find a setting called: 'AzureTableSubscriptionStore.AzureConnectionString'
* `AzureQueueWebhookEventStore` will try to find a setting called: 'AzureQueueEventStore.AzureConnectionString'

Otherwise you can set those values directly in code when you register the services:

```
public override void Configure(Container container)
{
    container.Register<IWebhookEventStore>(new AzureQueueWebhookEventStore
        {
            AzureConnectionString = "connectionstring",
        });
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore
        {
            AzureConnectionString = "connectionstring",
        });

    Plugins.Add(new WebhookFeature();
}
```

### Configuring Azure Resources

By default, 

* `AzureTableWebhookSubscriptionStore` will create and use a table named: 'webhooksubscriptions'
* `AzureQueueWebhookEventStore` will create and use a queue named: 'webhookevents'

You can change those values when you register the services.

```
public override void Configure(Container container)
{
    container.Register<IWebhookEventStore>(new AzureQueueWebhookEventStore
        {
            QueueName = "myqueuename",
        });
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore
        {
            TableName = "mytablename",
        });

    Plugins.Add(new WebhookFeature();
}
```



