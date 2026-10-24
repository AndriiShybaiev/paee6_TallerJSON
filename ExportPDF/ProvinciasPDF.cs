using System;
using System.Collections.Generic;
using System.Text;
using DataAccess;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace ExportPDF
{
    public class ProvinciasPDF
    {
        static ProvinciasPDF()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public bool SavePDF(List<Provincia> provincias, string path)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                Document document = new Document();
                Section section = document.AddSection();
                Paragraph paragraph = section.AddParagraph();
                paragraph.AddText("Provincias");
                paragraph.Format.Font.Size = 20;
                paragraph.Format.Font.Bold = true;
                paragraph.Format.Alignment = ParagraphAlignment.Center;

                section.AddParagraph();

                Table table = section.AddTable();
                table.Style = "Table";

                Column column = table.AddColumn("2cm");
                column.Format.Alignment = ParagraphAlignment.Center;
                column = table.AddColumn("10cm");
                column.Format.Alignment = ParagraphAlignment.Left;

                Row row = table.AddRow();
                row.HeadingFormat = true;
                row.Format.Font.Bold = true;
                row.Shading.Color = Colors.LightBlue;
                row.Cells[0].AddParagraph("ID").Format.Alignment = ParagraphAlignment.Center;
                row.Cells[1].AddParagraph("Provincia");

                foreach (Provincia p in provincias)
                {
                    row = table.AddRow();
                    row.Cells[0].AddParagraph(p.ID.ToString()).Format.Alignment =
                        ParagraphAlignment.Center;
                    row.Cells[1].AddParagraph(p.Nombre);
                }

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer
                {
                    Document = document,
                    PdfDocument = new PdfDocument()
                };
                pdfRenderer.RenderDocument();
                pdfRenderer.PdfDocument.Save(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
