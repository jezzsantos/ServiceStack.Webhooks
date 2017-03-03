[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [![Build status](https://ci.appveyor.com/api/projects/status/j2a8skqibee6d7vt/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/servicestack-webhooks/branch/master) [![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.svg?label=ServiceStack.Webhooks)](https://www.nuget.org/packages/ServiceStack.Webhooks) [![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.Azure.svg?label=ServiceStack.Webhooks.Azure)](https://www.nuget.org/packages/ServiceStack.Webhooks.Azure) 

# ServiceStack.Webhooks
Add Webhooks to your ServiceStack services

# Overview

This project makes it very easy to expose your own Webhooks to your ServiceStack services, and to manage your user's subscriptions to those webhooks.

By adding the `WebhookFeature` to the AppHost of your service, you automatically get all the pieces you need to raise and manage the events raised by your services. 

We _know_ that most services are built for scalability and to be hosted in the cloud, so we know that you are going to want to use your own components and technologies that fit in with your own architecture. Now you can, with the `WebhookFeature`.

For example: In one service, you may want to store the Webhook subscriptions in a _MongoDB database_, and have an _Azure worker role_ relay the events to subscribers from a (reliable) cloud _queue_.
In another service, you may want to store subscriptions in _Ormlite SQL database_, and relay events to subscribers directly from within the same service _on a background thread_, or throw the event to an _AWS lambda_ to process.

The choice is entirely yours.

_Oh, don't worry, we haven't left it entirely up to you, just to get started. We got your back with a built-in subscription store and built-in event sink that will get you going seeing how it works. But eventually you'll want to swap those out for your own pieces, which is easy._

![](https://raw.githubusercontent.com/jezzsantos/ServiceStack.Webhooks/master/docs/images/Webhooks.Architecture.PNG)

If you cant find the component you want for your architecture, it should be easy for you to build add your own and _just plug it in_.

### Contribute!

Want to get involved in this project? or want to help improve this capability for your services? just send us a message or pull-request!

# Getting Started

Install from NuGet:
```
Install-Package ServiceStack.Webhooks
```

Simply add the `WebhookFeature` in your `AppHost.Configure()` method:

```
public override void Configure(Container container)
{
    // Add ValidationFeature and AuthFeature plugins first

    Plugins.Add(new WebhookFeature());
}
```

Note: You must add the `WebhookFeature` after you use either of these features:
* `Plugins.Add(new ValidationFeature();`
* `Plugins.Add(new AuthFeature());`

## Subscription Service

The `WebhookFeature` plugin automatically installs a built-in (and secure) subscription management API in your service on the following routes:

* POST /webhooks/subscriptions - creates a new subscription (for the current user)
* GET /webhooks/subscriptions - lists all subscriptions (for the current user)
* GET /webhooks/subscriptions/{Id} - gets the details of the subscription
* PUT /webhooks/subscriptions/{Id} - updates the subscription
* DELETE /webhooks/subscriptions/{Id} - deletes the subscription
* GET /webhooks/subscriptions/search - gets the subscribers for a specific event

This allows any users of your web service to create webhook registrations (subscribers to webhook events) for the events you raise in your service.

Note: Webhook subscriptions will be associated to the UserId (`ISession.UserId`) of the user using your service.

Note: This service uses role-based authorization to restrict who can call what, and you can customize those roles. (see later)

A subscriber creates a subscription by POSTing the following data:

```
POST /webhooks/subscriptions
{
    name: "aname",
    events: ["anevent1", "anevent2"],
    config: {
        url: "http://myserver/api/incoming",
        content-type: "application/json",  (optional)
        secret: "ASUPERSECRETKEY",  (optional)
    }
}
```

## Pluggable Components

By default, the following components are installed by the `WebhookFeature`:

* SubscriptionStore: `MemoryWebhookSubscriptionStore` to store all registered subscriptions
* Event Sink: `AppHostWebhookEventSink` to relay raised events directly from your service to subscribers.

**WARNING:** In production systems these default components will **need to be replaced**, by customizing your configuration of the `WebhookFeature`: 

* Configure a `IWebhookSubscriptionStore` with one that is more appropriate to more persistent storage, like an OrmLiteStore or RedisStore, or a stores subscriptions using a database of your choice. WARNING: If you don't do this, and you continue to use the built-in `MemoryWebhookSubscriptionStore` your subscriptions will be lost when your host/site is restarted.
* (Optional) Consider configuring a `IWebhookEventSink` with one that introduces some buffering between raising events and POSTing them to registered subscribers, like an Trigger, Queue, Bus-based implementation of your choice. WARNING: If you don't do this, and you continue to use the built-in `AppHostWebhookEventSink` your subscribers will be notified in the same thread that you raised the event, which can slow down your service significantly. 

## Raising Events

To raise events from your own services, add the `IWebhooks` dependency to your service, and call: `IWebhooks.Publish<TDto>(string eventName, TDto data)`. Simple as that.

```
internal class HelloService : Service
{
    public IWebhooks Webhooks { get; set; }

    public HelloResponse Any(Hello request)
    {
        Webhooks.Publish("hello", new HelloEvent{ Text = "Hello" });
    }
}
```

## Receiving Events

A subscriber that subscribes to your raised events  would need to provide a HTTP POST endpoint to receive the webhook event. 

In the case of the "hello" event above, the POSTed request would look something like this:

```
POST http://myserver/hello HTTP/1.1
Accept: application/json
User-Agent: ServiceStack .NET Client 4.56
Accept-Encoding: gzip,deflate
X-Webhook-Delivery: 7a6224aad9c8400fb0a70b8a71262400
X-Webhook-Event: aneventname
Content-Type: application/json
Host: myserver
Content-Length: 25
Expect: 100-continue
Proxy-Connection: Keep-Alive

{
    "Text":"Hello"
}
```

In this case the subscriber would need the following [ServiceStack] service operation to receive this particular event's payload:

```
internal class MyService : Service
{
    public void Post(Hello request)
    {
       // I got a webhook event!
       
       // The event name, and other messaging metadata are included in the headers
       var eventName = Request.Headers["X-Webhook-Event"];
       var deliveryId = Request.Headers["X-Webhook-Delivery"];
       var signature = Request.Headers["X-Hub-Signature"];
    }
}

[Route("/hello", "POST")]
public class Hello
{
    public string Text { get; set; }
}
```

## How It Works

When you register the `WebhookFeature` in your AppHost, it installs the subscriptions API, and the basic components to support raising events.

By default, the `AppHostWebhookEventSink` is used as the event sink.

When events are raised to it, the sink queries the `ISubscriptionsService.Search(eventName)` (in-proc) to fetch all the subscriptions to POST events to. It caches those subscriptions for a TTL (say 60s), to reduce the number of times the query for the same event is made (to avoid chatter as events are raised in your services). Then is dispatches the notification of that event to all registered subscribers (over HTTP). It will retry 3 times before giving up (`EventServiceClient.Post`).

![](https://raw.githubusercontent.com/jezzsantos/ServiceStack.Webhooks/master/docs/images/Webhooks.Default.PNG)

WARNING: The `AppHostWebhookEventSink` can work well in testing, but it is going to slow down your service request times, as it has to notify each of the subscribers, and that network latency is added to the call time of your API (since it is done in-proc and on the same thread as that of the web request that raised the event).

* We recommend only using the `AppHostWebhookEventSink` in testing and non-production systems.
* We recommend, configuring a `IWebhookEventSink` that scales better with your architecture, and decouples the raising of events from the notifying of subscribers.

## Customizing

There are various components of the webhook architecture that you can extend with your own pieces to suit your needs:

### Subscription Service

The subscription service is automatically built-in to your service when you add the `WebhookFeature`.
If you prefer to roll your own subscription service, you can turn off the built-in one like this:

```
public override void Configure(Container container)
{
   // Add other plugins first

    Plugins.Add(new WebhookFeature
    {
        IncludeSubscriptionService = false
    });
}
```

By default, the subscription service is secured by role-based access if you already have the `AuthFeature` plugin in your AppHost.
If you don't use the `AuthFeature` then the subscription service is not secured, and can be used by anyone using your API.

If you use the `AuthFeature`, rememmber to add the `WebhookFeature` plugin after you add the `AuthFeature` plugin.

When the subscription service is secured, by default, the following roles are protecting the following operations:
* POST /webhooks/subscriptions - "user"
* GET /webhooks/subscriptions - "user"
* GET /webhooks/subscriptions/{Id} - "user"
* PUT /webhooks/subscriptions/{Id} - "user"
* DELETE /webhooks/subscriptions/{Id} - "user"
* GET /webhooks/subscriptions/search - "service"

These roles are configurable by setting the following properties of the `WebhookFeature` when you register it:

```
public override void Configure(Container container)
{
    // Add the AuthFeature plugin first
    Plugins.Add(new AuthFeature(......);

    Plugins.Add(new WebhookFeature
    {
        SubscriptionAccessRoles = "accessrole1",
        SubscriptionSearchRoles = "searchrole1;searchrole2"
    });

}
```

Note: You can even set the `SubscriptionAccessRoles` or `SubscriptionSearchRoles` to null if you don't want to use role-based access to secure them.

### Subscription Store

Subscriptons for webhooks need to be stored (in `IWebhookSubscriptionStore`), once a user of your service subscribes to a webhook (using the API: `POST /webhooks/subscriptions`)

You specify your own store by registering it in the IOC container.
If you specify no store, the default `MemoryWebhookSubscriptionStore` will be used, which is fine for testing, but beware that your subscriptions will be lost whenever your service is restarted.

```
public override void Configure(Container container)
{
    // Register your own subscription store
    container.Register<IWebhookSubscriptionStore>(new MyDbSubscriptionStore());

    Plugins.Add(new WebhookFeature();
}
```

### Events Sink

When events are raised they are passed to the event sink (`IWebhookEventSink`), typically temporarily, until they are "relayed" to all registered subscribers for that event. 

Ideally, for scalability and performance, you probably would not want to do that on the same thread that riased the event, and probably not even by the same process.

You specify your own sink by registering it in the IOC container.
If you specify no sink, the default `AppHostWebhookEventSink` will be used, which is fine for testing, but beware that this sink works synchronously on the same thread that raises the event, and so it is not optimal for scale in production systems.

```
public override void Configure(Container container)
{
    // Register your own event store
    container.Register<IWebhookEventSink>(new MyDbEventSink());

    Plugins.Add(new WebhookFeature();
}
```

# Azure Webhook Extensions

If you deploy your web service to Microsoft Azure, you may want to use Azure storage Tables and Queues etc. to implement the various components of the webhooks.

For example, 'subscriptions' can be stored in Azure Table Storage, 'events' can be queued in a Azure Queue Storage, and then 'events' can be relayed by a WorkerRole to subscribers.

![](https://raw.githubusercontent.com/jezzsantos/ServiceStack.Webhooks/master/docs/images/Webhooks.Azure.PNG)

Install from NuGet:
```
Install-Package Servicestack.Webhooks.Azure
```

Add the `WebhookFeature` in your `AppHost.Configure()` method as usual, and register the Azure components:

```
public override void Configure(Container container)
{
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore());
    container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink());

    Plugins.Add(new WebhookFeature();
}
```

If you are hosting your web service in an Azure WebRole, you may want to configure the 'subscription store' and the 'event sink' from your cloud configuration, instead of using the defaults, or specifying them in code, then register the services like this:

```
public override void Configure(Container container)
{
    var appSettings = new CloudAppSettings();
    container.Register<IAppSettings>(appSettings);

    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore(appSettings));
    container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink(appSettings));

    Plugins.Add(new WebhookFeature();
}
```

Then, in the 'Cloud' project that you have created for your service, edit the properties of the role you have for your web service.

Go to the 'Settings' tab and add the following settings:

* (ConnectionString) AzureTableWebhookSubscriptionStore.ConnectionString - The Azure Storage account connection string for your table. For example: UseDevelopmentStorage=true
* (string) AzureTableWebhookSubscriptionStore.Table.Name - The name of the table where subscriptions will be stored. For example: webhooksubscriptions
* (ConnectionString) AzureQueueWebhookEventSink.ConnectionString - The Azure Storage account connection string for your queue. For example: UseDevelopmentStorage=true
* (string) AzureQueueWebhookEventSink.Queue.Name - The name of the queue where events will be written. For example: webhookevents

### Configuring an Azure WorkerRole Relay

Now you can deploy an Azure WorkerRole that can query the events 'queue' and relay those events to all subscribers.
Since you are deploying this component to Azure, the configuration for it will exist in your Azure configuration files: `ServiceConfiguration.cscfg`

Create a new 'Azure Cloud Service' project in your solution, and add a 'WorkerRole' to it. (in this example we will name it "WebhookEventRelay")

In the new 'WebhookEventRelay' project, install the nuget package:

```
Install-Package Servicestack.Webhooks.Azure
```

In the 'WorkerRole.cs' file that was created for you, replace the 'WorkerRole' class with this code:

```
public class WorkerRole : AzureWorkerRoleEntryPoint
    {
        private List<WorkerEntryPoint> workers;

        protected override IEnumerable<WorkerEntryPoint> Workers
        {
            get { return workers; }
        }

        public override void Configure(Container container)
        {
            base.Configure(container);

            container.Register<IAppSettings>(new CloudAppSettings());
            container.Register(new EventRelayWorker(container));

            workers = new List<WorkerEntryPoint>
            {
                container.Resolve<EventRelayWorker>(),
                // (Add other types if you want to use this WorkerRole for multiple workloads)
            };
        }
    }
```

In the 'Cloud' project that you created, edit the properties of the 'WebhookEventRelay' role.

Go to the 'Settings' tab and add the following settings:

* (string) SubscriptionServiceClient.SubscriptionService.BaseUrl - The base URL of your webhook subscription service (where the `WebhookFeature` is installed ). For example: http://myserver:80/api
* (string) EventRelayQueueProcessor.Polling.Interval.Seconds - The interval (in seconds) that the worker role polls the Azure queue for new events. For example: 5
* (ConnectionString) EventRelayQueueProcessor.ConnectionString - The Azure Storage account connection string. For example: UseDevelopmentStorage=true
* (string) EventRelayQueueProcessor.TargetQueue.Name - The name of the queue where events will be polled. For example: webhookevents
* (string) EventRelayQueueProcessor.UnhandledQueue.Name - The name of the queue where failed events are dropped. For example: unhandledwebhookevents
* (string) EventRelayQueueProcessor.ServiceClient.Retries - The number of retry attempts the relay will make to notify a subscriber before giving up. For example: 3
* (string) EventRelayQueueProcessor.ServiceClient.Timeout.Seconds - The timeout (in seconds) the relay will wait for the subscriber endpoint before cancelling the notification. For example: 60

Note: the value of the setting `EventRelayQueueProcessor.TargetQueue.Name` must be the same as the `AzureQueueWebhookEventSink.QueueName` that you may have configured in the `WebhookFeature`.

### Configuring Azure Storage Credentials

By default, these services will connect to the local Azure Emulator (UseDevelopmentStorage=true) which might be fine for testing your service, but after you have deployed to your cloud, you will want to provide different storage connection strings.

If you use the overload constructors, and pass in the `IAppSettings`, like this, you can load settings from your Azure cloud configuration:

```
public override void Configure(Container container)
{
    var appSettings = new CloudAppSettings();
    container.Register<IAppSettings>(appSettings);

    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore(appSettings));
    container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink(appSettings));

    Plugins.Add(new WebhookFeature();
}
```
then from your current ServiceConfiguration.<Configuration>.cscfg file:

* `AzureTableWebhookSubscriptionStore` will try to use a setting called: 'AzureTableWebhookSubscriptionStore.ConnectionString' for its storage connection
* `AzureQueueWebhookEventSink` will try to use a setting called: 'AzureQueueWebhookEventSink.ConnectionString' for its storage connection

Otherwise, you can set those values directly in code when you register the services:

```
public override void Configure(Container container)
{
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore
        {
            ConnectionString = "connectionstring",
        });
    container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink
        {
            ConnectionString = "connectionstring",
        });

    Plugins.Add(new WebhookFeature();
}
```

### Configuring Azure Storage Resources

By default, 

* `AzureTableWebhookSubscriptionStore` will create and use a storage table named: 'webhooksubscriptions'
* `AzureQueueWebhookEventSink` will create and use a storage queue named: 'webhookevents'

You can change those values when you register the services.

```
public override void Configure(Container container)
{
    container.Register<IWebhookSubscriptionStore>(new AzureTableWebhookSubscriptionStore
        {
            TableName = "mytablename",
        });
    container.Register<IWebhookEventSink>(new AzureQueueWebhookEventSink
        {
            QueueName = "myqueuename",
        });

    Plugins.Add(new WebhookFeature();
}
```



