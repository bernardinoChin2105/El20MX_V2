using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace MVC_Project.Web.Controllers
{
    public class PdfSignatureController : Controller
    {
        // GET: PdfSignature
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GeneratePdf(string signature)
        {
            MemoryStream stream = this.CreateSignedPDF(signature);
            var pdfContent = stream.ToArray();
            stream.Dispose();
            return File(pdfContent, "application/pdf", "Contrato-9202390.pdf");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GenerateJpeg(string signature)
        {
            MemoryStream pdfStream = this.CreateSignedPDF(signature);

            int desired_x_dpi = 96;
            int desired_y_dpi = 96;

            //Reference to Ghostscript
            string path = Server.MapPath("~/Libs/gsdll32.dll");
            GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(@path);

            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(pdfStream, gvi, true);

                //Only get first page
                var img = rasterizer.GetPage(desired_x_dpi, desired_y_dpi, 1);
                MemoryStream imageStream = new MemoryStream();
                img.Save(imageStream, ImageFormat.Jpeg);
                pdfStream.Dispose();
                var imageContent = imageStream.ToArray();
                imageStream.Dispose();
                return File(imageContent, "image/jpeg", "Contrato-9202390.jpeg");
            }
        }

        private MemoryStream CreateSignedPDF(string signature)
        {
            //Create PDF Document
            Document document = new Document();

            //Define document styles
            Style style = document.Styles["Normal"];
            style.Font.Name = "sans-serif";
            style.Font.Size = 12;
            style.Font.Bold = false;

            style = document.Styles["Heading1"];
            style.Font.Size = 24;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceBefore = 0;
            style.ParagraphFormat.SpaceAfter = 15;

            style = document.Styles["Heading3"];
            style.Font.Size = 13.5;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceBefore = 13.5;
            style.ParagraphFormat.SpaceAfter = 13.5;

            //Create document section
            Section section = document.AddSection();

            //Logo
            string pathLogo = Server.MapPath("~/Images/Anytime_Fitness_logo.png");
            section.AddImage(pathLogo);

            //Add elements to section
            Paragraph paragraph = section.AddParagraph("Contrato 9202390", "Heading1");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            paragraph = section.AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddText("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Ut sagittis nulla tempus mi sodales ultricies. " +
                "Nulla hendrerit, justo quis ultricies pellentesque, est urna tincidunt nunc, vel faucibus ante leo id metus. Duis tempus " +
                "eleifend venenatis. Suspendisse faucibus nibh non est molestie, eu pretium ligula facilisis. Etiam laoreet venenatis eros " +
                "vitae dictum. Proin faucibus eget magna non accumsan. Proin cursus congue velit imperdiet tempor. Donec eu diam ac sem facilisis " +
                "tristique vitae et arcu. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas.");
            paragraph.Format.SpaceAfter = "10cm";

            paragraph = section.AddParagraph("Firma", "Heading3");
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            //Format signature image data and add to section
            var encodedImage = signature.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)[1];
            var image = section.AddImage("base64:" + encodedImage);
            image.Width = "10cm";
            image.Left = "3cm";

            //Render PDF document
            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(false);
            pdfRenderer.Document = document;
            pdfRenderer.RenderDocument();

            //Download PDF file
            MemoryStream stream = new MemoryStream();
            pdfRenderer.PdfDocument.Save(stream, false);

            return stream;
        }
    }
}