import {
  useEffect,
  useMemo,
  useState,
  type ReactNode,
  type RefObject,
} from 'react'
import { buildChapters, looksLikeHeading } from '../lib/chapters'
import type { Highlight, ReadableContent } from '../lib/types'
import { Spinner } from './ui'

type Props = {
  content: ReadableContent | null
  loading: boolean
  fontSize: number
  bodyRef: RefObject<HTMLDivElement | null>
  highlights: Highlight[]
  activeChapterId: string | null
  onActiveChapterChange: (id: string) => void
  onChaptersChange?: (chapters: { id: string; title: string }[]) => void
  readingMode?: boolean
}

function renderHighlightedSlice(
  slice: string,
  sliceStart: number,
  highlights: Highlight[],
): ReactNode {
  const relevant = highlights
    .map((h) => ({
      ...h,
      localStart: Math.max(0, h.startOffset - sliceStart),
      localEnd: Math.min(slice.length, h.endOffset - sliceStart),
    }))
    .filter((h) => h.localEnd > h.localStart)
    .sort((a, b) => a.localStart - b.localStart || a.localEnd - b.localEnd)

  if (!relevant.length) return slice

  // Clip overlaps so a second highlight never reprints characters already painted.
  const parts: ReactNode[] = []
  let cursor = 0
  relevant.forEach((h, i) => {
    const start = Math.max(h.localStart, cursor)
    const end = h.localEnd
    if (end <= start) return
    if (start > cursor) parts.push(slice.slice(cursor, start))
    parts.push(
      <mark
        key={h.id || i}
        className="reader-mark rounded-[3px] bg-[var(--highlight)] px-0.5 text-ink"
      >
        {slice.slice(start, end)}
      </mark>,
    )
    cursor = end
  })
  if (cursor < slice.length) parts.push(slice.slice(cursor))
  return parts
}

function paragraphOffset(paragraphs: string[], index: number): number {
  let cursor = 0
  for (let i = 0; i < index; i++) {
    cursor += paragraphs[i].length
    if (i < paragraphs.length - 1) cursor += 2
  }
  return cursor
}

export function TextReader({
  content,
  loading,
  fontSize,
  bodyRef,
  highlights,
  activeChapterId,
  onActiveChapterChange,
  onChaptersChange,
  readingMode = false,
}: Props) {
  const paragraphs = useMemo(() => {
    if (!content) return []
    if (content.paragraphs.length > 0) return content.paragraphs
    return content.body.split(/\n{2,}/).filter(Boolean)
  }, [content])

  const chapters = useMemo(
    () => buildChapters(paragraphs, content?.body),
    [paragraphs, content?.body],
  )

  const [localActive, setLocalActive] = useState<string | null>(null)

  useEffect(() => {
    onChaptersChange?.(chapters.map((c) => ({ id: c.id, title: c.title })))
  }, [chapters, onChaptersChange])

  useEffect(() => {
    if (!chapters.length) return
    const nodes = chapters
      .map((c) => document.getElementById(c.id))
      .filter((n): n is HTMLElement => Boolean(n))

    if (!nodes.length) return

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((e) => e.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio)
        const top = visible[0]?.target.id
        if (top) {
          setLocalActive(top)
          onActiveChapterChange(top)
        }
      },
      { rootMargin: '-20% 0px -55% 0px', threshold: [0.1, 0.35, 0.6] },
    )

    nodes.forEach((n) => observer.observe(n))
    return () => observer.disconnect()
  }, [chapters, onActiveChapterChange])

  if (loading) {
    return (
      <div className="flex min-h-[320px] items-center justify-center px-6 py-16">
        <Spinner label="Preparando o texto para leitura…" />
      </div>
    )
  }

  if (!content || (!content.body && paragraphs.length === 0)) {
    return (
      <div className="px-6 py-12 sm:px-10">
        <p className="measure mx-auto font-serif text-lg leading-relaxed text-muted">
          Este artigo não está disponível para leitura no Acervo. Volte à busca e escolha outro.
        </p>
      </div>
    )
  }

  const current = activeChapterId ?? localActive ?? chapters[0]?.id

  return (
    <div className={`relative ${readingMode ? 'reader-focus' : ''}`}>
      {(content.message || content.source) && !readingMode && (
        <div className="border-b border-line px-6 py-3 sm:px-10">
          <p className="text-xs text-muted">
            {content.source === 'PdfText' &&
              `Texto extraído do PDF${content.pageCount ? ` · ${content.pageCount} páginas` : ''}`}
            {content.source === 'HtmlPage' && 'Texto extraído da página da fonte'}
            {content.source === 'Abstract' && 'Resumo disponível'}
            {content.message ? ` · ${content.message}` : ''}
            {chapters.length > 1 ? ` · ${chapters.length} seções` : ''}
          </p>
        </div>
      )}

      <div
        ref={bodyRef}
        className={`reader-body px-6 py-10 sm:px-12 sm:py-14 ${
          readingMode ? 'sm:px-16 sm:py-16' : ''
        }`}
        style={{ fontSize: `${fontSize}px` }}
      >
        <div
          className={`mx-auto font-serif leading-[1.85] text-ink-soft ${
            readingMode ? 'max-w-[62ch]' : 'measure'
          }`}
        >
          {chapters.map((chapter) => {
            const showHeading = chapter.title !== 'Texto completo'

            return (
              <section
                key={chapter.id}
                id={chapter.id}
                data-chapter={chapter.id}
                className={`reader-chapter scroll-mt-28 ${
                  current === chapter.id ? 'is-active' : ''
                }`}
              >
                {showHeading && (
                  <h2 className="reader-chapter-title mb-5 mt-10 font-display text-[1.15em] font-semibold tracking-tight text-ink first:mt-0">
                    {chapter.title}
                  </h2>
                )}
                <div className="space-y-5">
                  {chapter.paragraphIndices.map((pIndex) => {
                    if (
                      looksLikeHeading(paragraphs[pIndex]) &&
                      pIndex === chapter.headingParagraphIndex &&
                      showHeading
                    ) {
                      return null
                    }
                    const offset = paragraphOffset(paragraphs, pIndex)
                    const text = paragraphs[pIndex]
                    return (
                      <p
                        key={`${chapter.id}-${pIndex}`}
                        data-offset={offset}
                        className="reader-paragraph"
                      >
                        {renderHighlightedSlice(text, offset, highlights)}
                      </p>
                    )
                  })}
                </div>
              </section>
            )
          })}
        </div>
      </div>
    </div>
  )
}
