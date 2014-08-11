using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.PlatformServices;

using System.Reactive.PlatformServices;

namespace RxFundamentals
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello world from Rx Fundamentals");

            //FirstLesson();

            //SecondLesson();

            ThirdLesson();

            Console.ReadKey();
        }

        static void FirstLesson()
        {
            var range = Enumerable.Range(1, 5);

            Console.WriteLine("\nCurrent thread is: {0}\n", Thread.CurrentThread.ManagedThreadId);

            //By default, the subscription runs on the current thread
            range.ToObservable().Subscribe(number =>
            {
                Console.WriteLine("Thread {0}: Number {1}", Thread.CurrentThread.ManagedThreadId, number);
            }, () =>
            {
                Console.WriteLine("First subscription is now done\n");
            });

            //Can tell the subscription to run on a separate thread
            //  by using an overload of the ToObservable()
            range.ToObservable(Scheduler.NewThread).Subscribe(number =>
            {
                Console.WriteLine("Thread {0}: Number {1}", Thread.CurrentThread.ManagedThreadId, number);
            }, () =>
            {
                Console.WriteLine("Second subscription is now done");
            });
        }

        /// <summary>
        /// How Rx implements Observer pattern by the Gang of Four
        /// Need an IObservable and an IObserver to connect between the two
        /// </summary>
        static void SecondLesson()
        {
            Console.WriteLine("\nAn example to show how Rx implements Observer pattern by the GOF:");
            var iObservable = Enumerable.Range(1, 6).ToObservable();
            var iObserver = Observer.Create<int>(number =>
            {
                Console.WriteLine("From a iObserver: {0}", number);
            });
            iObservable.Subscribe(iObserver);
        }

        /// <summary>
        /// How to control thread of execution for 3 components in Rx: Observable, Observer and Subscription
        /// </summary>
        static void ThirdLesson()
        {
            var observable = Enumerable.Range(1, 100).Select(no =>
            {
                Thread.Sleep(250);
                return no;
            })
            .ToObservable(Scheduler.ThreadPool)
            .ObserveOn(Scheduler.CurrentThread)
            .Subscribe(no => Console.WriteLine(no));
        
            //
            //Observable.Create
        }
    }
}
