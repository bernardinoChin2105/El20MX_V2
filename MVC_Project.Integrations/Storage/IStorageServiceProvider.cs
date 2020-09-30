using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Integrations.Storage
{
    public interface IStorageServiceProvider
    {
        Tuple<string, string> UploadPublicFile(System.IO.Stream fileStream, string fileName, string containerName, string folder = "");
        System.IO.MemoryStream DownloadFile(string containerName, string Url);
    }
}
