
using AI_Manual_dotnet_backend.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using RtfPipe;
using System.Text;
using DocumentFormat.OpenXml.Office2016.Excel;

namespace AI_Manual_dotnet_backend.Controllers
{
    [ApiController]
    [Route("api/generate-docx")]
    public class DocumentController : ControllerBase
    {
        //private readonly TranscriptionService _transcriptionService;
        public DocumentController()
        {
            //_transcriptionService = transcriptionService;
        }
        [HttpPost()]
        public async Task<IActionResult> GenerateDocx([FromBody] DocRequest request)
        {
            if (request == null || request.Content == null || !request.Content.Any())
            {
                return BadRequest(new { error = "No sections provided." });
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (!string.IsNullOrEmpty(request.PreviousDocument))
                {
                    try
                    {
                        byte[] existingBytes = Convert.FromBase64String(request.PreviousDocument);
                        memoryStream.Write(existingBytes, 0, existingBytes.Length);
                        memoryStream.Position = 0;
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { error = $"Error decoding previous document: {ex.Message}" });
                    }
                }

                using (WordprocessingDocument wordDocument = memoryStream.Length > 0
                    ? WordprocessingDocument.Open(memoryStream, true)
                    : WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
                {
                    MainDocumentPart mainPart = wordDocument.MainDocumentPart ?? wordDocument.AddMainDocumentPart();
                    if (mainPart.Document == null)
                    {
                        mainPart.Document = new Document();
                        mainPart.Document.AppendChild(new Body());
                    }

                    Body body = mainPart.Document.Body;

                    if (memoryStream.Length == 0 && !string.IsNullOrEmpty(request.Title))
                    {
                        Paragraph titleParagraph = new Paragraph(new Run(new Text(request.Title)))
                        {
                            ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center })
                        };
                        body.Append(titleParagraph);
                    }

                    foreach (var section in request.Content)
                    {
                        if (!string.IsNullOrEmpty(section.Text))
                        {
                            string altChunkId = "AltChunkId" + Guid.NewGuid().ToString("N"); //mora biti unique
                            AlternativeFormatImportPart altPart = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Rtf, altChunkId);

                            byte[] rtfBytes = Encoding.UTF8.GetBytes(section.Text); 
                            altPart.FeedData(new MemoryStream(rtfBytes)); 

                            AltChunk altChunk = new AltChunk { Id = altChunkId };
                            mainPart.Document.Body.AppendChild(altChunk);
                        }


                        if (!string.IsNullOrEmpty(section.Image))
                        {
                            try
                            {
                                byte[] imageBytes = Convert.FromBase64String(section.Image.Split(',').Last());

                                ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
                                using (var imgStream = new MemoryStream(imageBytes))
                                {
                                    imagePart.FeedData(imgStream);
                                }

                                AddImageToBody(body, mainPart.GetIdOfPart(imagePart), section.Image.Split(',').Last());
                            }
                            catch (Exception ex)
                            {
                                return BadRequest(new { error = $"Error processing image: {ex.Message}" });
                            }
                        }
                    }

                    mainPart.Document.Save();
                }

                string base64Doc = Convert.ToBase64String(memoryStream.ToArray());
                return Ok(new { docx = base64Doc });
            }
        }


        private void AddImageToBody(Body body, string relationshipId, string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            using (var ms = new MemoryStream(imageBytes))
            using (var image = System.Drawing.Image.FromStream(ms))
            {
                long imageWidth = image.Width;
                long imageHeight = image.Height;

                double aspectRatio = (double)imageWidth / imageHeight;
                long cx = 5736960L; 
                long cy = (long)(cx / aspectRatio);  

                if (cy > 8868960L)
                {
                    cy = 8868960L;
                    cx = (long)(cy * aspectRatio); 
                }

                var element = new Drawing(
                    new Inline(
                        new Extent() { Cx = cx, Cy = cy },
                        new EffectExtent() { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                        new DocProperties() { Id = (UInt32Value)1U, Name = "Inserted Image" },
                        new NonVisualGraphicFrameDrawingProperties(new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                        new DocumentFormat.OpenXml.Drawing.Graphic(
                            new DocumentFormat.OpenXml.Drawing.GraphicData(
                                new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties() { Id = 0, Name = "Picture" },
                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                        new DocumentFormat.OpenXml.Drawing.Blip() { Embed = relationshipId, CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.None },
                                        new DocumentFormat.OpenXml.Drawing.Stretch(new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                                    new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                        new DocumentFormat.OpenXml.Drawing.Transform2D(
                                            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                            new DocumentFormat.OpenXml.Drawing.Extents() { Cx = cx, Cy = cy }),
                                        new DocumentFormat.OpenXml.Drawing.PresetGeometry(new DocumentFormat.OpenXml.Drawing.AdjustValueList()) { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle })
                                )
                            )
                            { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                        )
                    )
                    { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U }
                );

                body.AppendChild(new Paragraph(new Run(element)));
            }
        }
    }
    public class DocRequest
    {
        public string Title { get; set; }
        public List<Section> Content { get; set; }
        public string? PreviousDocument { get; set; }
    }
    public class Section
    {
        public string Text { get; set; }
        public string? Image { get; set; }
    }
}
