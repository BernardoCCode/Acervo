import { BookmarkSimple, BookOpenText, Sparkle } from '@phosphor-icons/react'
import { Link } from 'react-router-dom'
import { getArticleSignals, sourceLabel, truncate } from '../lib/signals'
import type { Article } from '../lib/types'
import { Badge, Button } from './ui'

type Props = {
  article: Article
  onSave?: () => void
  onSummarize?: () => void
  saving?: boolean
  saved?: boolean
}

export function ArticleCard({
  article,
  onSave,
  onSummarize,
  saving,
  saved,
}: Props) {
  const signals = getArticleSignals(article)
  const summary = article.abstract
    ? truncate(article.abstract, 260)
    : 'Resumo ainda não disponível para este artigo.'

  return (
    <article className="group border-b border-line py-7 last:border-b-0">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0 flex-1 space-y-3">
          {signals.length > 0 && (
            <div className="flex flex-wrap gap-2">
              {signals.map((signal) => (
                <Badge key={signal.key} tone={signal.key === 'cited' ? 'signal' : 'accent'}>
                  <span className="mr-1.5 tracking-tight opacity-80">
                    {'★'.repeat(signal.score)}
                    {'☆'.repeat(5 - signal.score)}
                  </span>
                  {signal.label}
                </Badge>
              ))}
            </div>
          )}

          <div>
            <h3 className="font-display text-xl font-semibold leading-snug tracking-tight text-ink sm:text-2xl">
              <Link to={`/artigo/${article.id}`} className="transition hover:text-accent-ink">
                {article.title}
              </Link>
            </h3>
            <p className="mt-2 text-sm text-muted">
              {[article.venue, article.year, sourceLabel(article.primarySource)]
                .filter(Boolean)
                .join(' · ')}
              {article.citationCount > 0 && (
                <span> · {article.citationCount.toLocaleString('pt-BR')} citações</span>
              )}
            </p>
            {article.authors.length > 0 && (
              <p className="mt-1 text-sm text-ink-soft/90">
                {article.authors.slice(0, 4).join(', ')}
                {article.authors.length > 4 ? ' et al.' : ''}
              </p>
            )}
          </div>

          <p className="measure font-serif text-[1.05rem] leading-relaxed text-ink-soft">
            {summary}
          </p>
        </div>

        <div className="flex shrink-0 flex-row gap-2 sm:flex-col">
          <Link
            to={`/artigo/${article.id}`}
            className="inline-flex flex-1 items-center justify-center gap-2 rounded-xl bg-accent px-4 py-2.5 text-sm font-medium text-accent-fg shadow-[var(--shadow-soft)] transition hover:brightness-110 sm:flex-none"
          >
            <BookOpenText size={18} weight="duotone" />
            Ler
          </Link>
          {onSave && (
            <Button
              variant="secondary"
              onClick={onSave}
              disabled={saving}
              className="flex-1 sm:flex-none"
            >
              <BookmarkSimple size={18} weight={saved ? 'fill' : 'duotone'} />
              {saved ? 'Salvo' : 'Salvar'}
            </Button>
          )}
          {onSummarize && (
            <Button variant="ghost" onClick={onSummarize} className="flex-1 sm:flex-none">
              <Sparkle size={18} weight="duotone" />
              Resumir
            </Button>
          )}
        </div>
      </div>
    </article>
  )
}
