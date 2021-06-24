using System.Collections.Generic;

namespace Bogan
{
  static class Extensions
  {
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