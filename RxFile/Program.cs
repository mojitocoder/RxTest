using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;


namespace RxFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start playing the spying game:");
            Observable.Range(0, 1).Spy("Range").Subscribe();


            Console.WriteLine("\n\n\nPlaying with files using Rx");
            var sFilePath = @"C:\Temp\DnB_TestData\RANALYTC.F9UMV0PJ.ROSS.MAR14.01";
            var x = File.ReadLines(sFilePath, Encoding.Default)
                        .ToObservable()
                        .Buffer(100)
                        .Subscribe(foo =>
                        {
                            Console.WriteLine(foo);
                        });


            //Observable.Range()
            Console.ReadLine();
        }
    }

}
