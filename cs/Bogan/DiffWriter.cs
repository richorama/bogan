﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bogan
{
  struct Match
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

 
  public class DiffWriter
  {
    const int chunkSize = 0xfff;
    const int maxMatchSize = 0xff;
    const int minMatchSize = 8;


    Match FindNextMatch(List<byte> source, List<byte> reference, List<bool> mask)
    {
      if (source.Count != mask.Count) throw new ArgumentException($"{source.Count}, {mask.Count}");

      var sourcePosition = 0;
      var referencePosition = 0;
      var length = 0;
      for (var sourceIndex = 0; sourceIndex + minMatchSize < source.Count; sourceIndex++)
      {
        if (mask[sourceIndex]) continue;
        for (var referenceIndex = 0; referenceIndex + minMatchSize < reference.Count; referenceIndex++)
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

    List<byte> GenerateChunk(List<byte> reference, List<byte> source)
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
      var missingBytes = Enumerable.Range(0, Math.Max(0, source.Count - refBytes.Count)).Select(_ => (byte)0);
      refBytes.AddRange(missingBytes);

      var chunkIndex = 0;
      var tasks = new List<Task<List<byte>>>();
      var factory = new TaskFactory();
      while (chunkIndex < source.Count)
      {
        // keep a scoped copy of this variable
        var refChunk = refBytes.GetRange(chunkIndex,  Math.Min(chunkSize, refBytes.Count - chunkIndex));
        var sourceChunk = source.GetRange(chunkIndex, Math.Min(chunkSize, source.Count - chunkIndex));
        var task = factory.StartNew(() => GenerateChunk(refChunk, sourceChunk));
        tasks.Add(task);

        chunkIndex += chunkSize;
      }
      Task.WhenAll(tasks).Wait();

      var output = new List<byte>();

      foreach (var task in tasks)
      {
        output.AddRange(task.Result);
      }


      var compressed = output.ToArray().Compress();

      Console.WriteLine($"{output.Count} => {compressed.Length}");

      return compressed;

    }

  }
}

