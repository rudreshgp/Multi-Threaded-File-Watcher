using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace WriteReadSameFileThreading.ReaWriteThred
{
    //  Summary:
    //      Watches file for any changes
    //  Parameters:
    //      T:
    //          Type of the class or object need to be read
    public class ReadThread<T> where T : class
    {
        //  Summary:
        //      Queue to add read items
        private readonly BlockingCollection<T> _blockingCollection;

        //  Summary:
        ///     Input file name from which items to be read
        private readonly string _inputFileName;

        //  Summary:
        //     Folder in which file present else application path is choosen
        private readonly string _fileFolder;

        //  Summary:
        //      Initialize a read thread and feed blocking collection
        //  Parameters:
        //      blockingCollection:
        //          The queue to which data need to be put
        //      inputFileName:
        //          name of the file from which data to be read
        //      fileFolder:
        //          folder which contains the file
        public ReadThread(BlockingCollection<T> blockingCollection, string inputFileName, string fileFolder = null)
        {
            _blockingCollection = blockingCollection;
            _inputFileName = inputFileName;
            _fileFolder = fileFolder ?? AppContext.BaseDirectory;
        }

        //  Summary:
        //      Read until program is running
        //      <see href="https://stackoverflow.com/questions/3791103/c-sharp-continuously-read-file"/>
        public void ReadItem()   
        {
            // Wait till file is avavilable TODO: probably better timeout strategy might be implemented
            while (!File.Exists(_inputFileName)) { }
            var waitHandler = new AutoResetEvent(false); // To handle when new data is written by the Write Thread
            using (var fileSystemWatcher = new FileSystemWatcher(_fileFolder, _inputFileName))
            {
                fileSystemWatcher.EnableRaisingEvents = true;
                fileSystemWatcher.Changed += (s, e) => waitHandler.Set();
                //Allow file read write so that write thread can write data and other threads can read data
                using (var fileStream = new FileStream(_inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string newData = null;
                    while (Program.Running || newData != null)  //parse untill program is running and data not fully read from the file
                    {
                        newData = streamReader.ReadLine();
                        if (newData != null)
                            _blockingCollection.Add(JsonConvert.DeserializeObject<T>(newData));
                        else if (Program.Running)
                            waitHandler.WaitOne(100);
                    }
                }
            }
            _blockingCollection.CompleteAdding();
        }
    }
}
