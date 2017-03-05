[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0) [![Build status](https://ci.appveyor.com/api/projects/status/j2a8skqibee6d7vt/branch/master?svg=true)](https://ci.appveyor.com/project/JezzSantos/servicestack-webhooks/branch/master) [![NuGet](https://img.shields.io/nuget/v/ServiceStack.Webhooks.svg?label=ServiceStack.Webhooks)](https://www.nuget.org/packages/ServiceStack.Webhooks)

# ServiceStack.Webhooks
Add Webhooks to your ServiceStack services

# Overview

This project makes it very easy to expose your own Webhooks to your ServiceStack services, and to manage your user's subscriptions to those webhooks.

By adding the `WebhookFeature` to the AppHost of your service, you automatically get all the pieces you need to raise and manage the events raised by your services. 

We _know_ that most services are built for scalability and to be hosted in the cloud, so we know that you are going to want to use your own components and technologies that fit in with your own architecture. Now you can, with the `WebhookFeature`.

For example: In one service, you may want to store the Webhook subscriptions in a _MongoDB database_, and have an _Azure worker role_ relay the events to subscribers from a (reliable) cloud _queue_.
In another service, you may want to store subscriptions in _Ormlite SQL database_, and relay events to subscribers directly from within the same service _on a background thread_, or throw the event to an _AWS lambda_ to process. Whatever works for you, the choice is yours.

_Oh, don't worry, we haven't left it all entirely up to you, just to get started with the `WebhookFeature`. We got your back with a built-in subscription store and built-in event sink that will get you going seeing how it all works. But eventually you'll want to swap those out for your own pieces that fit your architecture, which is easy._

![](https://raw.githubusercontent.com/jezzsantos/ServiceStack.Webhooks/master/docs/images/Webhooks.Architecture.PNG)

If you cant find the component you want for your architecture (see [Plugins](https://github.com/jezzsantos/ServiceStack.Webhooks/wiki/Plugins)), it should be easy for you to build add your own and _just plug it in_.

### Contribute!

Want to get involved in this project? or want to help improve this capability for your services? just send us a message or pull-request!

# [Getting Started](https://github.com/jezzsantos/ServiceStack.Webhooks/wiki/Getting-Started)

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

The URL to that endpoint is the  `Config.Url` of the created subscription.

In the case of the "hello" event above, the POSTed request would look something like this:

```
POST http://myserver/hello HTTP/1.1
Accept: application/json
User-Agent: ServiceStack .NET Client 4.56
Accept-Encoding: gzip,deflate
X-Webhook-Delivery: 7a6224aad9c8400fb0a70b8a71262400
X-Webhook-Event: hello
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

# [Documentation](https://github.com/jezzsantos/ServiceStack.Webhooks/wiki)

More documentation about how the `WebhookFeature` works, and how to customize it are available in [here](https://github.com/jezzsantos/ServiceStack.Webhooks/wiki)
