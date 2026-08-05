import { Path, SpinnerGap } from '@phosphor-icons/react'
import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import {
  Badge,
  Button,
  EmptyState,
  ErrorBanner,
  Field,
  Panel,
  Spinner,
} from '../components/ui'
import { api } from '../lib/api'
import { useFeatures } from '../lib/features'
import { difficultyLabel } from '../lib/signals'
import type { LearningTrail } from '../lib/types'

export function TrailsPage() {
  const { aiEnabled } = useFeatures()
  const [prompt, setPrompt] = useState('')
  const [trails, setTrails] = useState<LearningTrail[]>([])
  const [active, setActive] = useState<LearningTrail | null>(null)
  const [loading, setLoading] = useState(true)
  const [creating, setCreating] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    void refresh()
  }, [])

  async function refresh() {
    setLoading(true)
    setError(null)
    try {
      const list = await api.listTrails()
      setTrails(list)
      if (list.length && !active) setActive(list[0])
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível carregar trilhas.')
    } finally {
      setLoading(false)
    }
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    const value = prompt.trim()
    if (!value) return
    setCreating(true)
    setError(null)
    try {
      const trail = await api.createTrail(value)
      setTrails((prev) => [trail, ...prev])
      setActive(trail)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao gerar trilha.')
    } finally {
      setCreating(false)
    }
  }

  return (
    <div className="page-enter space-y-8">
      <header className="max-w-2xl space-y-2">
        <h1 className="font-display text-3xl font-semibold tracking-tight text-ink sm:text-4xl">
          Trilhas de aprendizado
        </h1>
        <p className="text-muted">
          Descreva seu objetivo com detalhes. A IA decompõe os pré-requisitos,
          decide quantas etapas são necessárias e encontra um artigo para cada uma.
        </p>
      </header>

      {aiEnabled && (
        <Panel className="p-6 sm:p-8">
          <form onSubmit={onSubmit} className="space-y-4">
            <label className="block space-y-2">
              <span className="text-sm text-muted">Seu pedido</span>
              <Field
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                placeholder="Quero aprender visão computacional do zero"
              />
            </label>
            <Button type="submit" disabled={creating}>
              {creating ? (
                <>
                  <SpinnerGap size={18} className="animate-spin" />
                  Gerando trilha…
                </>
              ) : (
                <>
                  <Path size={18} weight="duotone" />
                  Gerar trilha com IA
                </>
              )}
            </Button>
          </form>
        </Panel>
      )}

      {error && <ErrorBanner message={error} />}
      {loading && aiEnabled && <Spinner label="Carregando trilhas…" />}

      {!loading && !aiEnabled && (
        <EmptyState
          title="Trilhas com IA nesta demo"
          description="A geração de trilhas fica disponível quando a IA está ligada. Enquanto isso, use a busca e o leitor."
          action={
            <Link to="/buscar" className="text-sm font-medium text-accent-ink hover:underline">
              Ir para busca
            </Link>
          }
        />
      )}

      {!loading && aiEnabled && trails.length === 0 && (
        <EmptyState
          title="Nenhuma trilha ainda"
          description="Experimente: “Quero aprender machine learning do zero” ou “Introdução à psicologia cognitiva”."
        />
      )}

      {!loading && aiEnabled && trails.length > 0 && (
        <div className="grid gap-6 lg:grid-cols-[280px_minmax(0,1fr)]">
          <Panel className="p-3">
            <ul className="space-y-1">
              {trails.map((trail) => (
                <li key={trail.id}>
                  <button
                    type="button"
                    onClick={() => setActive(trail)}
                    className={`w-full rounded-2xl px-3 py-3 text-left transition ${
                      active?.id === trail.id
                        ? 'bg-accent text-accent-fg'
                        : 'hover:bg-surface-2 text-ink-soft'
                    }`}
                  >
                    <p className="font-medium leading-snug">{trail.topic || trail.prompt}</p>
                    <p className="mt-1 text-xs opacity-80">
                      {trail.steps.length} passos · {trail.status}
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          </Panel>

          {active && (
            <Panel className="p-6 sm:p-8">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h2 className="font-display text-2xl font-semibold text-ink">
                    {active.topic || 'Trilha'}
                  </h2>
                  <p className="mt-2 text-sm text-muted">“{active.prompt}”</p>
                </div>
                <Badge tone={active.status === 'Ready' ? 'accent' : 'default'}>
                  {active.status}
                </Badge>
              </div>

              {active.failureReason && (
                <p className="mt-4 text-sm text-danger">{active.failureReason}</p>
              )}

              <ol className="mt-8 space-y-0">
                {active.steps
                  .slice()
                  .sort((a, b) => a.order - b.order)
                  .map((step, index) => (
                    <li
                      key={step.id}
                      className="relative border-b border-line py-5 last:border-b-0"
                    >
                      <div className="flex gap-4">
                        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-accent font-display text-sm font-semibold text-accent-fg">
                          {index + 1}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="flex flex-wrap items-center gap-2">
                            <h3 className="font-display text-lg font-semibold text-ink">
                              {step.title}
                            </h3>
                            <Badge>{difficultyLabel(step.difficulty)}</Badge>
                          </div>
                          {step.rationale && (
                            <p className="mt-2 font-serif text-sm leading-relaxed text-ink-soft">
                              {step.rationale}
                            </p>
                          )}
                          {step.articleId ? (
                            <Link
                              to={`/artigo/${step.articleId}`}
                              className="mt-3 block rounded-xl bg-surface-2 px-3 py-2.5 transition hover:bg-accent-soft"
                            >
                              <span className="block text-xs font-medium text-muted">
                                Leitura escolhida para esta etapa
                              </span>
                              <span className="mt-0.5 block text-sm font-medium leading-snug text-accent-ink">
                                {step.articleTitle || 'Abrir artigo sugerido'}
                              </span>
                            </Link>
                          ) : (
                            <p className="mt-3 text-xs text-muted">
                              Artigo ainda não vinculado. Use a busca pelo título do passo.
                            </p>
                          )}
                        </div>
                      </div>
                    </li>
                  ))}
              </ol>
            </Panel>
          )}
        </div>
      )}
    </div>
  )
}
