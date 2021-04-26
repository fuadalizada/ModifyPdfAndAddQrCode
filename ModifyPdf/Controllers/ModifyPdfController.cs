using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using iTextSharp.text.pdf;
using System.Web.Http;
using iTextSharp.text;
using QRCoder;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;

namespace ModifyPdf.Controllers
{
    public class ModifyPdfController : ApiController
    {
        public bool IsFirstValuePortrait { get; set; }
        public static readonly List<string> ImageExtensions = new List<string> { ".jpg", ".jpeg", ".bmp", ".gif", ".png" };
        public int PageRotation { get; set; }
        public Rectangle PageOriginalSize { get; set; }

        #region QrCode
        [HttpGet]
        [Route("api/ModifyPdf/Modify")]
        public IHttpActionResult Modify(string fileLocation, string outLocation, string newFileLocation, string imageLocation, string actionName, string docNumtext, string docNumber, string datetime)
        {
            if (ImageExtensions.Contains(Path.GetExtension(fileLocation)))
            {
                if (File.Exists(fileLocation))
                {
                    fileLocation = ConvertImageToPdf(fileLocation, imageLocation);
                }
                else
                {
                    return Ok(false + " File movcud deyil");
                }
            }

            using (var reader = new PdfReader(fileLocation))
            {
                using (var fileStream = new FileStream(outLocation, FileMode.Create, FileAccess.Write))
                {
                    var pageSize = GetPageSize(reader, 1);
                    var document = new Document(pageSize);
                    var writer = PdfWriter.GetInstance(document, fileStream);
                    document.Open();

                    for (var i = 1; i <= reader.NumberOfPages; i++)
                    {
                        pageSize = GetPageSize(reader, i);
                        document.SetPageSize(pageSize);
                        document.NewPage();
                        var arialFontPath = HttpContext.Current.Server.MapPath("/Template/ARIALUNI.TTF");
                        FontFactory.Register(arialFontPath);
                        var baseFont = BaseFont.CreateFont(arialFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                        var importedPage = writer.GetImportedPage(reader, i);

                        var contentByte = writer.DirectContent;
                        contentByte.BeginText();
                        contentByte.SetFontAndSize(baseFont, 10f);
                        contentByte.EndText();
                        contentByte.SetLineWidth(0f);

                        string docText = $"{docNumtext}";
                        string bottomText = string.Concat(docText, docNumber);
                        string date = $"{datetime}";

                        string qrCodeLocation = GenerateQrCode(actionName, docNumber);
                        Image image = Image.GetInstance(qrCodeLocation);
                        image.ScaleAbsoluteWidth(54.5f);
                        image.ScaleAbsoluteHeight(48.5f);

                        float pageoriginwidth = PageOriginalSize.Width;
                        float pageoriginhe = PageOriginalSize.Height;
                        float pagesizerotationw = pageSize.Width;
                        float pagesizerotationh = pageSize.Height;

                        if (PageRotation == 0)
                        {
                            if (pageSize.Width < pageSize.Height)
                            {
                                contentByte.AddTemplate(importedPage, 1f, 0, 0, 1f, 0, 0);
                                contentByte.MoveTo(50, document.Bottom + 27f);
                                contentByte.LineTo(553, document.Bottom + 27f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, bottomText, 50, 42, 0);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);

                                image.SetAbsolutePosition(pageSize.Width - 96, 5);
                                contentByte.AddImage(image, false);
                            }
                        }
                        else if (PageRotation == 90)
                        {
                            if (!(pageSize.Width.Equals(PageOriginalSize.Width) && pageSize.Height.Equals(PageOriginalSize.Height)))
                            {
                                contentByte.AddTemplate(importedPage, 0, -1f, 1f, 0, 0, pageSize.Height);
                                contentByte.MoveTo(50, document.Bottom + 27f);
                                contentByte.LineTo(553, document.Bottom + 27f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, bottomText, 50, 42, 0);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);

                                image.SetAbsolutePosition(pageSize.Width - 96, 5);
                                contentByte.AddImage(image, false);
                            }
                            else
                            {
                                contentByte.AddTemplate(importedPage, 0, 0);
                                contentByte.MoveTo(520, document.Bottom + 8f);
                                contentByte.LineTo(520, document.Bottom + 780f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);


                                image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                                contentByte.AddImage(image, false);
                            }
                        }
                        else if (PageRotation == 180)
                        {
                            if (pageSize.Width > pageSize.Height)
                            {
                                contentByte.AddTemplate(importedPage, -1f, 0, 0, -1f, pageSize.Width, pageSize.Height);
                                contentByte.MoveTo(50, document.Bottom + 27f);
                                contentByte.LineTo(553, document.Bottom + 27f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, bottomText, 50, 42, 0);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);

                                image.SetAbsolutePosition(pageSize.Width - 96, 5);
                                contentByte.AddImage(image, false);
                            }
                            else
                            {
                                contentByte.AddTemplate(importedPage, 0, 0);
                                contentByte.MoveTo(520, document.Bottom + 8f);
                                contentByte.LineTo(520, document.Bottom + 780f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);

                                image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                                contentByte.AddImage(image, false);
                            }
                        }
                        else if (PageRotation == 270)
                        {
                            if (pageSize.Width > pageSize.Height)
                            {
                                contentByte.AddTemplate(importedPage, 0, 1f, -1f, 0, pageSize.Width, 0);
                                contentByte.MoveTo(50, document.Bottom + 27f);
                                contentByte.LineTo(553, document.Bottom + 27f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, bottomText, 50, 42, 0);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);

                                image.SetAbsolutePosition(pageSize.Width - 96, 5);
                                contentByte.AddImage(image, false);
                            }
                            else
                            {
                                contentByte.AddTemplate(importedPage, 0, 0);
                                contentByte.MoveTo(520, document.Bottom + 8f);
                                contentByte.LineTo(520, document.Bottom + 780f);
                                contentByte.Stroke();
                                contentByte.BeginText();
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);

                                image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                                contentByte.AddImage(image, false);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unexpected page rotation: [{PageRotation}].");
                        }

                        //contentByte.AddTemplate(importedPage, 0, 0);
                        //contentByte.MoveTo(520, document.Bottom + 8f);
                        //contentByte.LineTo(520, document.Bottom + 780f);
                        //contentByte.Stroke();
                        //contentByte.BeginText();
                        //contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                        //contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);


                        //image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                        //contentByte.AddImage(image, false);

                        //if (IsPortrait(reader, i))
                        //{
                        //    if (IsFirstValuePortrait)
                        //    {
                        //        contentByte.AddTemplate(importedPage, 0, 0);
                        //        contentByte.MoveTo(520, document.Bottom + 8f);
                        //        contentByte.LineTo(520, document.Bottom + 780f);
                        //        contentByte.Stroke();
                        //        contentByte.BeginText();
                        //        contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                        //        contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);


                        //        image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                        //        contentByte.AddImage(image, false);
                        //    }
                        //    else
                        //    {
                        //        contentByte.AddTemplate(importedPage, 0, 0);
                        //        contentByte.MoveTo(50, document.Bottom + 27f);
                        //        contentByte.LineTo(553, document.Bottom + 27f);
                        //        contentByte.Stroke();
                        //        contentByte.BeginText();
                        //        contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, bottomText, 50, 42, 0);
                        //        contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);


                        //        image.SetAbsolutePosition(pageSize.Width - 96, 5);
                        //        contentByte.AddImage(image, false);
                        //    }
                        //}
                        //else
                        //{
                        //    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, pageSize.Width, 0);
                        //    contentByte.MoveTo(520, document.Bottom + 8f);
                        //    contentByte.LineTo(520, document.Bottom + 780f);
                        //    contentByte.Stroke();
                        //    contentByte.BeginText();
                        //    contentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, bottomText, 540, 80, 90);
                        //    contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 555, 45, 90);


                        //    image.SetAbsolutePosition(pageSize.Width - 77, 740);//64---770    
                        //    contentByte.AddImage(image, false);
                        //}

                        contentByte.EndText();
                    }

                    document.Close();
                    writer.Close();
                    fileStream.Close();
                }
            }
            if (File.Exists(outLocation))
            {
                string oldFileName = Path.GetFileName(fileLocation);
                string newFilePath = newFileLocation + oldFileName;

                File.Move(fileLocation, newFilePath);
                File.Move(outLocation, fileLocation);

                return Ok(true);
            }
            return Ok(false);
        }

        public string ConvertImageToPdf(string srcFilename, string dstFilename)
        {
            var document = new Document(PageSize.A4, 25, 25, 25, 25);
            using (var stream = new FileStream(dstFilename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();
                using (var imageStream = new FileStream(srcFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var image = Image.GetInstance(imageStream);
                    if (image.Height > PageSize.A4.Height - 25)
                    {
                        image.ScaleToFit(PageSize.A4.Width - 25, PageSize.A4.Height - 25);
                    }
                    else if (image.Width > PageSize.A4.Width - 25)
                    {
                        image.ScaleToFit(PageSize.A4.Width - 25, PageSize.A4.Height - 25);
                    }
                    image.Alignment = Element.ALIGN_MIDDLE;
                    document.Add(image);
                }

                document.Close();
                return dstFilename;
            }
        }

        #region GenerateQrCode

        public string GenerateQrCode(string actionName, string docNumber)
        {
            string text = $"https://edoc.ady.az/History/{actionName}?documentId={docNumber}";
            PayloadGenerator.Url generator = new PayloadGenerator.Url(text);
            string payload = generator.ToString();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeAsBitmap = qrCode.GetGraphic(20);
            qrCodeAsBitmap.Save(HttpContext.Current.Server.MapPath($"/Template/{docNumber}.jpeg"), System.Drawing.Imaging.ImageFormat.Jpeg);
            return HttpContext.Current.Server.MapPath($"/Template/{docNumber}.jpeg");
        }

        #endregion

        public Rectangle GetPageSize(PdfReader reader, int pageNumber)
        {
            PageOriginalSize = reader.GetPageSize(pageNumber);
            Rectangle pageSize = reader.GetPageSizeWithRotation(pageNumber);
            PageRotation = reader.GetPageRotation(pageNumber);
            return new Rectangle(
                Math.Min(pageSize.Width, pageSize.Height),
                Math.Max(pageSize.Width, pageSize.Height));
            //Rectangle pageSize = reader.GetPageSizeWithRotation(pageNumber);

            //if (pageSize.Width > pageSize.Height)
            //{
            //    IsFirstValuePortrait = true;
            //}

            //return new Rectangle(
            //    Math.Min(pageSize.Width, pageSize.Height),
            //    Math.Max(pageSize.Width, pageSize.Height));
        }

        public Boolean IsPortrait(PdfReader reader, int pageNumber)
        {
            Rectangle pageSize = reader.GetPageSize(pageNumber);
            return pageSize.Height > pageSize.Width;
        }

        #endregion

        #region CreatorPdfForCreator

        [HttpGet]
        [Route("api/ModifyPdf/CreatePdfForCreator")]
        public IHttpActionResult CreatePdfForCreator(string fullName, string profession, string workPhone, string text, string signDate, string qrCodeLocation, string fileLocation)
        {
            using (var fileStream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);
                var writer = PdfWriter.GetInstance(document, fileStream);
                document.Open();

                var arialFontPath = HttpContext.Current.Server.MapPath("/Template/ARIALUNI.TTF");
                FontFactory.Register(arialFontPath);
                var baseFont = BaseFont.CreateFont(arialFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                var contentByte = writer.DirectContent;
                contentByte.BeginText();
                contentByte.SetFontAndSize(baseFont, 12f);

                string executorName = $"{fullName}";
                string prof = $"{profession}";
                string phone = $"{workPhone}";
                string docNumber = $"{text}";
                string date = $"{signDate}";

                Rectangle pageSize = new Rectangle(document.PageSize);

                contentByte.EndText();
                contentByte.SetLineWidth(0f);
                contentByte.MoveTo(50, document.Bottom + 27f);
                contentByte.LineTo(553, document.Bottom + 27f);
                contentByte.Stroke();
                contentByte.BeginText();
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "İcraçı:", 50, 130, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, prof + " " + executorName, 50, 105, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, "Telefon: " + phone, 50, 80, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, docNumber, 50, 42, 0);
                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, date, 50, 27, 0);


                Image image = Image.GetInstance(qrCodeLocation);
                image.ScaleAbsoluteWidth(54.5f);
                image.ScaleAbsoluteHeight(48.5f);
                image.SetAbsolutePosition(pageSize.Width - 96, 5);
                contentByte.AddImage(image, false);

                contentByte.EndText();


                document.Close();
                writer.Close();
                fileStream.Close();
            }

            return Ok(true);
        }
        #endregion
    }
}
