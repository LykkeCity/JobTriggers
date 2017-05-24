﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Lykke.JobTriggers.Triggers.Bindings
{
    [TriggerDefine(typeof(TimerTriggerAttribute))]
    public class TimerTriggerBinding : BaseTriggerBinding
    {
        private readonly ILog _log;
        private IServiceProvider _serviceProvider;
        private MethodInfo _method;

        private TimeSpan _period;
        private string _processId;

        public TimerTriggerBinding(ILog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));
            _log = log;
        }

        public override void InitBinding(IServiceProvider serviceProvider, MethodInfo callbackMethod)
        {
            _serviceProvider = serviceProvider;
            _method = callbackMethod;
            var attribute = _method.GetCustomAttribute<TimerTriggerAttribute>();
            _period = attribute.Period;
            _processId = _method.DeclaringType.Name + "." + _method.Name;
            if (_method.GetParameters().Length > 0)
                throw new Exception($"Method {_method.Name} should be parameterless");
        }

        public override Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                Exception globalEx = null;
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                        try
                        {
                            await Invoke(_serviceProvider, _method, null);
                        }
                        catch (Exception ex)
                        {
                            await LogError("TimerTriggerBinding", "RunAsync", ex);
                        }
                        finally
                        {
                            await Task.Delay(_period, cancellationToken);
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
                    await _log.WriteInfoAsync("TimerTriggerBinding", "RunAsync", _processId, msg);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default).Unwrap();
        }

        private Task LogError(string component, string process, Exception ex)
        {
            try
            {                
                return _log.WriteErrorAsync(component, process, _processId, ex);
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error in logger: {logEx.Message}. Trace: {logEx.StackTrace}");
                return Task.CompletedTask;
            }
        }
    }
}
