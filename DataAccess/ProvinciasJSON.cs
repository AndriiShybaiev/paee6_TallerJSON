using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataAccess
{
    public class ProvinciasJSON
    {
        public string Path { get; set; }

        public ProvinciasJSON(string path)
        {
            Path = path;
        }

        public List<Provincia>? getProvincias()
        {
            try
            {
                string jsonString = File.ReadAllText(Path);
                List<Provincia>? provincias =
                    JsonConvert.DeserializeObject<List<Provincia>>(jsonString);

                if (provincias != null)
                {
                    string? baseDir = System.IO.Path.GetDirectoryName(Path);
                    if (string.IsNullOrEmpty(baseDir))
                    {
                        baseDir = Directory.GetCurrentDirectory();
                    }

                    foreach (var p in provincias)
                    {
                        if (!string.IsNullOrWhiteSpace(p.EscudoPath))
                        {
                            p.EscudoPath = ResolveAbsolutePath(baseDir, p.EscudoPath);
                        }

                        if (!string.IsNullOrWhiteSpace(p.MapaPath))
                        {
                            p.MapaPath = ResolveAbsolutePath(baseDir, p.MapaPath);
                        }
                    }
                }

                return provincias;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string? ResolveAbsolutePath(string baseDir, string relativeOrAbsolutePath)
        {
            try
            {
                if (System.IO.Path.IsPathRooted(relativeOrAbsolutePath))
                {
                    return System.IO.Path.GetFullPath(relativeOrAbsolutePath);
                }

                string combined = System.IO.Path.Combine(baseDir, relativeOrAbsolutePath);
                return System.IO.Path.GetFullPath(combined);
            }
            catch
            {
                return null;
            }
        }

        public bool saveProvincias(List<Provincia> provincias, string? targetPath = null)
        {
            string destinationPath = string.IsNullOrWhiteSpace(targetPath) ? Path : targetPath;

            try
            {
                string? targetDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(destinationPath));
                if (string.IsNullOrEmpty(targetDir))
                {
                    targetDir = Directory.GetCurrentDirectory();
                }

                Directory.CreateDirectory(targetDir);

                string baseName = System.IO.Path.GetFileNameWithoutExtension(destinationPath);
                string assetsFolderName = baseName + "_assets";
                string assetsFolderPath = System.IO.Path.Combine(targetDir, assetsFolderName);

                var serializableList = new List<Provincia>();
                var updatedEscudoPaths = new Dictionary<Provincia, string?>();
                var updatedMapaPaths = new Dictionary<Provincia, string?>();

                foreach (var p in provincias)
                {
                    var copy = new Provincia
                    {
                        ID = p.ID,
                        Nombre = p.Nombre,
                        Poblacion = p.Poblacion,
                        SuperficieKm2 = p.SuperficieKm2,
                        EscudoPath = null,
                        MapaPath = null
                    };

                    // Process Escudo
                    if (!string.IsNullOrWhiteSpace(p.EscudoPath) && File.Exists(p.EscudoPath))
                    {
                        Directory.CreateDirectory(assetsFolderPath);
                        string ext = System.IO.Path.GetExtension(p.EscudoPath);
                        string fileName = $"{p.ID}_{Guid.NewGuid():N}_escudo{ext}";
                        string destFile = System.IO.Path.Combine(assetsFolderPath, fileName);

                        if (!string.Equals(System.IO.Path.GetFullPath(p.EscudoPath), System.IO.Path.GetFullPath(destFile), StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(p.EscudoPath, destFile, true);
                        }

                        copy.EscudoPath = $"{assetsFolderName}/{fileName}";
                        updatedEscudoPaths[p] = destFile;
                    }
                    else
                    {
                        copy.EscudoPath = null;
                        updatedEscudoPaths[p] = null;
                    }

                    // Process Mapa
                    if (!string.IsNullOrWhiteSpace(p.MapaPath) && File.Exists(p.MapaPath))
                    {
                        Directory.CreateDirectory(assetsFolderPath);
                        string ext = System.IO.Path.GetExtension(p.MapaPath);
                        string fileName = $"{p.ID}_{Guid.NewGuid():N}_mapa{ext}";
                        string destFile = System.IO.Path.Combine(assetsFolderPath, fileName);

                        if (!string.Equals(System.IO.Path.GetFullPath(p.MapaPath), System.IO.Path.GetFullPath(destFile), StringComparison.OrdinalIgnoreCase))
                        {
                            File.Copy(p.MapaPath, destFile, true);
                        }

                        copy.MapaPath = $"{assetsFolderName}/{fileName}";
                        updatedMapaPaths[p] = destFile;
                    }
                    else
                    {
                        copy.MapaPath = null;
                        updatedMapaPaths[p] = null;
                    }

                    serializableList.Add(copy);
                }

                // Write JSON atomically
                string tempFile = destinationPath + ".tmp";
                string jsonString = JsonConvert.SerializeObject(serializableList, Formatting.Indented);
                File.WriteAllText(tempFile, jsonString);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                File.Move(tempFile, destinationPath);

                // Update in-memory models and current Path ONLY on success
                Path = destinationPath;
                foreach (var p in provincias)
                {
                    if (updatedEscudoPaths.TryGetValue(p, out string? newEscudo))
                    {
                        p.EscudoPath = newEscudo;
                    }
                    if (updatedMapaPaths.TryGetValue(p, out string? newMapa))
                    {
                        p.MapaPath = newMapa;
                    }
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
