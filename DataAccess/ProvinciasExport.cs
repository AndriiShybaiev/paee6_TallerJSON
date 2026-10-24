using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace DataAccess
{
    public static class ProvinciasExport
    {
        private static readonly CultureInfo SpanishCulture = CultureInfo.GetCultureInfo("es-ES");

        public static bool SaveCSV(List<Provincia> provincias, string filePath)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("ID;Nombre;Poblacion;SuperficieKm2;EscudoPath;MapaPath\r\n");

                foreach (var p in provincias)
                {
                    string idStr = EscapeCsv(p.ID.ToString());
                    string nombreStr = EscapeCsv(SanitizeCsvFormula(p.Nombre));
                    string pobStr = p.Poblacion.HasValue ? EscapeCsv(p.Poblacion.Value.ToString(CultureInfo.InvariantCulture)) : "";
                    string supStr = p.SuperficieKm2.HasValue ? EscapeCsv(p.SuperficieKm2.Value.ToString("0.##", SpanishCulture)) : "";
                    string escudoStr = EscapeCsv(SanitizeCsvFormula(p.EscudoPath));
                    string mapaStr = EscapeCsv(SanitizeCsvFormula(p.MapaPath));

                    sb.Append($"{idStr};{nombreStr};{pobStr};{supStr};{escudoStr};{mapaStr}\r\n");
                }

                // UTF-8 with BOM
                File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string SanitizeCsvFormula(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string trimmed = input.TrimStart();
            if (trimmed.Length > 0)
            {
                char firstChar = trimmed[0];
                if (firstChar == '=' || firstChar == '+' || firstChar == '-' || firstChar == '@' ||
                    input.StartsWith("\t") || input.StartsWith("\r") || input.StartsWith("\n"))
                {
                    return "'" + input;
                }
            }

            return input;
        }

        private static string EscapeCsv(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        public static bool SaveXML(List<Provincia> provincias, string filePath)
        {
            try
            {
                var root = new XElement("Provincias");

                foreach (var p in provincias)
                {
                    var provElem = new XElement("Provincia",
                        new XElement("ID", p.ID),
                        new XElement("Nombre", p.Nombre ?? string.Empty),
                        new XElement("Poblacion", p.Poblacion.HasValue ? p.Poblacion.Value.ToString(CultureInfo.InvariantCulture) : null),
                        new XElement("SuperficieKm2", p.SuperficieKm2.HasValue ? p.SuperficieKm2.Value.ToString("0.##", CultureInfo.InvariantCulture) : null),
                        new XElement("EscudoPath", string.IsNullOrEmpty(p.EscudoPath) ? null : p.EscudoPath),
                        new XElement("MapaPath", string.IsNullOrEmpty(p.MapaPath) ? null : p.MapaPath)
                    );

                    root.Add(provElem);
                }

                var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);

                var settings = new System.Xml.XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = "\r\n",
                    OmitXmlDeclaration = false
                };

                using (var writer = System.Xml.XmlWriter.Create(filePath, settings))
                {
                    doc.Save(writer);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
