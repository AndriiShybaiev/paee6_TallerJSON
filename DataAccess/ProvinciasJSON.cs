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
            string jsonString = File.ReadAllText(Path);
            try
            {
                List<Provincia>? provincias =
                    JsonConvert.DeserializeObject<List<Provincia>>(jsonString);
                return provincias;
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        public bool saveProvincias(List<Provincia> provincias)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(provincias);
                File.WriteAllText(Path, jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
