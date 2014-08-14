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
using System.Text.RegularExpressions;

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

            //FifthLesson();

            //SixthLesson();

            //SeventhLesson();

            //EighthLesson();

            //NinthLesson();

            TenthLesson();

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
                                .SubscribeOn(Scheduler.ThreadPool)
                                .ObserveOn(Scheduler.ThreadPool)
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

        /// <summary>
        /// Using finally to do something at the end of the observable sequence
        /// </summary>
        static void SixthLesson()
        {
            var seq = Enumerable.Range(1, 10).Select(no =>
            {
                Thread.Sleep(250);
                return no;
            });

            var seqDone = new ManualResetEvent(false);

            var observable = seq.ToObservable(Scheduler.ThreadPool).Finally(() => seqDone.Set());

            observable.Subscribe(no => Console.WriteLine(no), ex => Console.WriteLine("Error: {0}", ex.Message), () => Console.WriteLine("Mission completed"));

            seqDone.WaitOne();

            Console.WriteLine("Done");
        }

        /// <summary>
        /// Using .Scan method
        /// </summary>
        static void SeventhLesson()
        {
            var random = new Random();

            //Generate a sequence of random numbers
            var seq = Enumerable.Range(1, 100).Select(foo => random.NextDouble()).ToObservable();

            //Do some processing on the sequence
            // The key lesson here is the same accumulator object will be used for the whole subscription of .Scan
            var runningAvg = seq.Scan(new double[] { 0, 0, 0 }, (accumulator, value) =>
            {
                accumulator[2] = accumulator[1];
                accumulator[1] = accumulator[0];
                accumulator[0] = value;
                return accumulator;
            }).Select(accumulator => accumulator.Sum() / 3);

            seq.Subscribe(no => Console.WriteLine("Original number: {0}", no));
            runningAvg.Subscribe(no => Console.WriteLine("Running avg: {0}", no));

            Console.ReadLine();
        }

        /// <summary>
        /// Using .Buffer to process a bunch of subjects at a time
        ///     I.e. Partition the sequence by size or time
        /// </summary>
        static void EighthLesson()
        {
            var seq = Enumerable.Range(1, 103);

            //Buffer with size
            var observable = seq.ToObservable().Buffer(3).Subscribe(lst =>
            {
                Console.WriteLine("List of {0}:", lst.Count);
                foreach (var item in lst)
                {
                    Console.WriteLine("\t{0}", item);
                }
            });

            //Buffer with time
            Console.WriteLine("Buffering with time:");
            var random = new Random();
            var infiSeq = GenerateInfiniteList().Select(no =>
            {
                Thread.Sleep(random.Next(1, 30) * 100);
                return no;
            })
            .ToObservable()
            .SubscribeOn(Scheduler.TaskPool);

            var sub = infiSeq.Subscribe(no =>
            {
                Console.WriteLine("\t{0}", no);
            });

            var infiTimeSeq = infiSeq.Buffer(TimeSpan.FromSeconds(5)).SubscribeOn(Scheduler.TaskPool);
            var timeSub = infiTimeSeq.Subscribe(lst =>
            {
                Console.WriteLine("\tBuffered: {0}", lst.Count);
            });

            Console.ReadKey();
            sub.Dispose();
            timeSub.Dispose();
            Console.WriteLine("Subscription stopped.");
        }

        /// <summary>
        /// Using .Window to process a bunch of subjects at a time
        ///     .Window will produce a sequence of sequences
        /// </summary>
        static void NinthLesson()
        {
            var random = new Random();
            var infiSeq = GenerateInfiniteList()
                .Select(no => { //Slow down the sequence to see the effect
                    Thread.Sleep(random.Next(1, 30) * 100);
                    return no;
                })
                .ToObservable();

            var windowedSeq = infiSeq.Window(TimeSpan.FromSeconds(5), 5).Subscribe(window =>
            {
                Console.WriteLine("Window");
                window.Subscribe(no => Console.WriteLine("\t{0}", no));
            });

        }

        /// <summary>
        /// Using Regex with Rx
        /// </summary>
        static void TenthLesson()
        {
            var sInput = @"Premier League chief Richard Scudamore is still in favour of adding a controversial 39th game to the season - and says he thinks the clubs are too.
The idea of playing an extra round of fixtures abroad was first floated in 2008, but drew fierce criticism from football fans and the media.
But Scudamore told the BBC: 'The clubs wanted it then and they all would still probably want it now.
'It will happen at some point - whether it is on my watch, who knows?'
But Scudamore, speaking at the official launch of the 2014-15 Premier League season - his first public engagement since he was widely criticised for making sexist comments in emails to colleagues - conceded there was no reason to think an additional round of league fixtures would do any more to attract fans than some of the pre-season games that already take place.";

            var regex = new Regex(@"he[\s]");

            var seq = RegexMatchToEnum(regex, sInput).ToObservable();

            seq.Subscribe(match => Console.WriteLine(match.Value));

            //seq.Catch()
            //seq.Catch()
        }

        static IEnumerable<Match> RegexMatchToEnum(Regex regex, string input)
        {
            var match = regex.Match(input);
            while (match.Success)
            {
                yield return match;
                match = match.NextMatch();
            }
        }

        static IEnumerable<int> GenerateInfiniteList()
        {
            var random = new Random();
            while (true)
            {
                yield return random.Next();
            }
        }
    }
}
