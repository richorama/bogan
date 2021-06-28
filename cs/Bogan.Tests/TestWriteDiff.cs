using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bogan.Tests
{
  [TestClass]
  public class TestWriteDiff
  {


    [TestMethod]
    public void TestBaseGenerate()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(800).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void TestBaseGenerateSwapped()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(1000).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(1000).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void TestBaseGenerateWithMoreThanOneChunk()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(0xfff + 1000).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(0xfff + 1000).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void TestBaseGenerateWithASmallerReference()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(3000).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(4000).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void TestBaseGenerateWithALargerReference()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(8000).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(5000).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void WriteDiff()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(800).ToList();

      var diff = new DiffWriter().Generate(reference, source);

      File.WriteAllBytes("../../../../../test-data/22 to 24 cs.bin", diff.ToArray());
    }

    [TestMethod]
    public void CheckAgainstPersistedDiff()
    {

      var diff = File.ReadAllBytes("../../../../../test-data/22 to 24 node.bin");
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();

      var regeneratedSource = new DiffReader().Read(diff.ToList(), reference).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").Take(800).ToList();
      
      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

    [TestMethod]
    public void GenerateEntireFile()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v24.bin").ToList();

      new DiffWriter().Generate(reference, source).ToList();
    }



    [TestMethod]
    public void TestIdenticalVersions()
    {
      var reference = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();
      var source = File.ReadAllBytes("../../../../../test-data/v22.bin").Take(800).ToList();

      var diff = new DiffWriter().Generate(reference, source).ToList();

      var regeneratedSource = new DiffReader().Read(diff, reference).ToList();

      Assert.AreEqual(source.Count, regeneratedSource.Count);
      for (var i = 0; i < source.Count; i++)
      {
        Assert.AreEqual(source[i], regeneratedSource[i], $"byte {i} does not match");
      }
    }

  }
}
