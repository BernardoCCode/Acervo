/** Resolve a text selection to offsets inside the article body container. */
export function getBodySelectionOffsets(
  container: HTMLElement,
): { start: number; end: number; text: string } | null {
  const selection = window.getSelection()
  if (!selection || selection.rangeCount === 0 || selection.isCollapsed) return null

  const range = selection.getRangeAt(0)
  if (!container.contains(range.commonAncestorContainer)) return null

  const raw = range.toString()
  if (!raw.trim()) return null

  const startNode = range.startContainer
  const endNode = range.endContainer
  const startEl = closestParagraph(startNode, container)
  const endEl = closestParagraph(endNode, container)
  if (!startEl || !endEl) return null

  const startBase = Number(startEl.dataset.offset ?? '0')
  const endBase = Number(endEl.dataset.offset ?? '0')

  const startLocal = offsetWithinParagraph(startEl, range, 'start')
  const endLocal = offsetWithinParagraph(endEl, range, 'end')

  const start = startBase + startLocal
  const end = endBase + endLocal
  if (end <= start) return null

  // Keep quoted text aligned with offsets (no whitespace collapsing).
  const text = raw.trim()
  return { start, end, text }
}

function closestParagraph(
  node: Node,
  container: HTMLElement,
): HTMLElement | null {
  let current: Node | null = node
  while (current && current !== container) {
    if (current instanceof HTMLElement && current.dataset.offset != null) {
      return current
    }
    current = current.parentNode
  }
  return null
}

function offsetWithinParagraph(
  paragraph: HTMLElement,
  range: Range,
  edge: 'start' | 'end',
): number {
  const probe = document.createRange()
  probe.selectNodeContents(paragraph)
  if (edge === 'start') {
    probe.setEnd(range.startContainer, range.startOffset)
  } else {
    probe.setEnd(range.endContainer, range.endOffset)
  }
  return probe.toString().length
}

export function clearSelection() {
  const selection = window.getSelection()
  selection?.removeAllRanges()
}
