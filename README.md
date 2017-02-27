[![Build status](https://ci.appveyor.com/api/projects/status/j2a8skqibee6d7vt/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/servicestack-webhooks/branch/master)[![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.svg)](https://www.nuget.org/packages/ServiceStack.Webhooks)[![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.Azure.svg)](https://www.nuget.org/packages/ServiceStack.Webhooks.Azure) 

# ServiceStack.Webhooks
Add Webhooks to your ServiceStack services

# Overview

This project aims to make it very easy to expose your own Webhooks to your ServiceStack services, and to manage 3rd party subscriptions to those webhooks. 

The project has these aims:

1. To deliver this capability in a small set of nuget packages. For example: The `ServiceStack.Webhooks` package would deliver the `AppHost` plugin (`WebhookFeature`) and the ability to manage subscriptions (built-in services, operations etc.), and the components to raise custom events from within your service (i.e. `IWebhook.Publish()`). Then other packages like `ServiceStack.Webhooks.Azure` or `ServiceStack.Webhooks.WebHost` would deliver various technologies/architectures with components responsible for relaying/delivering events to registered subscribers over HTTP.
2. Make it simple to configure the Webhooks in your service (i.e. in your `AppHost` just add the `WebhookFeature` PlugIn, and configure it  with your chosen technology components). 
3. Make it each component extensible, allowing you to use your favorite data repositories (for subsciption management), and to integrate the pub/sub mechanics with your favorite technologies in your host architecture. (i.e. buses, queues, functions, etc)

For example: In your serivce, you may want to store the Webhook subscriptions in a MongoDB database, and have an Azure worker role relay the events to subscribers from a (reliable) cloud queue.
In another service, you may want to store subscriptions in Ormlite SQL database, and relay events to subscribers directly from within the same service on a background thread.

The choice should be yours.

Want to get involved? Want to add this capability to your services? just send us a message or pull-request!

# Getting Started

[**This project is still in its infancy, and is not yet operational end to end**]

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

By default, the `WebhookFeature` uses a `MemoryWebhookSubscriptionStore` to store all subscriptions, but this will need replacing in production systems with a store that more appropriate to more persistent storage, like an OrmLiteStore or RedisStore, or database of your choice. 

The plugin also installs a subscription management service in your service on these routes:
* POST /webhooks/subscriptions - creates a new subscription (for the current user)
* GET /webhooks/subscriptions - lists all subscriptions (for the current user)
* GET /webhooks/subscriptions/{Id} - gets the details of the subscription
* PUT /webhooks/subscriptions/{Id} - updates the subscription
* DELETE /webhooks/subscriptions/{Id} - deletes the subscription

## Customizing

There are various components of the webhook architecture that you can extend with your own pieces to suit your needs:

```
public override void Configure(Container container)
{
    // Register your own subscription store
    container.Register<IWebhookSubscriptionStore>(new MyDbSubscriptionStore());

    Plugins.Add(new WebhookFeature();
}
```
