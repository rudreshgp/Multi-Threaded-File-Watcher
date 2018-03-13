using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using WriteReadSameFileThreading.DataModel;
using WriteReadSameFileThreading.ReaWriteThred;

//  author:
//      Rudresha, Gulaganjihalli Parameshappa
//  date:
//      13/03/2018 17:07:00
//  summary:
//      Class showing usage of threads reading and writing same file
namespace WriteReadSameFileThreading
{
    /// <summary>
    /// Entry of the application
    /// </summary>
    public class Program
    {
        //  summary:
        //      A queue of type <see cref="SampleData"/> to write to file as json via write thread
        private static readonly BlockingCollection<SampleData> WriteBlockingCollection = new BlockingCollection<SampleData>();

        //  summary:
        //      A queue of type <see cref="SampleData"/> to read from json file via read thread
        private static readonly BlockingCollection<SampleData> ReadBlockingCollection = new BlockingCollection<SampleData>();

        //  summary:
        //      To inform read thread that no more write items
        public static bool Running = true;

        public static void Main(string[] args)
        {
            var writeThread = new WriteThread<SampleData>(WriteBlockingCollection, "Output.txt"); //Create a object for write thread
            var writeThread1 = new Thread(writeThread.WriteData); //Create write thread
            writeThread1.Start(); //start the write thread

            var readThread = new ReadThread<SampleData>(ReadBlockingCollection, "Output.txt"); //Create a object for write thread
            var readThread1 = new Thread(readThread.ReadItem); //Create read thread
            readThread1.Start(); //start read thread

            var random = new Random(50000);
            int count = 0;
            int totalObjects = 50000;
            var writeTask = Task.Run(() =>
            {
                //create some random objects and add them to write queue
                while (count < totalObjects)
                {
                    WriteBlockingCollection.Add(new SampleData(random.Next(), $"Random Data {random.Next()}",
                        DateTime.Now.ToString(CultureInfo.InvariantCulture)));
                    count++;
                }
                //End adding new items to queue thus mentioning end of write operation
                WriteBlockingCollection.CompleteAdding();
                while (WriteBlockingCollection.Count > 0)
                {
                    Thread.Sleep(100);
                    //Just to make sure all are read
                }
                //Set this so that read thread can stop further reading from file
                Running = false;
            });
            var readTask = Task.Run(() =>
            {
                var readCount = 0;
                foreach (var item in ReadBlockingCollection.GetConsumingEnumerable())
                {
                    Console.WriteLine(item.ToString());
                    readCount++;
                }
                //Just verify total count should be
                Console.WriteLine($"Read Count From Read Thread :  {readCount}");
                Debug.Assert(totalObjects==count,"Either File is appended with new values or something wrong, delete Output.txt and recreate it");
            });
            Task.WaitAll(writeTask, readTask);
            writeThread1.Join();
            readThread1.Join();
            Console.WriteLine("Reading and writing from file Completed!. Press any key to continue");
            Console.ReadLine();
        }
    }
}
