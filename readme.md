# JobTriggers

Provides ability to call methods using Timer Trigger or Queue Trigger (using azure queues)

## Setup

1) Download latest version from Nuget `Install-Package Lykke.JobTriggers`
2) Register dependencies (you can also override ILog and IPoisonQueueNotifier):
- Service collection example
```
  var serviceCollection = new ServiceCollection();
  
  // override ILog and IPosionQueueNotifier if you want
  serviceCollection.AddTransient<ILog, LogImplementation>();
  serviceCollection.AddTransient<IPoisionQueueNotifier, PoisionQueueNotifierImplementation>();
  
  // register only time triggers
  serviceCollection.AddTriggers();
  
  //register time and queue triggers
  serviceCollection.AddTriggers(pool =>
  {
      // default connection must be initialized
      pool.AddDefaultConnection(defaultConnectionString);
      
      // you can add additional connection strings and then specify it in QueueTriggerAttribute 
      pool.AddConnection("custom", additionalConnectionString);
  });
```
If you won't override ILog and IPoisionQueueNotifier then default implementation will be used.

- Autofac example
```
  var ioc = new ContainerBuilder();
  
  var serviceCollection = new ServiceCollection();
  
  // override ILog and IPosionQueueNotifier if you want
  serviceCollection.AddTransient<ILog, ILogImplementation>();
  serviceCollection.AddTransient<IPoisionQueueNotifier, IPoisionQueueNotifierImplementation>();
  
  // register only time triggers
  serviceCollection.AddTriggers();
  
  //register time and queue triggers
  serviceCollection.AddTriggers(pool =>
  {
      // default connection must be initialized
      pool.AddDefaultConnection(defaultConnectionString);
      
      // you can additional connection strings and then specify it in QueueTriggerAttribute 
      pool.AddConnection("custom", additionalConnectionString);
  });
  
  ioc.Populate(serviceCollection);
```

3) You should also register your classes, where Triggers are used.
4) Use TimeTrigger or QueueTrigger
```
 public class TestFunction
 {
  // queue name is required
  // optional parameters:
  // notify - when set to "true" message you will receive notification through IPoisionQueueNotifier when mesage will be sent to poison queue
  // connection - specify connection name, that was registered before (by default "default" connection will be used)
  // you can specify strongly typed model and queue message will be deserialized into this model 
  // or you can set in parameter as String, and will receive raw queue message
  [QueueTrigger("test-queue", notify: true, connection: "custom")]
  public async Task Process(CustomModel model)
  {
  }
  
  // you can also specify second parameters:
  
  // DateTimeOffset - queue message insertion time
  [QueueTrigger("test-queue")]
  public async Task Process(string rawModel, DateTimeOffset dt)
  {
  }
  
  // QueueTriggeringContext - message context
  [QueueTrigger("test-queue")]
  public async Task Process(string rawModel, QueueTriggeringContext context)
  {
  }
  
  // period in HH:mm:ss
  [TimerTrigger("00:00:30")]
  public async Task Process()
  {
  }
 }
```
5) Start trigger host
```
var triggerHost = new TriggerHost(serviceProvider);
await triggerHost.Start();
```
6) Stop trigger host (cancellation token is used)
```
triggerHost.Cancel();
```
