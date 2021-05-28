import fs from 'fs/promises'
import writeDiff from './write-diff'
import readDiff from './read-diff'
import expect from 'expect.js'

describe('DiffCreator', function () {
  it('calculates a diff', async function () {
    const reference = (await fs.readFile('./test-data/v22.bin')).slice(0, 800)
    const source = (await fs.readFile('./test-data/v24.bin')).slice(0, 800)

    const diff = writeDiff(reference, source)
    const source2 = readDiff(diff, reference)

    expect(source2.length).to.be(source.length)
    for (let i = 0; i < source.length; i++) {
      expect(source[i]).to.be(source2[i])
    }

    console.log(`${source.length} => ${diff.length} = ${100 * diff.length / source.length}%`)
  })


  it('calculates a diff swapped', async function () {
    const reference = (await fs.readFile('./test-data/v24.bin')).slice(0, 1000)
    const source = (await fs.readFile('./test-data/v22.bin')).slice(0, 1000)

    const diff = writeDiff(reference, source)
    const source2 = readDiff(diff, reference)

    expect(source2.length).to.be(source.length)
    for (let i = 0; i < source.length; i++) {
      expect(source[i]).to.be(source2[i])
    }

    console.log(`${source.length} => ${diff.length} = ${100 * diff.length / source.length}%`)
  })

  it('calculates a more than one chunk', async function () {
    const reference = (await fs.readFile('./test-data/v22.bin')).slice(0, 0xfff + 1000)
    const source = (await fs.readFile('./test-data/v24.bin')).slice(0, 0xfff + 1000)

    const diff = writeDiff(reference, source)
    const source2 = readDiff(diff, reference)

    expect(source2.length).to.be(source.length)
    for (let i = 0; i < source.length; i++) {
      expect(source[i]).to.be(source2[i])
    }

    console.log(`${source.length} => ${diff.length} = ${100 * diff.length / source.length}%`)
  })

  it('calculates with a smaller reference', async function () {
    const reference = (await fs.readFile('./test-data/v22.bin')).slice(0, 5000)
    const source = (await fs.readFile('./test-data/v24.bin')).slice(0, 8000)

    const diff = writeDiff(reference, source)
    const source2 = readDiff(diff, reference)

    expect(source2.length).to.be(source.length)
    for (let i = 0; i < source.length; i++) {
      expect(source[i]).to.be(source2[i])
    }

    console.log(`${source.length} => ${diff.length} = ${100 * diff.length / source.length}%`)
  })

  it('calculates with a larger reference', async function () {
    const reference = (await fs.readFile('./test-data/v22.bin')).slice(0, 8000)
    const source = (await fs.readFile('./test-data/v24.bin')).slice(0, 5000)

    const diff = writeDiff(reference, source)
    const source2 = readDiff(diff, reference)

    expect(source2.length).to.be(source.length)
    for (let i = 0; i < source.length; i++) {
      expect(source[i]).to.be(source2[i])
    }

    console.log(`${source.length} => ${diff.length} = ${100 * diff.length / source.length}%`)
  })
})