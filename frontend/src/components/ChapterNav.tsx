import { ListBullets } from '@phosphor-icons/react'

type ChapterLink = { id: string; title: string }

type Props = {
  chapters: ChapterLink[]
  activeId: string | null
  compact?: boolean
}

export function ChapterNav({ chapters, activeId, compact = false }: Props) {
  if (chapters.length <= 1) return null

  return (
    <nav aria-label="Capítulos" className={compact ? '' : 'space-y-2'}>
      {!compact && (
        <p className="flex items-center gap-2 text-sm font-medium text-ink">
          <ListBullets size={18} weight="duotone" />
          Capítulos
        </p>
      )}
      <ol className={`space-y-1 ${compact ? 'max-h-48 overflow-y-auto' : 'max-h-[50vh] overflow-y-auto pr-1'}`}>
        {chapters.map((ch, i) => {
          const active = ch.id === activeId
          return (
            <li key={ch.id}>
              <a
                href={`#${ch.id}`}
                className={`block rounded-xl px-3 py-2 text-left text-sm transition ${
                  active
                    ? 'bg-accent-soft font-medium text-accent-ink'
                    : 'text-muted hover:bg-surface-2 hover:text-ink'
                }`}
                onClick={(e) => {
                  e.preventDefault()
                  document.getElementById(ch.id)?.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                  })
                }}
              >
                <span className="mr-2 tabular-nums text-xs text-muted">
                  {String(i + 1).padStart(2, '0')}
                </span>
                <span className="line-clamp-2">{ch.title}</span>
              </a>
            </li>
          )
        })}
      </ol>
    </nav>
  )
}
