using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Sockets.SocketFileTransfer.Packet
{
    public class PacketWriter : BinaryWriter
    {
        private MemoryStream _ms;
        private BinaryFormatter _bf;

        public PacketWriter()
            :base()
        {
            _ms = new MemoryStream();
            _bf = new BinaryFormatter();
            OutStream = _ms;
        }

        public void Write(Image image)
        {
            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);

            ms.Close();

            byte[] imageBytes = ms.ToArray();

            Write(imageBytes.Length);
            Write(imageBytes);
        }

        public void WriteT(object obj)
        {
            _bf.Serialize(_ms, obj);
        }

        public byte[] GetBytes()
        {
            Close();

            byte[] data = _ms.ToArray();
            return data;
        }
    }
}
