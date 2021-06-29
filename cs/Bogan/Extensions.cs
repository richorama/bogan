using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Bogan
{
  static class Extensions
  {

    public static byte[] Compress(this byte[] data)
    {
      var output = new MemoryStream();
      using (var dstream = new DeflateStream(output, CompressionLevel.Optimal))
      {
        dstream.Write(data, 0, data.Length);
      }
      return output.ToArray();
    }

    public static byte[] Decompress(this byte[] data)
    {
      var input = new MemoryStream(data);
      var output = new MemoryStream();
      using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
      {
        dstream.CopyTo(output);
      }
      return output.ToArray();
    }

    public static void AddUInt16(this List<byte> list, int value)
    {
      list.Add((byte)(value & 0x000000ff));
      list.Add((byte)((value & 0x0000ff00) >> 8));
    }

    public static void MaskMatch(this List<bool> mask, Match match)
    {
      for (var i = 0; i < match.Length; i++)
      {
        mask[i + match.SourcePosition] = true;
      }
    }

    public static void Set<T>(this List<T> list, int index, T value)
    {
      while (list.Count <= index)
      {
        list.Add(default(T));
      }
      list[index] = value;
    }


    public static T Get<T>(this List<T> list, int index)
    {
      if (index >= list.Count) return default(T);
      return list[index];
    }

  }

}