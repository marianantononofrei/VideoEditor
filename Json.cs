using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor
{
    public class Json
    {
        /// <summary>
        /// Load content from file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>"" if filePath is empty, not valid or not found</returns>
        public static string LoadFromFile(string filePath)
        {
            string content = "";

            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception)
            {
            }

            return content;
        }

        /// <summary>
        /// Load content from file 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <returns>null if filename is empty, not valid or not faund and deserialized object otherwise</returns>
        public static T LoadFromFile<T>(string filename)
        {
            string content = File.ReadAllText(filename);

            return JsonConvert.DeserializeObject<T>(content);
        }

        /// <summary>
        /// Write entities in filename
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="filename"></param>
        public static void WriteToFile(object entities, string filename)
        {
            string content = JsonConvert.SerializeObject(entities);
            File.WriteAllText(filename, content);
        }
    }
}
