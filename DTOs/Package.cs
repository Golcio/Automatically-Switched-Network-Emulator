using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DTOs
{
    [Serializable]
    public class Package
    {
        public Queue<byte[]> packageQueue;

        public Package()
        {
            packageQueue = new Queue<byte[]>();
        }

        public Package(Queue <byte[]> packageQueue)
        {
            this.packageQueue = packageQueue;
        }

        public static byte[] PackageToByte(Package packet)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, packet);
            return ms.ToArray();
        }

        public static Package ByteToPackage(byte[] data, int offset)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] newdata = new byte[data.Length - offset];
            Array.Copy(data, offset, newdata, 0, newdata.Length);
            ms.Write(newdata, 0, newdata.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (Package)bf.Deserialize(ms);
        }
    }
}
