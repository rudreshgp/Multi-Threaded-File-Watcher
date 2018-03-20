# .Net Core Multi Threaded File Watcher
This repo contains .Net core console application to run two threads at same time. In which one thread writes Objects to a file from a write queue as json objects and other threads watches this file and if there are any changes into the file it reads and converts those json objects into objects and puts them into read queue. 

## Write Thread Setup
Write Thread requires a Type, blocking queue and a file. Type to define Write Thread for a particular type of object and use json serializer to serialize data object. The blocking queue is fed from the application with new objects and write threads pop from this queue for new objects and serializes it into json string and writes to mentioned file.

### Write Thread for Type SampleData
```csharp
public class SampleData
{
  public int Id { get; set; }
  public string Details { get; set; }
  public string Time { get; set; }
}

//usage
var writeBlockingCollection = new BlockingCollection<SampleData>();
var writeThreadObject = new WriteThread<SampleData>(writeBlockingCollection, "Output.txt"); //Where Output.txt is the output file name
var writeThread = new Thread(writeThreadObject);
writeThread.Start();
//Start pushing objects into writeBlockingCollection. these objects will be serialized into the file
  ```
  
## Read Thread
Read Thread always looks for a change in the passed file and if any new object comes it deserializes it into menioned type and puts into a blocking queue. Read Thread requires a BlockingQueue where deserialized objects are pushed and a folder and a file name to watch for changes. 
### Read Thread for Type SampleData
```csharp
//usage
var readBlockingCollection = new BlockingCollection<SampleData>();
var readThreadObject = new ReadThread<SampleData>(readBlockingCollection,"Output.txt",""); //Where Output.txt is the input file name, 3rd parameter is folder path, if no path provided application root folder is selected
var readThread = new Thread(readThreadObject);
readThread.Start();

//You can start poping from readBlockingCollection simultaniously
  ```
## Sample Application
A sample read and write thread is created in `Program.cs` file.

```csharp
//clone code
//change command line inside the parent directory of the project
//restore all dependencies
dotnet restore
dotnet run
```

## Contribution
I will be very much happy if you want to add new features, please create a pull request or if any issues please log them.
