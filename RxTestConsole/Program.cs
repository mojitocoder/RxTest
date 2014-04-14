using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RxTestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console: File Watcher");

            //ShowForm_NormalEvents();
            //ShowForm_Rx();
            RunRx(@"C:\Sync");
            Console.ReadLine();
        }

        static void ShowForm_NormalEvents()
        {
            var lbl = new Label();
            var frm = new Form
            {
                Controls = { lbl }
            };

            frm.MouseMove += (sender, a) =>
            {
                lbl.Text = a.Location.ToString(); // This has become a position-tracking label. 
            };

            Application.Run(frm);
        }

        static void ShowForm_Rx()
        {
            var lbl = new Label();
            var frm = new Form
            {
                Controls = { lbl }
            };

            //var moves = Observable.FromEvent<MouseEventArgs>(frm, "MouseMove");
            //using (moves.Subscribe(evt =>
            //{
            //    lbl.Text = evt.EventArgs.Location.ToString();
            //}))
            //{
            //    Application.Run(frm);
            //}

            //var x = Observable.FromEvent(foo => frm.MouseMove, foo => foo += Console.WriteLine(""), foo => foo - +Console.WriteLine(""));
            //Observable.FromEventPattern(


            var y = Observable.FromEventPattern(frm, "MouseMove");

            y.Subscribe(evt =>
            {

                Console.WriteLine(evt);
            });

            Application.Run(frm);
        }

        #region FileWatcher
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run(string directory)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = directory;

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            //watcher.Filter = "*.qvw";

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }

        // Define the event handlers. 
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("OnChanged");
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Console.WriteLine("OnRenamed");
        }

        private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.WriteLine("OnCreated");
        }
        #endregion

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void RunRx(string directory)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = directory;

            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            //watcher.Filter = "*.qvw";

            var sequenceOfChanged = Observable.FromEventPattern(watcher, "Changed");
            var sequenceOfCreated = Observable.FromEventPattern(watcher, "Created");
            var sequenceOfDeleted = Observable.FromEventPattern(watcher, "Deleted");
            var sequenceOfRenamed = Observable.FromEventPattern(watcher, "Renamed");

            sequenceOfChanged.Merge(sequenceOfCreated).Merge(sequenceOfDeleted).Merge(sequenceOfRenamed).Subscribe(evt =>
            {
                Console.WriteLine("Some random change");
            });

            //sequenceOfDeleted.Subscribe(evt =>
            //{
            //    Console.WriteLine("Deleted");
            //});

            // Begin watching.
            watcher.EnableRaisingEvents = true;

            // Wait for the user to quit the program.
            //Console.WriteLine("Press \'q\' to quit the sample.");
            //while (Console.Read() != 'q') ;
        }
    }
}
