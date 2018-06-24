using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.JobTriggers.Abstractions;
using Lykke.JobTriggers.Abstractions.QueueReader;
using Lykke.JobTriggers.Triggers.Attributes;
using Lykke.JobTriggers.Triggers.Delay;

namespace Lykke.JobTriggers.Triggers.Bindings
{
    [TriggerDefine(typeof(QueueTriggerAttribute))]
    [PublicAPI]
    [UsedImplicitly]
    public class QueueTriggerBinding : BaseTriggerBinding
    {
        private readonly TimeSpan _minDelay = TimeSpan.FromMilliseconds(100);
        private readonly TimeSpan _maxDelay = TimeSpan.FromMinutes(1);
        private const string PoisonSuffix = "-poison";

        private readonly ILog _log;
        private readonly IQueueReaderFactory _queueReaderFactory;
        private readonly IPoisionQueueNotifier _notifier;

        private int _maxDequeueCount;
        private IDelayStrategy _delayStrategy;
        private MethodInfo _method;
        private IServiceProvider _serviceProvider;
        private string _queueName;
        private IQueueReader _queueReader;
        private IQueueReader _poisonQueueReader;
        private Type _parameterType;
        private bool _hasSecondParameter;
        private bool _useTriggeringContext;
        private bool _shouldNotify;
        private string _connection;

        [Obsolete]
        public QueueTriggerBinding(ILog log, IQueueReaderFactory queueReaderFactory, IPoisionQueueNotifier notifier)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _queueReaderFactory = queueReaderFactory ?? throw new ArgumentNullException(nameof(queueReaderFactory));
            _notifier = notifier;
        }

        public QueueTriggerBinding(ILogFactory logFactory, IQueueReaderFactory queueReaderFactory, IPoisionQueueNotifier notifier)
        {
            if (logFactory == null)
            {
                throw new ArgumentNullException(nameof(logFactory));
            }
            
            _log = logFactory.CreateLog(this);
            _queueReaderFactory = queueReaderFactory ?? throw new ArgumentNullException(nameof(queueReaderFactory));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        public override void InitBinding(IServiceProvider serviceProvider, MethodInfo callbackMethod)
        {
            _serviceProvider = serviceProvider;
            _method = callbackMethod;

            var metadata = _method.GetCustomAttribute<QueueTriggerAttribute>();

            _connection = metadata.Connection;
            _queueName = metadata.Queue;
            _queueReader = _queueReaderFactory.Create(_connection, _queueName, TimeSpan.FromSeconds(metadata.TimeoutInSeconds));
            _shouldNotify = metadata.Notify;

            var parameters = _method.GetParameters();
            if (parameters.Length > 2 && parameters.Length < 1)
                throw new Exception($"Method {_method.Name} must have 1 or 2 parameters");
            if (parameters.Length == 2 && parameters[1].ParameterType != typeof(DateTimeOffset) && parameters[1].ParameterType != typeof(QueueTriggeringContext))
                throw new Exception($"Method {_method.Name} second parameter type is {parameters[1].ParameterType.Name}, but should be DateTimeOffset or QueueTriggeringContext");

            _parameterType = parameters[0].ParameterType;
            _hasSecondParameter = parameters.Length == 2;
            if (_hasSecondParameter)
                _useTriggeringContext = parameters[1].ParameterType == typeof(QueueTriggeringContext);
            _delayStrategy = new RandomizedExponentialStrategy(_minDelay,
                metadata.MaxPollingIntervalMs > 0 ? TimeSpan.FromMilliseconds(metadata.MaxPollingIntervalMs) : _maxDelay);
            _maxDequeueCount = metadata.MaxDequeueCount;
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                Exception globalEx = null;
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        IQueueMessage message = null;
                        bool executionSucceeded = false;
                        try
                        {
                            do
                            {
                                message = await _queueReader.GetMessageAsync();
                                if (message == null)
                                    break;

                                var context = new QueueTriggeringContext(message.InsertionTime);

                                var p = new List<object>() {message.Value(_parameterType)};

                                if (_hasSecondParameter)
                                    p.Add(_useTriggeringContext ? context : (object) message.InsertionTime);

                                var telemtryOperation = ApplicationInsightsTelemetry.StartRequestOperation($"{nameof(QueueTriggerBinding)} from {_queueName}");
                                try
                                {
                                    await Invoke(_serviceProvider, _method, p.ToArray());
                                }
                                catch (Exception ex)
                                {
                                    ApplicationInsightsTelemetry.MarkFailedOperation(telemtryOperation);
                                    ApplicationInsightsTelemetry.TrackException(ex);
                                    throw;
                                }
                                finally
                                {
                                    ApplicationInsightsTelemetry.StopOperation(telemtryOperation);
                                }
                                await ProcessCompletedMessage(message, context);
                                executionSucceeded = true;
                            } while (!cancellationToken.IsCancellationRequested);
                        }
                        catch (Exception ex)
                        {
                            await LogError("QueueTriggerBinding", "RunAsync", ex);
                            await ProcessFailedMessage(message);
                            executionSucceeded = false;
                        }
                        finally
                        {
                            await Task.Delay(_delayStrategy.GetNextDelay(executionSucceeded), cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    globalEx = ex;                    
                }
                finally
                {
                    var msg =
                        $"Process ended. Exception={globalEx?.Message + globalEx?.StackTrace}. Token.IsCancellationRequested={cancellationToken.IsCancellationRequested}";
                    await _log.WriteInfoAsync("QueueTriggerBinding", "RunAsync", _queueName, msg);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
        }



        private async Task ProcessCompletedMessage(IQueueMessage message, QueueTriggeringContext context)
        {
            switch (context.MovingAction)
            {
                case QueueTriggeringContext.MessageMovingAction.Default:
                    await _queueReader.FinishMessageAsync(message);
                    break;
                case QueueTriggeringContext.MessageMovingAction.MoveToEnd:
                    await MoveToEnd(message, context.NewMessageVersion);
                    break;
                case QueueTriggeringContext.MessageMovingAction.MoveToPoison:
                    await MoveToPoisonQueue(message, context.NewMessageVersion);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await context.Delay(await _queueReader.Count());
        }

        private async Task ProcessFailedMessage(IQueueMessage message)
        {
            if (message == null)
                return;
            try
            {
                if (message.DequeueCount >= _maxDequeueCount)
                    await MoveToPoisonQueue(message, null);
                else
                {
                    await _queueReader.ReleaseMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                await LogError("QueueTriggerBinding", "ProcessFailedMessage", ex);
            }
        }

        private async Task MoveToEnd(IQueueMessage message, string newMessageVersion)
        {
            newMessageVersion = newMessageVersion ?? message.AsString;
            await _queueReader.AddMessageAsync(newMessageVersion);
            await _queueReader.FinishMessageAsync(message);
        }

        private async Task MoveToPoisonQueue(IQueueMessage message, string newMessageVersion)
        {
            newMessageVersion = newMessageVersion ?? message.AsString;
            if (_poisonQueueReader == null)
                _poisonQueueReader = _queueReaderFactory.Create(_connection, _queueName + PoisonSuffix);
            await _poisonQueueReader.AddMessageAsync(newMessageVersion);
            await _queueReader.FinishMessageAsync(message);

            if (_shouldNotify)
                await _notifier.NotifyAsync($"Msg put to {_queueName + PoisonSuffix}, data: {newMessageVersion}");
        }

        private async Task LogError(string component, string process, Exception ex)
        {
            try
            {
                await _log.WriteErrorAsync(component, process, _queueName, ex);
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error in logger: {logEx.Message}. Trace: {logEx.StackTrace}");                
            }
        }
    }
}
