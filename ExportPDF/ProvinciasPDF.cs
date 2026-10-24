using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DataAccess;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace ExportPDF
{
    public class ProvinciasPDF
    {
        private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");

        static ProvinciasPDF()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public bool SavePDF(List<Provincia> provincias, string path, string? scopeDescription = null)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                Document document = new Document();
                document.Info.Title = "Provincias de España";

                Section section = document.AddSection();
                section.PageSetup.Orientation = Orientation.Landscape;
                section.PageSetup.TopMargin = "1.5cm";
                section.PageSetup.BottomMargin = "1.5cm";
                section.PageSetup.LeftMargin = "1.5cm";
                section.PageSetup.RightMargin = "1.5cm";

                // Footer
                Paragraph footer = section.Footers.Primary.AddParagraph();
                footer.Format.Alignment = ParagraphAlignment.Right;
                footer.Format.Font.Size = 9;
                footer.Format.Font.Color = Colors.DimGray;
                footer.AddText("Página ");
                footer.AddPageField();
                footer.AddText(" de ");
                footer.AddNumPagesField();

                // Header title
                Paragraph titleParagraph = section.AddParagraph();
                titleParagraph.AddText("Provincias de España");
                titleParagraph.Format.Font.Size = 18;
                titleParagraph.Format.Font.Bold = true;
                titleParagraph.Format.Font.Color = new Color(30, 58, 138); // Dark Blue
                titleParagraph.Format.Alignment = ParagraphAlignment.Left;
                titleParagraph.Format.SpaceAfter = "4pt";

                // Subtitle metadata
                Paragraph subParagraph = section.AddParagraph();
                subParagraph.Format.Font.Size = 10;
                subParagraph.Format.Font.Color = Colors.DarkSlateGray;
                subParagraph.Format.SpaceAfter = "12pt";

                string scopeText = string.IsNullOrWhiteSpace(scopeDescription) ? "Todas las provincias" : scopeDescription;
                subParagraph.AddText($"Ámbito: {scopeText}  |  Total registros: {provincias.Count}  |  Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");

                // Table
                Table table = section.AddTable();
                table.Borders.Color = new Color(229, 231, 235);
                table.Borders.Width = 0.5;

                // Columns: 1.5 + 7.5 + 4.5 + 4.5 + 3.5 = 21.5cm
                Column colId = table.AddColumn("1.5cm");
                colId.Format.Alignment = ParagraphAlignment.Center;

                Column colNombre = table.AddColumn("8.0cm");
                colNombre.Format.Alignment = ParagraphAlignment.Left;

                Column colPob = table.AddColumn("4.5cm");
                colPob.Format.Alignment = ParagraphAlignment.Right;

                Column colSup = table.AddColumn("4.5cm");
                colSup.Format.Alignment = ParagraphAlignment.Right;

                Column colEscudo = table.AddColumn("3.5cm");
                colEscudo.Format.Alignment = ParagraphAlignment.Center;

                // Header Row
                Row headerRow = table.AddRow();
                headerRow.HeadingFormat = true;
                headerRow.Format.Font.Bold = true;
                headerRow.Format.Font.Size = 10;
                headerRow.Format.Font.Color = Colors.White;
                headerRow.Shading.Color = new Color(37, 99, 235); // Blue header
                headerRow.Height = "0.7cm";
                headerRow.HeightRule = RowHeightRule.AtLeast;

                headerRow.Cells[0].AddParagraph("ID").Format.Alignment = ParagraphAlignment.Center;
                headerRow.Cells[1].AddParagraph("Provincia").Format.Alignment = ParagraphAlignment.Left;
                headerRow.Cells[2].AddParagraph("Población").Format.Alignment = ParagraphAlignment.Right;
                headerRow.Cells[3].AddParagraph("Superficie (km²)").Format.Alignment = ParagraphAlignment.Right;
                headerRow.Cells[4].AddParagraph("Escudo").Format.Alignment = ParagraphAlignment.Center;

                // Data Rows
                Color altBg = new Color(248, 250, 252);
                Color whiteBg = Colors.White;

                for (int i = 0; i < provincias.Count; i++)
                {
                    Provincia p = provincias[i];
                    Row row = table.AddRow();
                    row.Height = "0.8cm";
                    row.HeightRule = RowHeightRule.AtLeast;
                    row.Shading.Color = (i % 2 == 1) ? altBg : whiteBg;
                    row.VerticalAlignment = VerticalAlignment.Center;

                    // ID
                    row.Cells[0].AddParagraph(p.ID.ToString()).Format.Alignment = ParagraphAlignment.Center;

                    // Nombre
                    row.Cells[1].AddParagraph(p.Nombre ?? string.Empty).Format.Alignment = ParagraphAlignment.Left;

                    // Población
                    string pobText = p.Poblacion.HasValue ? p.Poblacion.Value.ToString("N0", SpanishCulture) : "—";
                    row.Cells[2].AddParagraph(pobText).Format.Alignment = ParagraphAlignment.Right;

                    // Superficie
                    string supText = p.SuperficieKm2.HasValue ? p.SuperficieKm2.Value.ToString("N2", SpanishCulture) : "—";
                    row.Cells[3].AddParagraph(supText).Format.Alignment = ParagraphAlignment.Right;

                    // Escudo
                    Paragraph escudoPara = row.Cells[4].AddParagraph();
                    escudoPara.Format.Alignment = ParagraphAlignment.Center;

                    bool imageAdded = false;
                    if (!string.IsNullOrWhiteSpace(p.EscudoPath) && File.Exists(p.EscudoPath) && IsValidImageFile(p.EscudoPath))
                    {
                        try
                        {
                            Image img = escudoPara.AddImage(p.EscudoPath);
                            img.Width = "1.0cm";
                            img.LockAspectRatio = true;
                            imageAdded = true;
                        }
                        catch
                        {
                            imageAdded = false;
                        }
                    }

                    if (!imageAdded)
                    {
                        var text = escudoPara.AddFormattedText("Sin imagen");
                        text.Color = Colors.Gray;
                        text.Italic = true;
                        text.Size = 8;
                    }
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

        private static bool IsValidImageFile(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(
                    stream,
                    System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,
                    System.Windows.Media.Imaging.BitmapCacheOption.None);
                return decoder.Frames.Count > 0 && decoder.Frames[0].PixelWidth > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
