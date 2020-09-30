using MVC_Project.Integrations.Storage;
using MVC_Project.WebBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Project.WebBackend.Controllers
{
    public class StorageController : Controller
    {
        public ActionResult Index()
        {
            UserImportViewModel model = new UserImportViewModel();
            return View(model);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Import(UserImportViewModel model)
        {
            IStorageServiceProvider storageService = null;

            //if (model.StorageProvider == "azure") storageService = new AzureBlobService();
            if (model.StorageProvider == "aws") storageService = new AWSBlobService();
            if (storageService != null)
            {
                string myBucketName = System.Configuration.ConfigurationManager.AppSettings["AWSBucketName"];
                Tuple<string, string> resultUpload = storageService.UploadPublicFile(model.ImportedFile.InputStream, model.ImportedFile.FileName, myBucketName);
                model.FinalUrl = resultUpload.Item1;
            }
            
            return View("Index", model);
        }
    }
}