import { FunnelSimple, MagnifyingGlass } from '@phosphor-icons/react'
import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ArticleCard } from '../components/ArticleCard'
import { Button, EmptyState, ErrorBanner, Field, Panel, Spinner } from '../components/ui'
import { api } from '../lib/api'
import { useFeatures } from '../lib/features'
import type { Article, SearchFilters, SourceSystem } from '../lib/types'

const SOURCES: SourceSystem[] = ['OpenAlex', 'PubMed', 'Scholar', 'ArXiv']

export function SearchPage() {
  const [params] = useSearchParams()
  const navigate = useNavigate()
  const { aiEnabled } = useFeatures()
  const initialQ = params.get('q') ?? ''

  const [query, setQuery] = useState(initialQ)
  const [articles, setArticles] = useState<Article[]>([])
  const [resultCount, setResultCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showFilters, setShowFilters] = useState(false)
  const [favorites, setFavorites] = useState<Set<string>>(new Set())
  const [savingId, setSavingId] = useState<string | null>(null)

  const [yearMin, setYearMin] = useState('')
  const [yearMax, setYearMax] = useState('')
  const [language, setLanguage] = useState('')
  const [minCitations, setMinCitations] = useState('')
  const [sources, setSources] = useState<SourceSystem[]>([])

  const filters = useMemo<SearchFilters>(() => {
    return {
      yearMin: yearMin ? Number(yearMin) : null,
      yearMax: yearMax ? Number(yearMax) : null,
      language: language || null,
      minCitations: minCitations ? Number(minCitations) : null,
      sources: sources.length ? sources : null,
    }
  }, [yearMin, yearMax, language, minCitations, sources])

  useEffect(() => {
    api.listFavorites()
      .then((list) => setFavorites(new Set(list.map((f) => f.articleId))))
      .catch(() => undefined)
  }, [])

  useEffect(() => {
    const q = params.get('q')?.trim()
    if (!q) {
      setArticles([])
      setResultCount(0)
      return
    }
    setQuery(q)
    void runSearch(q)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params])

  async function runSearch(q: string) {
    setLoading(true)
    setError(null)
    try {
      const result = await api.search(q, filters)
      setArticles(result.articles)
      setResultCount(result.resultCount)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha na busca.')
      setArticles([])
      setResultCount(0)
    } finally {
      setLoading(false)
    }
  }

  function onSubmit(e: FormEvent) {
    e.preventDefault()
    const q = query.trim()
    if (!q) return
    navigate(`/buscar?q=${encodeURIComponent(q)}`)
  }

  function toggleSource(source: SourceSystem) {
    setSources((prev) =>
      prev.includes(source) ? prev.filter((s) => s !== source) : [...prev, source],
    )
  }

  async function toggleFavorite(articleId: string) {
    setSavingId(articleId)
    try {
      if (favorites.has(articleId)) {
        await api.unfavorite(articleId)
        setFavorites((prev) => {
          const next = new Set(prev)
          next.delete(articleId)
          return next
        })
      } else {
        await api.favorite(articleId)
        setFavorites((prev) => new Set(prev).add(articleId))
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível salvar.')
    } finally {
      setSavingId(null)
    }
  }

  return (
    <div className="page-enter space-y-8">
      <header className="space-y-2">
        <h1 className="font-display text-3xl font-semibold tracking-tight text-ink sm:text-4xl">
          Buscar artigos
        </h1>
        <p className="text-muted">
          Busca em OpenAlex, Europe PMC, Semantic Scholar e arXiv — só papers com PDF aberto legível aqui.
        </p>
      </header>

      <form onSubmit={onSubmit} className="space-y-3">
        <div className="relative">
          <MagnifyingGlass
            className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-muted"
            size={20}
            weight="duotone"
          />
          <Field
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="O que você quer aprender hoje?"
            className="pl-11 pr-28"
          />
          <Button type="submit" className="absolute right-2 top-1/2 -translate-y-1/2">
            Buscar
          </Button>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <Button
            type="button"
            variant="secondary"
            onClick={() => setShowFilters((v) => !v)}
          >
            <FunnelSimple size={18} weight="duotone" />
            Filtros
          </Button>
          {params.get('q') && (
            <Button type="button" variant="ghost" onClick={() => void runSearch(params.get('q')!)}>
              Aplicar filtros
            </Button>
          )}
        </div>

        {showFilters && (
          <Panel className="grid gap-4 p-5 sm:grid-cols-2 lg:grid-cols-3">
            <label className="space-y-1.5 text-sm">
              <span className="text-muted">Ano mínimo</span>
              <Field
                type="number"
                value={yearMin}
                onChange={(e) => setYearMin(e.target.value)}
                placeholder="2018"
              />
            </label>
            <label className="space-y-1.5 text-sm">
              <span className="text-muted">Ano máximo</span>
              <Field
                type="number"
                value={yearMax}
                onChange={(e) => setYearMax(e.target.value)}
                placeholder="2026"
              />
            </label>
            <label className="space-y-1.5 text-sm">
              <span className="text-muted">Idioma</span>
              <Field
                value={language}
                onChange={(e) => setLanguage(e.target.value)}
                placeholder="en, pt…"
              />
            </label>
            <label className="space-y-1.5 text-sm">
              <span className="text-muted">Mín. de citações</span>
              <Field
                type="number"
                value={minCitations}
                onChange={(e) => setMinCitations(e.target.value)}
                placeholder="50"
              />
            </label>
            <div className="space-y-2 text-sm sm:col-span-2 lg:col-span-1">
              <span className="text-muted">Fontes</span>
              <div className="flex flex-wrap gap-2">
                {SOURCES.map((source) => {
                  const active = sources.includes(source)
                  return (
                    <button
                      key={source}
                      type="button"
                      onClick={() => toggleSource(source)}
                      className={`rounded-full px-3 py-1.5 text-xs font-medium transition ${
                        active
                          ? 'bg-accent text-accent-fg'
                          : 'bg-surface-2 text-ink-soft hover:bg-line/70'
                      }`}
                    >
                      {source === 'ArXiv'
                        ? 'arXiv'
                        : source === 'PubMed'
                          ? 'Europe PMC'
                          : source === 'Scholar'
                            ? 'Semantic Scholar'
                            : source}
                    </button>
                  )
                })}
              </div>
            </div>
          </Panel>
        )}
      </form>

      {error && <ErrorBanner message={error} />}

      {loading && (
        <Spinner label="Buscando artigos com PDF aberto nas fontes acadêmicas…" />
      )}

      {!loading && params.get('q') && (
        <div>
          <p className="mb-2 text-sm text-muted">
            {resultCount === 1
              ? `1 artigo legível para “${params.get('q')}”`
              : `${resultCount} artigos legíveis para “${params.get('q')}”`}
          </p>
          {articles.length === 0 ? (
            <EmptyState
              title="Nenhum artigo legível encontrado"
              description="Só listamos papers cujo texto conseguimos extrair. Tente outro termo ou remova filtros."
            />
          ) : (
            <Panel className="px-5 sm:px-7">
              <div className="stagger">
                {articles.map((article) => (
                  <ArticleCard
                    key={article.id}
                    article={article}
                    saved={favorites.has(article.id)}
                    saving={savingId === article.id}
                    onSave={() => void toggleFavorite(article.id)}
                    onSummarize={
                      aiEnabled
                        ? () => navigate(`/artigo/${article.id}?insight=Summary`)
                        : undefined
                    }
                  />
                ))}
              </div>
            </Panel>
          )}
        </div>
      )}

      {!loading && !params.get('q') && (
        <EmptyState
          title="Comece por um tema"
          description="Digite o que você quer aprender — Machine Learning, Medicina, Psicologia…"
        />
      )}
    </div>
  )
}
