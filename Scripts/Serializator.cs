using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Remote
{
    class Serializator
    {
        
        public static byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);

                return stream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(buffer, 0, buffer.Length);
                stream.Position = 0;

                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}
