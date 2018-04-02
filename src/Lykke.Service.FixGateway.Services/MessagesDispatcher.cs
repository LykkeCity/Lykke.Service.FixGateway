using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.FixGateway.Core.Services;

namespace Lykke.Service.FixGateway.Services
{
    [UsedImplicitly]
    public sealed class MessagesDispatcher<T> : IObservable<T>, ISupportInit
    {
        private readonly RabbitMqSubscriber<T> _subscriber;
        private readonly ILog _log;
        private readonly Subject<T> _subject;

        public MessagesDispatcher(RabbitMqSubscriber<T> subscriber, ILog log)
        {
            _subscriber = subscriber;
            _log = log.CreateComponentScope(GetType().Name);
            subscriber.Subscribe(OnNewEvent);
            _subject = new Subject<T>();
        }

        private Task OnNewEvent(T newEvent)
        {
            _subject.OnNext(newEvent);
            return Task.CompletedTask;
        }

        public void Init()
        {
            _subscriber.Start();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _subject.ObserveOn(new TaskPoolScheduler(new TaskFactory())).Subscribe(nxt =>
            {
                try
                {
                    observer.OnNext(nxt);
                }
                catch (Exception ex)
                {
                    _log.WriteWarning(nameof(Subscribe), "", "", ex);
                }
            });
        }
    }
}
