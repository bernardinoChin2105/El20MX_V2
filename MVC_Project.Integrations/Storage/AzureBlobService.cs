using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO; // Namespace for Blob storage types

namespace MVC_Project.Integrations.Storage
{
    public class AzureBlobService : IStorageServiceProvider
    {

        public Tuple<string, string> UploadPublicFile(System.IO.Stream fileStream, string fileName, string containerName, string folder = "")
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            
            //CloudBlobDirectory directory = container.GetDirectoryReference(folder);
            
            //string uuid = Guid.NewGuid().ToString();
            string finalBlobName = folder + "/" + fileName;
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(finalBlobName);
            
            blockBlob.UploadFromStream(fileStream);

            return new Tuple<string, string>(blockBlob.StorageUri.PrimaryUri.ToString(), fileName);
        }

        public static Tuple<string, string> UploadFile(System.IO.Stream fileStream, string fileName, string containerName, string folder = "")
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToLower().Replace("_", "-"));
            container.CreateIfNotExists(BlobContainerPublicAccessType.Off);

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Off });

            string uuid = Guid.NewGuid().ToString();
            string finalBlobName = folder + uuid.Substring(24) + "-" + fileName;// + Path.GetExtension(fileName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(finalBlobName);

            blockBlob.UploadFromStream(fileStream);

            return new Tuple<string, string>(blockBlob.StorageUri.PrimaryUri.ToString(), uuid);
        }

        public static bool DeleteFile(string fileUri)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlockBlob blockBlob = new CloudBlockBlob(new Uri(fileUri), storageAccount.Credentials);
                blockBlob.Delete();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static IList<CloudBlockBlob> GetOldFiles(string folder, string containerName)
        {           
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(containerName);
            CloudBlobDirectory directory = container.GetDirectoryReference(folder);
            var blobs = directory.ListBlobs(true).OfType<CloudBlockBlob>().Where(b => (DateTime.UtcNow.AddDays(-30) > b.Properties.LastModified.Value.DateTime)).ToList();
            return blobs;
        }

        public static long ContainerSize(string containerName)
        {
            long fileSize = 0;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            if (container.Exists())
            { 
                foreach (IListBlobItem blobItem in container.ListBlobs(null, true, BlobListingDetails.None))
                {
                    if (blobItem is CloudBlockBlob)
                    {
                        CloudBlockBlob blob = blobItem as CloudBlockBlob;
                        //report.FileCount++;
                        //report.TotalBytes += blob.Properties.Length;
                        fileSize += blob.Properties.Length;
                    }
                    else if (blobItem is CloudPageBlob)
                    {
                        CloudPageBlob pageBlob = blobItem as CloudPageBlob;
                        //report.FileCount++;
                        //report.TotalBytes += pageBlob.Properties.Length;
                        fileSize += pageBlob.Properties.Length;
                    }
                    else if (blobItem is CloudBlobDirectory)
                    {
                        CloudBlobDirectory directory = blobItem as CloudBlobDirectory;
                        fileSize = ContainerSize(directory.Container.Name);
                        //report.DirectoryCount++;
                    }
                }
            }
            return fileSize;
        }
        
        public MemoryStream DownloadFile(string containerName, string Url)
        {
            string SASToken = CloudConfigurationManager.GetSetting("StorageSASToken");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Url);

            var fileStream = new MemoryStream();
            blockBlob.DownloadToStream(fileStream);
            return fileStream;
        }
    }
}
