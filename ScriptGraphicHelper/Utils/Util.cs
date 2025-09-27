using Avalonia.Media;

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ScriptGraphicHelper.Utils
{
    public static class Util
    {
        public static int ToInt(this byte[] src, int offset = 0)
        {
            int value;
            value = (src[offset] & 0xFF) << 24
                    | (src[offset + 1] & 0xFF) << 16
                    | (src[offset + 2] & 0xFF) << 8
                    | src[offset + 3] & 0xFF;
            return value;
        }

        public static byte[] ToBytes(this int value)
        {
            var src = new byte[4];
            src[0] = (byte)(value >> 24 & 0xFF);
            src[1] = (byte)(value >> 16 & 0xFF);
            src[2] = (byte)(value >> 8 & 0xFF);
            src[3] = (byte)(value & 0xFF);
            return src;
        }
        public static int ToInt32(this byte[] value, int offset = 0)
        {
            int result;
            result = value[offset] << 24
                    | value[offset + 1] << 16
                    | value[offset + 2] << 8
                    | value[offset + 3];
            return result;
        }

        public static void WriteInt32(this Stream stream, int number)
        {
            stream.Write(number.ToBytes());
        }

        public static int ReadInt32(this Stream stream)
        {
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            return bytes.ToInt32();
        }

        public static List<string> GetLocalAddress()
        {
            var result = new List<string>();
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    result.Add(address.ToString().Trim());
                }
            }
            return result;
        }

        public static string ToHexString(this Color color)
        {
            return $"0x{color.R:x2}{color.G:x2}{color.B:x2}";
        }
    }
}
