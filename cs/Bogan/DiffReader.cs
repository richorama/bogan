using System;
using System.Collections.Generic;
using System.Linq;

namespace Bogan
{
  public class DiffReader
  {
    const int chunkSize = 0xfff;
    public IEnumerable<byte> Read(List<byte> diff, List<byte> reference)
    {
      var position = 0;
      var chunkIndex = 0;
      var output = new List<byte>();
      var diffArray = diff.ToArray().Decompress();

      while (position < diffArray.Length)
      {
        var chunk = new List<byte>(chunkSize);
        var matchCount = BitConverter.ToUInt16(diffArray, position);
        position += 2;

        var positionsFilled = 0;
        var mask = Enumerable.Range(0, chunkSize).Select(_ => false).ToList();

        for (var i = 0; i < matchCount; i++)
        {
          var length = diffArray[position];
          position += 1;
          var sourcePosition = BitConverter.ToUInt16(diffArray, position);
          position += 2;
          var referencePosition = BitConverter.ToUInt16(diffArray, position);
          position += 2;

          for (var j = 0; j < length; j++)
          {
            chunk.Set(sourcePosition + j, reference.Get((chunkIndex * chunkSize) + referencePosition + j));
            mask[sourcePosition + j] = true;
          }
          positionsFilled += length;
        }

        var trailerSize = chunkSize - positionsFilled;
        var trailersAdded = 0;
        for (var i = 0; i < chunkSize && trailersAdded < trailerSize && position < diffArray.Length; i++)
        {
          if (mask[i]) continue;
          chunk.Set(i, diffArray[position]);
          position += 1;
          trailersAdded += 1;
        }

        chunkIndex += 1;
        foreach (var value in chunk)
        {
          yield return value;
        }

      }
    }
  }
}

