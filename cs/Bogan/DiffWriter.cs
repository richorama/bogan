using System;
using System.Collections.Generic;
using System.Linq;

namespace Bogan
{
  public struct Match
  {
    public Match(int sourcePosition, int referencePosition, int length)
    {
      this.SourcePosition = sourcePosition;
      this.ReferencePosition = referencePosition;
      this.Length = length;
    }

    public int SourcePosition { get; set; }
    public int ReferencePosition { get; set; }
    public int Length { get; set; }
  }

  public static class Extensions
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

  }


  public class DiffWriter
  {
    const int chunkSize = 0xfff;
    const int maxMatchSize = 0xff;
    const int minMatchSize = 8;


    Match FindNextMatch(List<byte> reference, List<byte> source, List<bool> mask)
    {
      var sourcePosition = 0;
      var referencePosition = 0;
      var length = 0;
      for (var sourceIndex = 0; sourceIndex < source.Count; sourceIndex++)
      {
        if (mask[sourceIndex]) continue;
        for (var referenceIndex = 0; referenceIndex < reference.Count; referenceIndex++)
        {
          var thisLength = 0;
          for (var windowIndex = 0; windowIndex < maxMatchSize && sourceIndex + windowIndex < source.Count && referenceIndex + windowIndex < reference.Count; windowIndex++)
          {
            if (mask[sourceIndex + windowIndex]) break;
            if (source[sourceIndex + windowIndex] != reference[referenceIndex + windowIndex]) break;
            thisLength += 1;
          }
          if (thisLength > length)
          {
            sourcePosition = sourceIndex;
            referencePosition = referenceIndex;
            length = thisLength;
          }
          if (thisLength >= maxMatchSize)
          {
            return new Match(sourcePosition, referencePosition, length);
          }
        }
      }
      return new Match(sourcePosition, referencePosition, length);
    }

    IEnumerable<byte> GenerateChunk(List<byte> reference, List<byte> source)
    {
      var mask = source.Select(_ => false).ToList();
      var matches = new List<Match>();
      while (true)
      {
        var match = this.FindNextMatch(source, reference, mask);
        if (match.Length < minMatchSize) break;
        mask.MaskMatch(match);
        matches.Add(match);
      }

      var output = new List<byte>();
      output.AddUInt16(matches.Count);

      foreach (var match in matches)
      {
        output.Add((byte)match.Length);
        output.AddUInt16(match.SourcePosition);
        output.AddUInt16(match.ReferencePosition);
      }

      var trailerSize = 0;
      for (var i = 0; i < source.Count; i++)
      {
        if (mask[i] == false)
        {
          output.Add(source[i]);
          trailerSize += 1;
        }
      }

      return output;
    }


    public IEnumerable<byte> Generate(List<byte> reference, List<byte> source)
    {
      var refBytes = new List<byte>();
      refBytes.AddRange(reference);
      while (refBytes.Count < source.Count)
      {
        refBytes.Add(0);
      }

      var chunkIndex = 0;
      while (chunkIndex < source.Count)
      {
        var output = GenerateChunk(
          refBytes.GetRange(chunkIndex, Math.Min(chunkSize, refBytes.Count - chunkIndex)),
          source.GetRange(chunkIndex, Math.Min(chunkSize, source.Count - chunkIndex)));

        foreach (var b in output)
        {
          yield return b;
        }
        chunkIndex += chunkSize;
      }
    }

  }
}

