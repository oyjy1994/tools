using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyResProtect
{
    class BinaryRes
    {
        static public byte[] Read(string resPath)
        {
            byte[] buffer = null;
            try
            {
                FileStream stream = new FileStream(resPath, FileMode.Open);
                if (null != stream)
                {
                    BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                    buffer = reader.ReadBytes((int)stream.Length);
                    stream.Close();
                    stream.Dispose();
                    reader.Close();
                }
            }
            catch (Exception)
            {   
                throw;
            }

            return buffer;
        }

        static public void Write(byte[] buffer, string resPath)
        {
            try
            {
                FileStream stream = new FileStream(resPath, FileMode.Create);
                if (null != stream)
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(buffer, 0, buffer.Length);
                    stream.Close();
                    stream.Dispose();
                    writer.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
