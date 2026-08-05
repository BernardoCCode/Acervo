import { ArrowsClockwise, MagnifyingGlass, Path, Sparkle, X } from '@phosphor-icons/react'
import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import { useFeatures } from '../lib/features'
import type { Recommendation, SearchHistoryItem } from '../lib/types'
import { Button, ErrorBanner, Field, Panel, Spinner } from '../components/ui'
import { sourceLabel, truncate } from '../lib/signals'

const SUGGESTIONS = [
  'Machine Learning',
  'Medicina',
  'Psicologia',
  'Direito',
  'Astronomia',
  'Visão computacional',
  'Neurociência',
  'Mudanças climáticas',
]

export function HomePage() {
  const navigate = useNavigate()
  const { aiEnabled } = useFeatures()
  const [query, setQuery] = useState('')
  const [history, setHistory] = useState<SearchHistoryItem[]>([])
  const [recommendations, setRecommendations] = useState<Recommendation[]>([])
  const [loadingMeta, setLoadingMeta] = useState(true)
  const [refreshing, setRefreshing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    let pollTimer: ReturnType<typeof setTimeout> | undefined

    ;(async () => {
      try {
        const [h, r] = await Promise.all([
          api.searchHistory(8),
          api.recommendations(6),
        ])
        if (!cancelled) {
          setHistory(h)
          setRecommendations(r)
          // Cold feed is filled in the background — poll a few times without blocking first paint.
          if (r.length === 0) {
            const poll = async (attempt: number) => {
              if (cancelled || attempt > 4) return
              pollTimer = setTimeout(async () => {
                try {
                  const next = await api.recommendations(6)
                  if (cancelled) return
                  if (next.length > 0) {
                    setRecommendations(next)
                    return
                  }
                  await poll(attempt + 1)
                } catch {
                  /* ignore soft poll errors */
                }
              }, 2500 * attempt)
            }
            void poll(1)
          }
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Não foi possível carregar o início.')
        }
      } finally {
        if (!cancelled) setLoadingMeta(false)
      }
    })()
    return () => {
      cancelled = true
      if (pollTimer) clearTimeout(pollTimer)
    }
  }, [])

  function goSearch(value: string) {
    const q = value.trim()
    if (!q) return
    navigate(`/buscar?q=${encodeURIComponent(q)}`)
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault()
    goSearch(query)
  }

  async function refreshRecommendations() {
    setRefreshing(true)
    setError(null)
    try {
      await api.refreshRecommendations()
      setRecommendations(await api.recommendations(6))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível atualizar as recomendações.')
    } finally {
      setRefreshing(false)
    }
  }

  async function dismissRecommendation(id: string) {
    await api.dismissRecommendation(id)
    setRecommendations((items) => items.filter((item) => item.id !== id))
  }

  return (
    <div className="page-enter space-y-12">
      <section className="relative overflow-hidden rounded-[2rem] border border-line bg-surface px-6 py-12 shadow-[var(--shadow-lift)] sm:px-10 sm:py-16">
        <div
          aria-hidden
          className="pointer-events-none absolute inset-0 opacity-70"
          style={{
            background:
              'radial-gradient(circle at 15% 20%, var(--glow-a), transparent 42%), radial-gradient(circle at 90% 10%, var(--glow-b), transparent 36%)',
          }}
        />
        <div className="relative mx-auto max-w-2xl text-center">
          <h1 className="font-display text-5xl font-semibold tracking-tight text-ink sm:text-6xl">
            Acervo
          </h1>
          <p className="mt-4 font-display text-2xl font-medium tracking-tight text-ink-soft sm:text-3xl">
            O que você quer aprender hoje?
          </p>
          <p className="mx-auto mt-4 max-w-xl text-base leading-relaxed text-muted">
            {aiEnabled
              ? 'Busque em várias fontes acadêmicas numa só tela — e leia, salve e entenda com ajuda de IA.'
              : 'Busque em várias fontes acadêmicas numa só tela — leia com conforto e salve o que importa.'}
          </p>

          <form onSubmit={onSubmit} className="mt-8">
            <label className="sr-only" htmlFor="home-search">
              Buscar tema
            </label>
            <div className="relative">
              <MagnifyingGlass
                className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-muted"
                size={22}
                weight="duotone"
              />
              <Field
                id="home-search"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
                placeholder="Inteligência Artificial, visão computacional…"
                className="pl-12 pr-28 text-base"
                autoFocus
              />
              <Button
                type="submit"
                className="absolute right-2 top-1/2 -translate-y-1/2"
              >
                Buscar
              </Button>
            </div>
          </form>

          <div className="mt-6 flex flex-wrap justify-center gap-2">
            {SUGGESTIONS.map((topic) => (
              <button
                key={topic}
                type="button"
                onClick={() => goSearch(topic)}
                className="rounded-full border border-line bg-canvas/70 px-3.5 py-1.5 text-sm text-ink-soft transition hover:border-accent hover:text-accent-ink"
              >
                {topic}
              </button>
            ))}
          </div>

          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            {aiEnabled && (
              <Link
                to="/trilhas"
                className="inline-flex items-center gap-2 rounded-xl border border-line bg-canvas/80 px-4 py-2.5 text-sm font-medium text-ink-soft transition hover:border-accent hover:text-accent-ink"
              >
                <Path size={18} weight="duotone" />
                Criar trilha com IA
              </Link>
            )}
            <Link
              to="/biblioteca"
              className="inline-flex items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium text-muted transition hover:text-ink"
            >
              <Sparkle size={18} weight="duotone" />
              Abrir biblioteca
            </Link>
          </div>
        </div>
      </section>

      {error && <ErrorBanner message={error} />}

      {loadingMeta ? (
        <Spinner label="Preparando sua estante…" />
      ) : (
        <div className="grid gap-8 lg:grid-cols-2">
          <Panel className="p-6">
            <h2 className="font-display text-lg font-semibold text-ink">
              Buscas recentes
            </h2>
            <p className="mt-1 text-sm text-muted">Retome de onde parou.</p>
            {history.length === 0 ? (
              <p className="mt-6 text-sm text-muted">
                Ainda não há histórico. Faça sua primeira busca acima.
              </p>
            ) : (
              <ul className="mt-5 space-y-2 stagger">
                {history.map((item) => (
                  <li key={item.id}>
                    <button
                      type="button"
                      onClick={() => goSearch(item.query)}
                      className="flex w-full items-center justify-between rounded-2xl px-3 py-3 text-left transition hover:bg-surface-2"
                    >
                      <span className="font-medium text-ink">{item.query}</span>
                      <span className="text-xs text-muted">
                        {item.resultCount} resultados
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </Panel>

          <Panel className="p-6">
            <div className="flex items-center justify-between gap-3">
              <h2 className="font-display text-lg font-semibold text-ink">
                Recomendações para você
              </h2>
              <button
                type="button"
                onClick={() => void refreshRecommendations()}
                disabled={refreshing}
                className="inline-flex items-center gap-1.5 rounded-xl px-2.5 py-1.5 text-xs font-medium text-accent-ink hover:bg-accent-soft disabled:opacity-60"
              >
                <ArrowsClockwise className={refreshing ? 'animate-spin' : ''} size={15} />
                {refreshing ? 'Analisando…' : 'Atualizar'}
              </button>
            </div>
            <p className="mt-1 text-sm text-muted">
              Sugestões a partir da sua biblioteca e buscas.
            </p>
            {recommendations.length === 0 ? (
              <p className="mt-6 text-sm text-muted">
                Gerando sugestões em segundo plano… salve artigos e busque temas
                para personalizar ainda mais.
              </p>
            ) : (
              <ul className="mt-5 space-y-3 stagger">
                {recommendations.map((rec) => (
                  <li key={rec.id}>
                    <div className="group relative rounded-2xl transition hover:bg-surface-2">
                      <Link
                        to={`/artigo/${rec.article.id}`}
                        className="block px-3 py-3 pr-10"
                      >
                        <p className="font-medium leading-snug text-ink">
                          {truncate(rec.article.title, 90)}
                        </p>
                        <p className="mt-1 text-xs text-muted">
                          {sourceLabel(rec.article.primarySource)}
                          {rec.article.year ? ` · ${rec.article.year}` : ''}
                        </p>
                        {rec.explanation && (
                          <p className="mt-2 text-sm leading-relaxed text-ink-soft">
                            {rec.explanation}
                          </p>
                        )}
                      </Link>
                      <button
                        type="button"
                        onClick={() => void dismissRecommendation(rec.id)}
                        className="absolute right-2 top-2 rounded-lg p-1.5 text-muted opacity-0 transition hover:bg-canvas hover:text-danger group-hover:opacity-100 focus:opacity-100"
                        aria-label="Não recomendar este artigo"
                      >
                        <X size={15} />
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </Panel>
        </div>
      )}
    </div>
  )
}
