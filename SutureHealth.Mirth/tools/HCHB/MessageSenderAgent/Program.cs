// See https://aka.ms/new-console-template for more information
using MessageSenderAgent;
using MessageSenderAgent.Utility;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapiTools.Base.Net;
using System.Diagnostics;
using System.Text;



string destinationFile = Path.GetTempPath();
int PORT_NUMBER = 8443;//57550;// change this to whatever your port number is
const int NUMBER_OF_SAMPLES = 500;
string SERVE_RNAME = "10.50.90.21"; //"localhost";
try
{
    IMessage message;
    var connection = new SimpleMLLPClient(SERVE_RNAME, PORT_NUMBER, Encoding.UTF8);      
    
    PipeParser parser = new PipeParser();
    string msg = String.Empty;
    //string myBucketName = "hl7messagebucket"; //your s3 bucket name goes here  
    string s3DirectoryName = string.Empty;

    for (int i = 0; i < NUMBER_OF_SAMPLES; i++) 
    {
        
        destinationFile = Path.Combine(destinationFile, "message_lc.txt");
        
        message = MessageFactory.CreateMessage();        
        msg = parser.Encode(message);

        /*
        File.WriteAllBytes(destinationFile, Encoding.ASCII.GetBytes(msg));

        if (message.GetType().Name.Contains("ADT")) 
        {
            s3DirectoryName = @"testdataset/";
        }
        
        string s3FileName = message.GetType().Name.Substring(message.GetType().Name.Length-3,3) +"/" +i.ToString("000") + (".txt");       
        AmazonUploader myUploader = new AmazonUploader();
        bool a = myUploader.SendFileToS3(destinationFile, myBucketName, s3DirectoryName, s3FileName);
        if (a == true)
        {
            Console.WriteLine("successfully uploaded");

        }
        else
            Console.WriteLine("Error");
        */

        var response = connection.SendHL7Message(msg);
        // display the message response received from the remote party    
        LogToDebugConsole("Received response:\n" + response);

    }

}
catch (Exception e)
{
    LogToDebugConsole($"Error occured while creating and transmitting HL7 message {e.Message}");
}

static void LogToDebugConsole(string informationToLog)
{
    Debug.WriteLine(informationToLog);
}

