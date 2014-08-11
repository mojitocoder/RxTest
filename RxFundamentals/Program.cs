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
using System.IO;

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

            //ThirdLesson();

            //FourthLesson();

            FifthLesson();

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
            var iSubscription = Enumerable.Range(1, 100).Select(no =>
            {
                Thread.Sleep(250);
                return no;
            })
            .ToObservable(Scheduler.ThreadPool)
            .ObserveOn(Scheduler.CurrentThread)
            .Subscribe(no => Console.WriteLine(no));

            Console.ReadKey();
            iSubscription.Dispose();
            Console.WriteLine("Subscription has been terminated.");

            //
            //Observable.Create
        }

        /// <summary>
        /// Using Observable.Using to read a text file as a stream of characters
        /// </summary>
        static void FourthLesson()
        {
            string sFilePath = @"C:\Temp\test.txt";

            //func to open the file into a stream
            Func<StreamReader> funcReadStream = () =>
                {
                    return new StreamReader(new FileStream(sFilePath, FileMode.Open));
                };

            var observable = Observable.Using<char, StreamReader>(funcReadStream, streamReader => streamReader.ReadToEnd().ToCharArray().ToObservable());

            observable.Subscribe(Console.WriteLine);
        }

        /// <summary>
        /// Read a text file into an observable of strings
        /// </summary>
        static void FifthLesson()
        {
            var sFilePath = @"C:\Temp\RANALYTC.F9UMV0PJ.ROSS.MAR14.01";

            //bufferSize > 64k as per BeginRead spec: http://msdn.microsoft.com/en-us/library/zxt5ahzw.aspx
            var bufferSize = 2 << 16;

            using (var stream = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var sub = stream.AsyncReadLines(bufferSize)
                                .Subscribe(line =>
                                {
                                    Console.WriteLine(line);
                                    Console.WriteLine();
                                    Thread.Sleep(500);
                                });

                Console.ReadKey();
                sub.Dispose();
                Console.WriteLine("Stop reading the file");
            }

            //constructor of FileStream that enables asynchronous operations
            //var stream = new FileStream(@"d:\temp\input.txt", FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
            //stream.AsyncReadLines(bufferSize).Subscribe(Console.WriteLine);
        }
    }
}
