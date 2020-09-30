using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using Amazon;

namespace MVC_Project.Integrations.Storage
{
    public class AWSBlobService: IStorageServiceProvider
    {
        public MemoryStream DownloadFile(string containerName, string Url)
        {
            throw new NotImplementedException();
        }

        public Tuple<string, string> UploadPublicFile(System.IO.Stream fileStream, string keyName, string bucketName, string folder ="")
        {
            try
            {   
                string uuid = Guid.NewGuid().ToString();
                string finalBlobName = folder + uuid.Substring(24) + "-" + keyName;
                
                RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
                IAmazonS3 s3Client = new AmazonS3Client(bucketRegion);
                var fileTransferUtility = new TransferUtility(s3Client);
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    BucketName = bucketName,
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                    PartSize = 6291456, // 6 MB.
                    Key = finalBlobName,
                    CannedACL = S3CannedACL.PublicRead
                };
                //fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
                //fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

                fileTransferUtility.Upload(fileTransferUtilityRequest);

                string finalUri = string.Format( System.Configuration.ConfigurationManager.AppSettings["AWSS3Uri"], bucketName, finalBlobName);

                return new Tuple<string, string>(finalUri, uuid);

            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
                return null;
            }
        }
    }
}
