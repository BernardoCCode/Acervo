import {
  Copy,
  Eraser,
  HighlighterCircle,
  Sparkle,
  Lightbulb,
  Translate,
  X,
} from '@phosphor-icons/react'
import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'

export type ReaderMenuAction =
  | 'highlight'
  | 'unhighlight'
  | 'summary'
  | 'explain'
  | 'translate'
  | 'copy'

type Props = {
  open: boolean
  /** Viewport X (clientX) */
  x: number
  /** Viewport Y (clientY) */
  y: number
  selectedText: string
  /** Selection overlaps an existing highlight */
  isHighlighted: boolean
  /** When false, hide summarize / explain / translate */
  aiEnabled?: boolean
  onAction: (action: ReaderMenuAction) => void
  onClose: () => void
}

const MENU_WIDTH = 224

export function ReaderMenu({
  open,
  x,
  y,
  selectedText,
  isHighlighted,
  aiEnabled = true,
  onAction,
  onClose,
}: Props) {
  const ref = useRef<HTMLDivElement>(null)
  const [pos, setPos] = useState({ left: x, top: y })

  useEffect(() => {
    if (!open) return
    const onPointer = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose()
    }
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    const onScroll = () => onClose()
    window.addEventListener('mousedown', onPointer)
    window.addEventListener('keydown', onKey)
    window.addEventListener('scroll', onScroll, true)
    return () => {
      window.removeEventListener('mousedown', onPointer)
      window.removeEventListener('keydown', onKey)
      window.removeEventListener('scroll', onScroll, true)
    }
  }, [open, onClose])

  useLayoutEffect(() => {
    if (!open) return

    const pad = 12
    const menu = ref.current
    const height = menu?.offsetHeight ?? 280
    const width = menu?.offsetWidth ?? MENU_WIDTH
    const vw = window.innerWidth
    const vh = window.innerHeight

    let left = x - width / 2
    left = Math.max(pad, Math.min(left, vw - width - pad))

    let top = y + 8
    if (top + height > vh - pad) {
      top = y - height - 8
    }
    top = Math.max(pad, Math.min(top, vh - height - pad))

    setPos({ left, top })
  }, [open, x, y, selectedText, isHighlighted])

  if (!open || typeof document === 'undefined') return null

  const preview =
    selectedText.length > 80 ? `${selectedText.slice(0, 80)}…` : selectedText

  const items: {
    action: ReaderMenuAction
    label: string
    icon: typeof Sparkle
  }[] = [
    isHighlighted
      ? { action: 'unhighlight', label: 'Remover destaque', icon: Eraser }
      : { action: 'highlight', label: 'Destacar', icon: HighlighterCircle },
    ...(aiEnabled
      ? ([
          { action: 'explain', label: 'Explicar trecho', icon: Lightbulb },
          { action: 'summary', label: 'Resumir trecho', icon: Sparkle },
          { action: 'translate', label: 'Traduzir trecho', icon: Translate },
        ] as const)
      : []),
    { action: 'copy', label: 'Copiar', icon: Copy },
  ]

  return createPortal(
    <div
      ref={ref}
      role="menu"
      aria-label="Ações do trecho"
      className="reader-menu fixed z-[200] w-56 overflow-hidden rounded-2xl border border-line bg-surface shadow-[var(--shadow-lift)]"
      style={{ left: pos.left, top: pos.top }}
    >
      <div className="flex items-start justify-between gap-2 border-b border-line px-3 py-2.5">
        <p className="line-clamp-2 font-serif text-xs leading-snug text-muted">
          “{preview}”
        </p>
        <button
          type="button"
          className="shrink-0 rounded-lg p-1 text-muted hover:bg-surface-2 hover:text-ink"
          onClick={onClose}
          aria-label="Fechar"
        >
          <X size={14} />
        </button>
      </div>
      <ul className="p-1.5">
        {items.map(({ action, label, icon: Icon }) => (
          <li key={action}>
            <button
              type="button"
              role="menuitem"
              className={`flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-left text-sm transition hover:bg-accent-soft hover:text-accent-ink ${
                action === 'unhighlight' ? 'text-danger' : 'text-ink-soft'
              }`}
              onClick={() => onAction(action)}
            >
              <Icon
                size={18}
                weight="duotone"
                className={
                  action === 'unhighlight' ? 'text-danger' : 'text-accent-ink'
                }
              />
              {label}
            </button>
          </li>
        ))}
      </ul>
    </div>,
    document.body,
  )
}

/** Best viewport point near the current text selection. */
export function getSelectionAnchorPoint(): { x: number; y: number } | null {
  const sel = window.getSelection()
  if (!sel || sel.rangeCount === 0 || sel.isCollapsed) return null

  const range = sel.getRangeAt(0)
  const rects = range.getClientRects()
  const rect =
    rects.length > 0
      ? rects[rects.length - 1]
      : range.getBoundingClientRect()

  if (!rect || (rect.width === 0 && rect.height === 0)) {
    return {
      x: window.innerWidth / 2,
      y: window.innerHeight / 2,
    }
  }

  return {
    x: rect.left + rect.width / 2,
    y: rect.bottom,
  }
}
