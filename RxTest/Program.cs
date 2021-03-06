﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RxTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Reactive Extension");

            var sName = "Fist example";
            using (var consoleColour = new ConsoleColour(ConsoleColor.Yellow))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    FirstExample();
                }
            }

            sName = "Second example";
            using (var consoleColour = new ConsoleColour(ConsoleColor.Red))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    SecondExample();
                }
            }

            sName = "Third example";
            Console.WriteLine("\n\n\n{0}:", sName);
            using (var timeIt = new TimeIt(sName))
            {
                ThirdExample();
            }

            sName = "Fourth example";
            using (var consoleColour = new ConsoleColour(ConsoleColor.Magenta))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    FourthExample();
                }
            }

            sName = "Fifth example";
            using (var consoleColour = new ConsoleColour(ConsoleColor.White))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    FifthExample();
                }
            }

            sName = "Sixth example";
            using (var consoleColour = new ConsoleColour(ConsoleColor.Cyan))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    SixthExample();
                }
            }

            sName = "Seventh example - creating IOservable from stream";
            using (var consoleColour = new ConsoleColour(ConsoleColor.DarkRed))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    SeventhExample();
                }
            }

            sName = "Eighth example - creating IOservable from array";
            using (var consoleColour = new ConsoleColour(ConsoleColor.Yellow))
            {
                Console.WriteLine("\n\n\n{0}:", sName);
                using (var timeIt = new TimeIt(sName))
                {
                    EighthExample();
                }
            }



            Console.ReadKey();
        }

        static void WriteSequenceToConsole(IObservable<string> sequence)
        {
            //The next two lines are equivalent.
            //sequence.Subscribe(value=>Console.WriteLine(value));
            sequence.Subscribe(Console.WriteLine);
        }

        static void FirstExample()
        {
            var numbers = new MySequenceOfNumbers();
            var observer = new MyConsoleObserver<int>();
            numbers.Subscribe(observer);
        }

        static void SecondExample()
        {
            var subject = new Subject<string>();
            subject.OnNext("a");

            WriteSequenceToConsole(subject);
            subject.OnNext("b");
            subject.OnNext("c");
        }

        static void ThirdExample()
        {
            var subject = new ReplaySubject<string>();
            subject.OnNext("1");

            WriteSequenceToConsole(subject);
            subject.OnNext("2");
            subject.OnNext("3");
            subject.OnNext("4");
        }

        static void FourthExample()
        {
            var singleValue = Observable.Return<string>("This is the only value");
            WriteSequenceToConsole(singleValue);
        }

        static void FifthExample()
        {
            var sequence = CreateNonBlockingSequence();
            WriteSequenceToConsole(sequence);
        }

        static void SixthExample()
        {
            var naturalNumbers = Corecursion.Unfold<int>(0, foo => foo + 1);
            foreach (var no in naturalNumbers.Take(20))
            {
                Console.WriteLine(no);
            }
        }

        static void SeventhExample()
        {
            //Create a text file
            var sPath = @"C:\Temp\test.txt";
            File.WriteAllText(sPath, @"Life is reactively beautiful.");

            var ObservableChars = Observable.Using<char, StreamReader>(
                                        () => new StreamReader(new FileStream(sPath, FileMode.Open)),
                                        streamReader =>
                                        (
                                            streamReader.ReadToEnd().Select(chr =>
                                            {
                                                Thread.Sleep(100);
                                                return chr;
                                            })
                                            //from str in streamReader.ReadToEnd()
                                            //    select str
                                        )
                                        .ToObservable()
                                    );
            ObservableChars.Subscribe(chr => Console.WriteLine("\t{0}", chr));
        }

        static void EighthExample()
        {
            var numbers = new[] { 1, 2, 3, 4 };
            var observable = numbers.ToObservable();

            var lst = Enumerable.Range(1, 20).ToObservable().Select(foo =>
            {
                Thread.Sleep(50);
                return foo;
            });

            lst.Subscribe(foo => Console.WriteLine("\t{0}", foo));
        }

        private static IObservable<string> CreateNonBlockingSequence()
        {
            return Observable.Create<string>((IObserver<string> observer) =>
            {
                observer.OnNext("a");
                Thread.Sleep(200);

                observer.OnNext("b");
                Thread.Sleep(200);

                observer.OnNext("c");
                Thread.Sleep(200);

                observer.OnNext("d");
                Thread.Sleep(200);

                observer.OnNext("e");
                Thread.Sleep(200);

                observer.OnCompleted();
                Thread.Sleep(200);

                return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                //or can return an Action like
                //return () => Console.WriteLine("Observer has unsubscribed");
            });
        }
    }


    public class MyConsoleObserver<T> : IObserver<T>
    {
        public void OnNext(T value)
        {
            Console.WriteLine("Received value {0}", value);
        }
        public void OnError(Exception error)
        {
            Console.WriteLine("Sequence faulted with {0}", error);
        }
        public void OnCompleted()
        {
            Console.WriteLine("Sequence terminated");
        }
    }

    public class MySequenceOfNumbers : IObservable<int>
    {
        public IDisposable Subscribe(IObserver<int> observer)
        {
            observer.OnNext(1);
            observer.OnNext(2);
            observer.OnNext(3);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }

    public class TimeIt : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _watch;

        public TimeIt(string name)
        {
            _name = name;
            _watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _watch.Stop();
            Console.WriteLine("{0} took {1}", _name, _watch.Elapsed);
        }
    }

    public class ConsoleColour : IDisposable
    {
        private readonly System.ConsoleColor _previousColor;
        public ConsoleColour(System.ConsoleColor color)
        {
            _previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
        }
        public void Dispose()
        {
            Console.ForegroundColor = _previousColor;
        }
    }
}
