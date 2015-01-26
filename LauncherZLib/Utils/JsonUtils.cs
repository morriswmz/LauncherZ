using System;
using System.IO;
using Newtonsoft.Json;

namespace LauncherZLib.Utils
{
    public static class JsonUtils
    {

        /// <summary>
        /// Deserialize a json object using file stream without suppressing exceptions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Path of the input json file.</param>
        public static T StreamDeserialize<T>(string path) where T : class
        {
            var serializer = new JsonSerializer();
            using (var sr = new StreamReader(path))
            {
                var jsonReader = new JsonTextReader(sr);
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        /// <summary>
        /// Attempts to deserialize a json object using file stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">Path of the input json file.</param>
        /// <param name="obj">Output object.</param>
        /// <returns>True if deserialization is successful and no exception occurs.</returns>
        /// <remarks>This method suppresses possible exceptions.</remarks>
        public static bool TryStreamDeserialize<T>(string path, out T obj) where T : class 
        {
            try
            {
                obj = StreamDeserialize<T>(path);
                return true;
            }
            catch (Exception)
            {
                obj = null;
                return false;
            }
        }

        /// <summary>
        /// Serialize a json object using file stream without suppressing exceptions.
        /// </summary>
        /// <param name="path">Path of the output json file.</param>
        /// <param name="obj">Input object.</param>
        /// <param name="formatting">Json formatting, indented or none.</param>
        public static void StreamSerialize(string path, object obj, Formatting formatting)
        {
            var serializer = new JsonSerializer { Formatting = formatting };
            using (var sw = new StreamWriter(path))
            {
                serializer.Serialize(sw, obj);
                sw.Flush();
            }
        }

        /// <summary>
        /// Attempts to serialize a json object using file stream.
        /// </summary>
        /// <param name="path">Path of the output json file.</param>
        /// <param name="obj">Input object.</param>
        /// <param name="formatting">Json formatting, indented or none.</param>
        /// <returns>True if serialization is successful and no exception occurs.</returns>
        /// <remarks>This method suppresses possible exceptions.</remarks>
        public static bool TryStreamSerialize(string path, object obj, Formatting formatting)
        {
            try
            {
                StreamSerialize(path, obj, formatting);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
