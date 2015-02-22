// taken from https://github.com/jorgenpt/unity-utilities/blob/master/Serialization/PlayerPrefsSerializer.cs

using UnityEngine;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PvT.Util
{
    static public class Base64Serializer
    {
        static public string ToBase64<T>(T t)
        {
            // write state to memory.
            using (var memory = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memory, t);

                var bytes = memory.ToArray();
                var retval = System.Convert.ToBase64String(bytes);

                Debug.Log(string.Format("Base64Serializer converted {0} bytes to {1} Base64 bytes", bytes.Length, retval.Length));
                return retval;
            }
        }

        static public T FromBase64<T>(string base64)
        {
            T loaded = default(T);
            var bytes = System.Convert.FromBase64String(base64);

            using (var memory = new MemoryStream(bytes))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    loaded = (T)bf.Deserialize(memory);

                    Debug.Log(string.Format("Base64Serializer converted {0} Base64 bytes to {1} bytes", base64.Length, memory.Length));
                }
                catch
                {
                    Debug.LogWarning("Unable to deserialize data");
                }
            }
            return loaded;
        }
    }
}
