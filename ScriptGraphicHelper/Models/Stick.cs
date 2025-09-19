using System;
using System.IO;
using System.Threading.Tasks;

namespace ScriptGraphicHelper.Models;

public static class Stick
{
    public static byte[] MakePackData(string key, byte[] buffer)
    {
        var pack = new PackData
        {
            Key = key,
            Buffer = buffer
        };
        var data = pack.ToBytes();
        return data;
    }

    public static byte[] MakePackData(string key, string desc, byte[] buffer)
    {
        var pack = new PackData
        {
            Key = key,
            Description = desc,
            Buffer = buffer
        };
        var data = pack.ToBytes();
        return data;
    }

    public static byte[] MakePackData(string key, string desc)
    {
        var pack = new PackData
        {
            Key = key,
            Description = desc
        };
        var data = pack.ToBytes();
        return data;
    }

    public static byte[] MakePackData(string key)
    {
        var pack = new PackData
        {
            Key = key
        };
        var data = pack.ToBytes();
        return data;
    }

    public static async Task<PackData?> ReadPackAsync(Stream stream)
    {
        try
        {
            var header = new byte[4];
            var offset = 0;
            while (offset < 4)
            {
                offset += await stream.ReadAsync(header.AsMemory(offset, 4 - offset));
            }

            var len = header.ToInt32();
            var data = new byte[len];

            offset = 0;
            while (offset < len)
            {
                offset += await stream.ReadAsync(data.AsMemory(offset, len - offset));
            }

            var result = PackData.Parse_Old(data);
            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
