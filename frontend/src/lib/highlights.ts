import type { Highlight } from './types'

export function highlightsOverlapping(
  highlights: Highlight[],
  start: number,
  end: number,
): Highlight[] {
  return highlights.filter(
    (h) => h.startOffset < end && h.endOffset > start,
  )
}

/** True when the selection sits inside (or equals) an existing highlight. */
export function isSelectionHighlighted(
  highlights: Highlight[],
  start: number,
  end: number,
): boolean {
  if (end <= start) return false
  return highlightsOverlapping(highlights, start, end).length > 0
}
