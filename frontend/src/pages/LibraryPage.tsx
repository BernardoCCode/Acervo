import { ArrowLeft, FolderPlus, Trash } from '@phosphor-icons/react'
import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import {
  Button,
  EmptyState,
  ErrorBanner,
  Field,
  Panel,
  Spinner,
} from '../components/ui'
import { api } from '../lib/api'
import type {
  Article,
  Collection,
  CollectionDetail,
  Favorite,
  Recommendation,
  SearchHistoryItem,
} from '../lib/types'

type Tab = 'favorites' | 'collections' | 'history' | 'recommendations'

export function LibraryPage() {
  const [tab, setTab] = useState<Tab>('favorites')
  const [favorites, setFavorites] = useState<Favorite[]>([])
  const [favoriteArticles, setFavoriteArticles] = useState<Article[]>([])
  const [collections, setCollections] = useState<Collection[]>([])
  const [selectedCollection, setSelectedCollection] = useState<CollectionDetail | null>(null)
  const [collectionLoading, setCollectionLoading] = useState(false)
  const [history, setHistory] = useState<SearchHistoryItem[]>([])
  const [recommendations, setRecommendations] = useState<Recommendation[]>([])
  const [newCollection, setNewCollection] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    void loadAll()
  }, [])

  async function loadAll() {
    setLoading(true)
    setError(null)
    try {
      const [favs, cols, hist, recs] = await Promise.all([
        api.listFavorites(),
        api.listCollections(),
        api.searchHistory(30),
        api.recommendations(12),
      ])
      setFavorites(favs)
      setCollections(cols)
      setHistory(hist)
      setRecommendations(recs)
      setFavoriteArticles(
        favs
          .map((f) => f.article)
          .filter((article): article is Article => Boolean(article)),
      )
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao carregar biblioteca.')
    } finally {
      setLoading(false)
    }
  }

  async function createCollection(e: FormEvent) {
    e.preventDefault()
    const name = newCollection.trim()
    if (!name) return
    try {
      const created = await api.createCollection(name)
      setCollections((prev) => [created, ...prev])
      setNewCollection('')
      setTab('collections')
      setSelectedCollection(null)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível criar a coleção.')
    }
  }

  async function openCollection(collectionId: string) {
    setCollectionLoading(true)
    setError(null)
    try {
      const detail = await api.getCollection(collectionId)
      setSelectedCollection(detail)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível abrir a coleção.')
    } finally {
      setCollectionLoading(false)
    }
  }

  async function removeFromCollection(articleId: string) {
    if (!selectedCollection) return
    try {
      await api.removeFromCollection(selectedCollection.id, articleId)
      const nextArticles = selectedCollection.articles.filter((a) => a.id !== articleId)
      const nextDetail = {
        ...selectedCollection,
        articles: nextArticles,
        itemCount: nextArticles.length,
      }
      setSelectedCollection(nextDetail)
      setCollections((prev) =>
        prev.map((c) =>
          c.id === selectedCollection.id
            ? { ...c, itemCount: nextArticles.length }
            : c,
        ),
      )
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao remover da coleção.')
    }
  }

  async function removeFavorite(articleId: string) {
    try {
      await api.unfavorite(articleId)
      setFavorites((prev) => prev.filter((f) => f.articleId !== articleId))
      setFavoriteArticles((prev) => prev.filter((a) => a.id !== articleId))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao remover favorito.')
    }
  }

  async function dismissRec(id: string) {
    try {
      await api.dismissRecommendation(id)
      setRecommendations((prev) => prev.filter((r) => r.id !== id))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao dispensar.')
    }
  }

  function switchTab(next: Tab) {
    setTab(next)
    if (next !== 'collections') {
      setSelectedCollection(null)
    }
  }

  const tabs: { id: Tab; label: string }[] = [
    { id: 'favorites', label: 'Favoritos' },
    { id: 'collections', label: 'Coleções' },
    { id: 'history', label: 'Histórico' },
    { id: 'recommendations', label: 'Recomendações' },
  ]

  return (
    <div className="page-enter space-y-8">
      <header className="space-y-2">
        <h1 className="font-display text-3xl font-semibold tracking-tight text-ink sm:text-4xl">
          Biblioteca
        </h1>
        <p className="text-muted">
          Favoritos, coleções, histórico de busca e recomendações num só lugar.
        </p>
      </header>

      <div className="flex flex-wrap gap-2">
        {tabs.map((t) => (
          <button
            key={t.id}
            type="button"
            onClick={() => switchTab(t.id)}
            className={`rounded-full px-4 py-2 text-sm font-medium transition ${
              tab === t.id
                ? 'bg-accent text-accent-fg'
                : 'bg-surface-2 text-ink-soft hover:bg-line/70'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {error && <ErrorBanner message={error} />}
      {loading && <Spinner label="Abrindo biblioteca…" />}

      {!loading && tab === 'favorites' && (
        favoriteArticles.length === 0 ? (
          <EmptyState
            title="Nenhum favorito ainda"
            description="Na busca ou no leitor, toque em Salvar para guardar artigos aqui."
            action={
              <Link to="/buscar" className="text-sm font-medium text-accent-ink hover:underline">
                Ir para busca
              </Link>
            }
          />
        ) : (
          <Panel className="divide-y divide-line">
            {favoriteArticles.map((article) => (
              <div
                key={article.id}
                className="flex flex-col gap-3 px-5 py-5 sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <Link
                    to={`/artigo/${article.id}`}
                    className="font-display text-lg font-semibold text-ink hover:text-accent-ink"
                  >
                    {article.title}
                  </Link>
                  <p className="mt-1 text-sm text-muted">
                    {[article.venue, article.year].filter(Boolean).join(' · ')}
                  </p>
                </div>
                <Button variant="ghost" onClick={() => void removeFavorite(article.id)}>
                  <Trash size={16} />
                  Remover
                </Button>
              </div>
            ))}
          </Panel>
        )
      )}

      {!loading && tab === 'collections' && (
        <div className="space-y-6">
          {!selectedCollection && (
            <Panel className="p-5">
              <form onSubmit={createCollection} className="flex flex-col gap-3 sm:flex-row">
                <Field
                  value={newCollection}
                  onChange={(e) => setNewCollection(e.target.value)}
                  placeholder="Nome da coleção (ex.: Psicologia cognitiva)"
                  className="flex-1"
                />
                <Button type="submit">
                  <FolderPlus size={18} weight="duotone" />
                  Criar coleção
                </Button>
              </form>
            </Panel>
          )}

          {collectionLoading && <Spinner label="Abrindo coleção…" />}

          {!collectionLoading && selectedCollection ? (
            <div className="space-y-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <button
                    type="button"
                    onClick={() => setSelectedCollection(null)}
                    className="mb-2 inline-flex items-center gap-1.5 text-sm font-medium text-accent-ink hover:underline"
                  >
                    <ArrowLeft size={16} />
                    Todas as coleções
                  </button>
                  <h2 className="font-display text-2xl font-semibold text-ink">
                    {selectedCollection.name}
                  </h2>
                  {selectedCollection.description && (
                    <p className="mt-1 text-sm text-muted">{selectedCollection.description}</p>
                  )}
                  <p className="mt-2 text-xs text-muted">
                    {selectedCollection.itemCount} artigo
                    {selectedCollection.itemCount === 1 ? '' : 's'}
                  </p>
                </div>
              </div>

              {selectedCollection.articles.length === 0 ? (
                <EmptyState
                  title="Coleção vazia"
                  description="Abra um artigo e use “Adicionar à coleção” no leitor."
                  action={
                    <Link
                      to="/buscar"
                      className="text-sm font-medium text-accent-ink hover:underline"
                    >
                      Buscar artigos
                    </Link>
                  }
                />
              ) : (
                <Panel className="divide-y divide-line">
                  {selectedCollection.articles.map((article) => (
                    <div
                      key={article.id}
                      className="flex flex-col gap-3 px-5 py-5 sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div>
                        <Link
                          to={`/artigo/${article.id}`}
                          className="font-display text-lg font-semibold text-ink hover:text-accent-ink"
                        >
                          {article.title}
                        </Link>
                        <p className="mt-1 text-sm text-muted">
                          {[article.venue, article.year].filter(Boolean).join(' · ')}
                        </p>
                      </div>
                      <Button
                        variant="ghost"
                        onClick={() => void removeFromCollection(article.id)}
                      >
                        <Trash size={16} />
                        Remover
                      </Button>
                    </div>
                  ))}
                </Panel>
              )}
            </div>
          ) : null}

          {!collectionLoading && !selectedCollection && (
            collections.length === 0 ? (
              <EmptyState
                title="Sem coleções"
                description="Organize artigos por tema, por exemplo “Papers clássicos” ou “Para ler no fim de semana”."
              />
            ) : (
              <div className="grid gap-4 sm:grid-cols-2">
                {collections.map((c) => (
                  <button
                    key={c.id}
                    type="button"
                    onClick={() => void openCollection(c.id)}
                    className="text-left"
                  >
                    <Panel className="p-5 transition hover:border-accent/40 hover:bg-surface-2/60">
                      <h3 className="font-display text-lg font-semibold text-ink">{c.name}</h3>
                      {c.description && (
                        <p className="mt-1 text-sm text-muted">{c.description}</p>
                      )}
                      <p className="mt-4 text-xs text-muted">
                        {c.itemCount} artigo{c.itemCount === 1 ? '' : 's'}
                      </p>
                    </Panel>
                  </button>
                ))}
              </div>
            )
          )}
        </div>
      )}

      {!loading && tab === 'history' && (
        history.length === 0 ? (
          <EmptyState
            title="Histórico vazio"
            description="Suas buscas aparecem aqui para retomar rápido."
          />
        ) : (
          <Panel className="divide-y divide-line">
            {history.map((item) => (
              <Link
                key={item.id}
                to={`/buscar?q=${encodeURIComponent(item.query)}`}
                className="flex items-center justify-between px-5 py-4 transition hover:bg-surface-2"
              >
                <span className="font-medium text-ink">{item.query}</span>
                <span className="text-xs text-muted">{item.resultCount} resultados</span>
              </Link>
            ))}
          </Panel>
        )
      )}

      {!loading && tab === 'recommendations' && (
        recommendations.length === 0 ? (
          <EmptyState
            title="Sem recomendações por enquanto"
            description="Salve artigos e explore temas para alimentar esta lista."
          />
        ) : (
          <Panel className="divide-y divide-line">
            {recommendations.map((rec) => (
              <div
                key={rec.id}
                className="flex flex-col gap-3 px-5 py-5 sm:flex-row sm:items-center sm:justify-between"
              >
                <div>
                  <Link
                    to={`/artigo/${rec.article.id}`}
                    className="font-display text-lg font-semibold text-ink hover:text-accent-ink"
                  >
                    {rec.article.title}
                  </Link>
                  <p className="mt-1 text-sm text-muted">
                    {rec.explanation || rec.reason}
                  </p>
                </div>
                <Button variant="ghost" onClick={() => void dismissRec(rec.id)}>
                  Dispensar
                </Button>
              </div>
            ))}
          </Panel>
        )
      )}

      {!loading && (
        <p className="text-xs text-muted">
          {favorites.length} favorito{favorites.length === 1 ? '' : 's'} ·{' '}
          {collections.length} coleç{collections.length === 1 ? 'ão' : 'ões'}
        </p>
      )}
    </div>
  )
}
