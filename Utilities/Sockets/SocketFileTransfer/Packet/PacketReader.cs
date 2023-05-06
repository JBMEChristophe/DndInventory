using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Sockets.SocketFileTransfer.Packet
{
    public class PacketReader : BinaryReader
    {
        private BinaryFormatter _bf;
        public PacketReader(byte[] data)
            :base(new MemoryStream(data))
        {
            _bf = new BinaryFormatter();
        }

        public Image ReadImage()
        {
            int len = ReadInt32();
            byte[] bytes = ReadBytes(len);

            Image img;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                img = Image.FromStream(ms);
            }

            return img;
        }

        public T ReadObject<T>()
        {
            return (T)_bf.Deserialize(BaseStream);
        }
    }
}
