using Amazon.S3.Transfer;
using Amazon.S3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;

namespace MessageSenderAgent.Utility
{
    public class AmazonUploader
    {


        public bool SendFileToS3( string localFilePath, string bucketName, string subDirectoryInBucket, string fileNameInS3)
        {

            //the key and access key are private, needs developer-specific value.
            IAmazonS3 client = new AmazonS3Client(awsAccessKeyId: "YOUR_AWS_ACCESS_KEY",
                                                  awsSecretAccessKey: "YOUR_AWS_SECRET_KEY", RegionEndpoint.USEast2);

            TransferUtility utility = new TransferUtility(client);
            TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();

            request.BucketName = bucketName;
            //if (string.IsNullOrEmpty(subDirectoryInBucket))
            //{
            //    request.BucketName = bucketName; //no subdirectory just bucket name  
            //}
            //else
            //{   // subdirectory and bucket name  
            //    request.BucketName = bucketName + @"/" + subDirectoryInBucket;
            //}

            request.Key = subDirectoryInBucket + fileNameInS3; //file name up in S3  
            //request.InputStream = localFilePath;
            request.FilePath = localFilePath;

            utility.Upload(request); //commensing the transfer  

            return true; //indicate that the file was sent  
        }
    }
}
