const chunkSize = 0xfff

const toUInt16 = (values: Uint8Array) => {
  if (values.length !== 2) throw new Error(`expected 2 items, got ${values.length}`)
  return new Uint16Array(values.buffer)[0]
}

const falseArray = (length: number) => [...new Array(length)].map(() => false)

const readDiff = (diff: Uint8Array, reference: Uint8Array): Uint8Array => {
  let position = 0
  let chunkIndex = 0
  let output: number[] = []

  while (position < diff.length) {
    let chunk: number[] = []
    let matchCount = toUInt16(diff.slice(position, position + 2))
    position += 2
    let positionsFilled = 0
    const mask = falseArray(chunkSize)
    for (let i = 0; i < matchCount; i++) {
      const length = diff[position]
      position += 1
      const sourcePosition = toUInt16(diff.slice(position, position + 2))
      position += 2
      const referencePosition = toUInt16(diff.slice(position, position + 2))
      position += 2

      for (let j = 0; j < length; j++) {
        chunk[sourcePosition + j] = reference[(chunkIndex * chunkSize) + referencePosition + j] || 0
        mask[sourcePosition + j] = true
      }
      positionsFilled += length
    }

    const trailerSize = chunkSize - positionsFilled
    let trailersAdded = 0
    for (let i = 0; i < chunkSize && trailersAdded < trailerSize && position < diff.length; i++) {
      if (mask[i]) continue
      chunk[i] = diff[position]
      position += 1
      trailersAdded += 1
    }

    chunkIndex += 1
    output = [...output, ...chunk]
  }

  return new Uint8Array(output)

}


export default readDiff