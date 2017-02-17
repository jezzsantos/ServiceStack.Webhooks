# ServiceStack.Webhooks
Add Webhooks to your ServiceStack services

This project aims to make it very easy to expose your own Webhooks to your ServiceStack services, and to manage 3rd party subscriptions to those webhooks. 

The project has these aims:

1. To deliver this capability in a small set of nuget packages. For example: The `ServiceStack.Webhooks` package could deliver the `AppHost` plugin (`WebhookFeature`) and the ability to manage subscriptions (built-in services, operations etc.), and the components to raise custom events from your service (i.e. `IWebhook.Publish()`). Then other packages like `ServiceStack.Webhooks.Azure` or `ServiceStack.Webhooks.WebHost` could be created for various technologies/architectures that deliver components responsible for delivering events to registered subscribers.
2. Make it simple to configure the Webhooks in your service (i.e. in your `AppHost` just configure the `WebhookFeature` with your chosen technology components). 
3. Make it extensible allowing you to use your favorite data repositories (for subsciption management), and to integrate the pub/sub mechanics with your favorite components in your host architecture. (i.e. queues, functions, etc)

For example: In your serivce, you may want to store the Webhook subscriptions in a MongoDB database, and have an Azure worker role send the events to subscribers from a reliable cloud queue.
In another service, you may want to store subscriptions in Ormlite SQL database, and send events to subscribers on a background thread in your web service.
The choice should be yours.

Want to get involved?, want to add this capability to your services?, just send us a pull-request!
