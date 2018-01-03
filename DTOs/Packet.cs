using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DTOs
{
    [Serializable]
    public class Packet
    {
        public string Message { get; set; }
        public string SourceAddress { get; set; }
        public int SourcePort { get; set; }
        public string DestinationAdress { get; set; }
        public int DestinationPort { get; set; }
        public Stack<int> Labels { get; set; }

        public Packet()
        {
            Message = "Elo elo wiadomość kozak testowa domyślna";
            SourceAddress = "127.0.0.1";
            SourcePort = 14001;
            DestinationAdress = "127.0.0.1";
            DestinationPort = 14010;
            Labels = new Stack<int>();
        }

        public Packet(string msg, string srca, int srcp, string dsta, int dstp, int lab)
        {
            this.Message = msg;
            this.SourceAddress = srca;
            this.SourcePort = srcp;
            this.DestinationAdress = dsta;
            this.DestinationPort = dstp;
            this.Labels = new Stack<int>();
            this.Labels.Push(lab);
        }

        public static byte[] PacketToByte(Packet packet)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, packet);
            return ms.ToArray();
        }

        public static Packet ByteToPacket(byte[] data, int offset)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] newdata = new byte[data.Length - offset];
            Array.Copy(data, offset, newdata, 0, newdata.Length);
            ms.Write(newdata, 0, newdata.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (Packet)bf.Deserialize(ms);
        }
    }
}
