const maxMatchSize = 0xff
const chunkSize = 0xfff
const minMatchSize = 8

export const toBytesUint16 = (num: number) =>
  new Uint8Array([num & 0x000000ff, (num & 0x0000ff00) >> 8])

const appendUint16 = (output: number[], num: number) => {
  output.push(num & 0x000000ff)
  output.push((num & 0x0000ff00) >> 8)
}

interface IMatch {
  sourcePosition: number
  referencePosition: number
  length: number
}

const findNextMatch = (
  source: Uint8Array,
  reference: Uint8Array,
  mask: boolean[]
): IMatch => {
  let sourcePosition = 0
  let referencePosition = 0
  let length = 0
  for (
    let sourceIndex = 0;
    sourceIndex + minMatchSize < source.length;
    sourceIndex++
  ) {
    if (mask[sourceIndex]) continue
    for (
      let referenceIndex = 0;
      referenceIndex + minMatchSize < reference.length;
      referenceIndex++
    ) {
      let thisLength = 0
      for (
        let windowIndex = 0;
        windowIndex < maxMatchSize &&
        sourceIndex + windowIndex < source.length &&
        referenceIndex + windowIndex < reference.length;
        windowIndex++
      ) {
        if (mask[sourceIndex + windowIndex]) break
        if (
          source[sourceIndex + windowIndex] !==
          reference[referenceIndex + windowIndex]
        )
          break
        thisLength += 1
      }
      if (thisLength > length) {
        sourcePosition = sourceIndex
        referencePosition = referenceIndex
        length = thisLength
      }
      if (thisLength >= maxMatchSize) {
        return {
          sourcePosition,
          referencePosition,
          length
        }
      }
    }
  }
  return {
    sourcePosition,
    referencePosition,
    length
  }
}

const maskMatch = (mask: boolean[], match: IMatch) => {
  for (let i = 0; i < match.length; i++) {
    mask[i + match.sourcePosition] = true
  }
}

const zeroArray = (length: number) => [...new Array(length)].map(() => 0)

const writeDiff = (reference: Uint8Array, source: Uint8Array) => {
  let output: number[] = []
  if (reference.length < source.length) {
    reference = new Uint8Array([
      ...reference,
      ...zeroArray(source.length - reference.length)
    ])
  }

  let chunkIndex = 0
  while (chunkIndex < source.length) {
    output = [
      ...output,
      ...generateChunk(
        reference.slice(
          chunkIndex,
          Math.min(chunkIndex + chunkSize, reference.length)
        ),
        source.slice(
          chunkIndex,
          Math.min(chunkIndex + chunkSize, source.length)
        )
      )
    ]
    chunkIndex += chunkSize
  }
  return new Uint8Array(output)
}

const generateChunk = (referenceChunk: Uint8Array, sourceChunk: Uint8Array) => {
  const mask = Array.from(sourceChunk).map(() => false)
  const matches: IMatch[] = []

  while (true) {
    const match = findNextMatch(sourceChunk, referenceChunk, mask)
    if (match.length < minMatchSize) break
    maskMatch(mask, match)
    matches.push(match)
  }
  const output: number[] = []
  appendUint16(output, matches.length)
  for (let match of matches) {
    output.push(match.length)
    appendUint16(output, match.sourcePosition)
    appendUint16(output, match.referencePosition)
  }

  let trailerSize = 0
  for (let i = 0; i < sourceChunk.length; i++) {
    if (mask[i] === false) {
      output.push(sourceChunk[i])
      trailerSize += 1
    }
  }

  return output
}

export default writeDiff
