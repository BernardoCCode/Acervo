export type Chapter = {
  id: string
  title: string
  /** Index in the flat paragraphs array where this chapter's heading lives (or first para). */
  headingParagraphIndex: number
  /** Paragraph indices belonging to this chapter (excluding the heading line when separate). */
  paragraphIndices: number[]
  startOffset: number
  endOffset: number
}

const KNOWN_HEADINGS =
  /^(abstract|introduction|background|methods?|materials?|results?|discussion|conclusion|conclusions|references|acknowledgements?|appendix|related work|literature review|experimental|keywords?|resumo|introdu[cç][aã]o|metodologia|m[eé]todos?|materiais?|resultados?|discuss[aã]o|conclus[aã]o|conclus[oõ]es|refer[eê]ncias|agradecimentos?|anexo|palavras[- ]chave|considera[cç][oõ]es finais)\b/i

function isKnownHeading(text: string) {
  return KNOWN_HEADINGS.test(text.trim())
}

export function looksLikeHeading(paragraph: string): boolean {
  const t = paragraph.replace(/\s+/g, ' ').trim()
  if (t.length < 2 || t.length > 100) return false
  if (/[.!?]$/.test(t) && t.length > 40) return false

  if (/^\d+(\.\d+){0,3}\.?\s+\S/.test(t)) return true
  if (/^[IVXLC]{1,6}\.\s+\S/i.test(t)) return true
  if (isKnownHeading(t)) return true

  const words = t.split(' ').filter(Boolean)
  if (words.length === 0 || words.length > 12) return false

  const lettersOnly = t.replace(/[^A-Za-zÀ-ÿ]/g, '')
  if (
    lettersOnly.length >= 4 &&
    t === t.toUpperCase() &&
    /[A-ZÁÉÍÓÚÀÂÊÔÃÕ]/.test(t)
  ) {
    return true
  }

  // Short title-case line without sentence punctuation
  if (!/[.!?:,;]$/.test(t) && words.length <= 8) {
    const capitalized = words.filter((w) => /^[A-ZÁÉÍÓÚÀÂÊÔÃÕ0-9]/.test(w)).length
    if (capitalized / words.length >= 0.7 && t.length <= 70) return true
  }

  return false
}

function paragraphOffsets(paragraphs: string[]): number[] {
  const offsets: number[] = []
  let cursor = 0
  for (let i = 0; i < paragraphs.length; i++) {
    offsets.push(cursor)
    cursor += paragraphs[i].length
    if (i < paragraphs.length - 1) cursor += 2 // \n\n join used in body
  }
  return offsets
}

/**
 * Split readable paragraphs into navigable chapters.
 * Falls back to synthetic chunks when the paper has no clear headings.
 */
export function buildChapters(paragraphs: string[], body?: string): Chapter[] {
  if (!paragraphs.length) return []

  const offsets = paragraphOffsets(paragraphs)
  const bodyLength = body?.length ?? offsets.at(-1)! + paragraphs.at(-1)!.length

  const headingIndexes: number[] = []
  paragraphs.forEach((p, i) => {
    if (looksLikeHeading(p)) headingIndexes.push(i)
  })

  // Need at least 2 real headings, or one early known heading, to trust structure
  const usableHeadings =
    headingIndexes.length >= 2 ||
    (headingIndexes.length === 1 &&
      headingIndexes[0] <= 3 &&
      isKnownHeading(paragraphs[headingIndexes[0]]))

  if (usableHeadings) {
    const starts = headingIndexes[0] === 0 ? headingIndexes : [0, ...headingIndexes]
    const chapters: Chapter[] = []

    for (let i = 0; i < starts.length; i++) {
      const start = starts[i]
      const end = i + 1 < starts.length ? starts[i + 1] : paragraphs.length
      const isHeading = looksLikeHeading(paragraphs[start])
      const title = isHeading
        ? paragraphs[start].replace(/\s+/g, ' ').trim()
        : i === 0
          ? 'Início'
          : `Seção ${i + 1}`

      const contentStart = isHeading ? start + 1 : start
      const indices: number[] = []
      for (let p = contentStart; p < end; p++) indices.push(p)
      // Keep heading-only sections visible
      if (indices.length === 0 && isHeading) indices.push(start)

      const startOffset = offsets[start] ?? 0
      const endOffset =
        end < paragraphs.length
          ? offsets[end]!
          : bodyLength

      chapters.push({
        id: `ch-${i}-${start}`,
        title,
        headingParagraphIndex: start,
        paragraphIndices: indices.length ? indices : [start],
        startOffset,
        endOffset: Math.max(endOffset, startOffset),
      })
    }

    return chapters.filter((c) => c.paragraphIndices.length > 0)
  }

  // Synthetic chapters every ~8–12 paragraphs for long papers
  const chunkSize = paragraphs.length > 40 ? 10 : paragraphs.length > 20 ? 8 : paragraphs.length
  if (paragraphs.length <= chunkSize) {
    return [
      {
        id: 'ch-0',
        title: 'Texto completo',
        headingParagraphIndex: 0,
        paragraphIndices: paragraphs.map((_, i) => i),
        startOffset: 0,
        endOffset: bodyLength,
      },
    ]
  }

  const chapters: Chapter[] = []
  for (let i = 0, n = 0; i < paragraphs.length; i += chunkSize, n++) {
    const end = Math.min(i + chunkSize, paragraphs.length)
    const indices = Array.from({ length: end - i }, (_, k) => i + k)
    chapters.push({
      id: `ch-syn-${n}`,
      title: `Parte ${n + 1}`,
      headingParagraphIndex: i,
      paragraphIndices: indices,
      startOffset: offsets[i] ?? 0,
      endOffset: end < paragraphs.length ? offsets[end]! : bodyLength,
    })
  }
  return chapters
}
