using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bogan.Tests
{
  [TestClass]
  public class TestWriteDiff
  {

    [TestMethod]
    public void TestUInt16()
    {
      var n = new List<byte>();
      n.AddUInt16(900);
      var value = BitConverter.ToUInt16(n.ToArray(), 0);
      Assert.AreEqual(900, value);
    }


    [TestMethod]
    public void TestBaseGenerate()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(800).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();
      
      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i =0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }

    }
  }
}
