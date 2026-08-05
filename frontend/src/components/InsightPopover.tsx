import {
  ArrowsClockwise,
  Check,
  Copy,
  Lightbulb,
  Sparkle,
  Translate,
  X,
} from '@phosphor-icons/react'
import { useEffect, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import type { InsightType } from '../lib/types'

export type TranslationLang = 'pt' | 'en'

export type InsightPopoverState = {
  open: boolean
  /** Viewport anchor (clientX / clientY) near the selection */
  x: number
  y: number
  type: InsightType
  /** The selected excerpt the AI is answering about */
  excerpt: string
  loading: boolean
  content: string | null
  error: string | null
  /** Translation waits for a target-language choice before running. */
  needsLanguage: boolean
}

type Props = {
  state: InsightPopoverState
  onClose: () => void
  onRetry: () => void
  onChooseLanguage: (lang: TranslationLang) => void
}

const WIDTH = 380

const TYPE_META: Record<
  InsightType,
  { label: string; loading: string; icon: typeof Sparkle }
> = {
  Summary: { label: 'Resumo do trecho', loading: 'Resumindo…', icon: Sparkle },
  BeginnerExplanation: {
    label: 'Explicação do trecho',
    loading: 'Explicando…',
    icon: Lightbulb,
  },
  Translation: {
    label: 'Tradução do trecho',
    loading: 'Traduzindo…',
    icon: Translate,
  },
}

export function InsightPopover({
  state,
  onClose,
  onRetry,
  onChooseLanguage,
}: Props) {
  const ref = useRef<HTMLDivElement>(null)
  const [pos, setPos] = useState({ left: state.x, top: state.y })
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    if (!state.open) return
    setCopied(false)
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [state.open, state.content, onClose])

  useLayoutEffect(() => {
    if (!state.open) return

    const pad = 16
    const el = ref.current
    const width = el?.offsetWidth ?? WIDTH
    const height = el?.offsetHeight ?? 240
    const vw = window.innerWidth
    const vh = window.innerHeight

    // Prefer beside the selection on wide screens, below it on narrow ones
    let left: number
    const fitsRight = state.x + 32 + width <= vw - pad
    const fitsLeft = state.x - 32 - width >= pad
    if (vw >= 900 && (fitsRight || fitsLeft)) {
      left = fitsRight ? state.x + 32 : state.x - 32 - width
    } else {
      left = Math.max(pad, Math.min(state.x - width / 2, vw - width - pad))
    }

    let top = Math.min(state.y - height / 3, vh - height - pad)
    top = Math.max(pad, top)

    setPos({ left, top })
  }, [
    state.open,
    state.x,
    state.y,
    state.loading,
    state.content,
    state.error,
    state.needsLanguage,
  ])

  if (!state.open || typeof document === 'undefined') return null

  const meta = TYPE_META[state.type]
  const Icon = meta.icon
  const excerpt =
    state.excerpt.length > 110 ? `${state.excerpt.slice(0, 110)}…` : state.excerpt

  async function copyAnswer() {
    if (!state.content) return
    await navigator.clipboard.writeText(state.content)
    setCopied(true)
    window.setTimeout(() => setCopied(false), 1600)
  }

  return createPortal(
    <aside
      ref={ref}
      role="dialog"
      aria-label={meta.label}
      className="insight-popover fixed z-[210] w-[min(380px,calc(100vw-32px))] overflow-hidden rounded-2xl border border-line bg-surface shadow-[var(--shadow-lift)]"
      style={{ left: pos.left, top: pos.top }}
    >
      <header className="flex items-center gap-2 border-b border-line bg-surface-2/60 px-4 py-2.5">
        <Icon size={17} weight="duotone" className="shrink-0 text-accent-ink" />
        <p className="flex-1 truncate text-[13px] font-medium text-ink">
          {meta.label}
        </p>
        {state.content && (
          <button
            type="button"
            onClick={() => void copyAnswer()}
            className="rounded-lg p-1.5 text-muted transition hover:bg-surface-2 hover:text-ink"
            aria-label="Copiar resposta"
          >
            {copied ? (
              <Check size={15} className="text-accent-ink" />
            ) : (
              <Copy size={15} />
            )}
          </button>
        )}
        <button
          type="button"
          onClick={onClose}
          className="rounded-lg p-1.5 text-muted transition hover:bg-surface-2 hover:text-ink"
          aria-label="Fechar"
        >
          <X size={15} />
        </button>
      </header>

      <blockquote className="border-b border-line/70 px-4 py-2">
        <p className="line-clamp-2 font-serif text-xs italic leading-snug text-muted">
          “{excerpt}”
        </p>
      </blockquote>

      <div className="max-h-[46vh] overflow-y-auto px-4 py-3.5">
        {state.needsLanguage && !state.loading && !state.content && (
          <div className="space-y-2.5">
            <p className="text-sm text-ink-soft">Traduzir para qual idioma?</p>
            <div className="flex gap-2">
              <button
                type="button"
                onClick={() => onChooseLanguage('pt')}
                className="flex-1 rounded-xl bg-accent px-3 py-2 text-sm font-medium text-accent-fg transition hover:brightness-110"
              >
                Português
              </button>
              <button
                type="button"
                onClick={() => onChooseLanguage('en')}
                className="flex-1 rounded-xl border border-line bg-surface-2 px-3 py-2 text-sm font-medium text-ink transition hover:border-accent/40"
              >
                English
              </button>
            </div>
          </div>
        )}

        {state.loading && (
          <div className="flex items-center gap-2.5 py-1 text-sm text-muted">
            <span className="insight-pulse inline-block h-2 w-2 rounded-full bg-accent" />
            {meta.loading}
          </div>
        )}

        {!state.loading && state.error && (
          <div className="space-y-2.5">
            <p className="text-sm leading-relaxed text-danger">{state.error}</p>
            <button
              type="button"
              onClick={onRetry}
              className="inline-flex items-center gap-1.5 rounded-xl bg-accent-soft px-3 py-1.5 text-xs font-medium text-accent-ink transition hover:brightness-95"
            >
              <ArrowsClockwise size={14} />
              Tentar de novo
            </button>
          </div>
        )}

        {!state.loading && !state.error && state.content && (
          <p className="whitespace-pre-wrap font-serif text-[15px] leading-relaxed text-ink-soft">
            {state.content}
          </p>
        )}
      </div>
    </aside>,
    document.body,
  )
}
