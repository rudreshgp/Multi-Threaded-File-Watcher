using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;


namespace WriteReadSameFileThreading.ReaWriteThred
{
    //  Summary:
    ///     Seariylizes objects in the BlockingCollection using Newtonsoft.Json and writes the serialized content to output file
    //  Parameters:
    //      T:
    //          Type of the object stored in the queue
    public class WriteThread<T> where T : class
    {
        //  Summary:
        //      The blocking queue which will be filled by application and the objects in this queue are serialized onto file
        private readonly BlockingCollection<T> _writeBlockingCollection;

        //  Summary:
        //      Name or path of the file to which serialized data is written
        private readonly string _outputFileName;

        //  Summary:
        //      Initializes write thread with the queue and the output filename
        // Parameters:
        //      writeBlockingCollection:
        //          BlockingCollection from which objects need to be peeked
        //      outputFileName:
        //          name or path of the file to which output is written
        public WriteThread(BlockingCollection<T> writeBlockingCollection, string outputFileName)
        {
            _writeBlockingCollection = writeBlockingCollection;
            _outputFileName = outputFileName;
        }

        //  Summary:
        //      Peeks data from blocking queue and writes it into the file as json
        public void WriteData()
        { 
            //Open the file and write the contents and also share file to read from other threads or processes
            using (var fileStream = new FileStream(_outputFileName, FileMode.Append, FileAccess.Write,
                                  FileShare.Read))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.AutoFlush = true;
                foreach (var item in _writeBlockingCollection.GetConsumingEnumerable())
                {
                    streamWriter.WriteLine(JsonConvert.SerializeObject(item));
                    //streamWriter.Flush();
                }
            }
        }
    }
}
