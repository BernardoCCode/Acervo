import {
  ArrowSquareOut,
  BookmarkSimple,
  Books,
  Copy,
  FolderPlus,
  HighlighterCircle,
  Sparkle,
  TextAa,
  Translate,
} from '@phosphor-icons/react'
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type MouseEvent,
} from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { ChapterNav } from '../components/ChapterNav'
import {
  InsightPopover,
  type InsightPopoverState,
  type TranslationLang,
} from '../components/InsightPopover'
import {
  getSelectionAnchorPoint,
  ReaderMenu,
  type ReaderMenuAction,
} from '../components/ReaderMenu'
import { TextReader } from '../components/TextReader'
import {
  Badge,
  Button,
  ErrorBanner,
  Panel,
  Spinner,
} from '../components/ui'
import { api } from '../lib/api'
import { useFeatures } from '../lib/features'
import { useReadingMode } from '../lib/readingMode'
import {
  highlightsOverlapping,
  isSelectionHighlighted,
} from '../lib/highlights'
import { clearSelection, getBodySelectionOffsets } from '../lib/selection'
import { getArticleSignals, sourceLabel } from '../lib/signals'
import { useTheme } from '../lib/theme'
import type {
  Article,
  CitationStyle,
  Collection,
  Insight,
  InsightType,
  ReadableContent,
  ReaderPreferences,
  ReadingSession,
} from '../lib/types'

type MenuState = {
  open: boolean
  x: number
  y: number
  text: string
  start: number
  end: number
}

export function ArticlePage() {
  const { id = '' } = useParams()
  const [searchParams] = useSearchParams()
  const { theme, setTheme } = useTheme()
  const { readingMode, setReadingMode, toggleReadingMode } = useReadingMode()
  const { aiEnabled } = useFeatures()
  const bodyRef = useRef<HTMLDivElement>(null)

  const [article, setArticle] = useState<Article | null>(null)
  const [readable, setReadable] = useState<ReadableContent | null>(null)
  const [readableLoading, setReadableLoading] = useState(true)
  const [session, setSession] = useState<ReadingSession | null>(null)
  const [prefs, setPrefs] = useState<ReaderPreferences>({
    darkMode: theme === 'dark',
    fontSize: 19,
    preferredTranslationLanguage: 'pt',
  })
  const [saved, setSaved] = useState(false)
  const [collections, setCollections] = useState<Collection[]>([])
  const [showCollectionPicker, setShowCollectionPicker] = useState(false)
  const [collectionBusyId, setCollectionBusyId] = useState<string | null>(null)
  const [newCollectionName, setNewCollectionName] = useState('')
  const [insight, setInsight] = useState<Insight | null>(null)
  const [insightLoading, setInsightLoading] = useState(false)
  const [translation, setTranslation] = useState<{
    language: 'pt' | 'en'
    body: string
  } | null>(null)
  const [citation, setCitation] = useState<string | null>(null)
  const [citationStyle, setCitationStyle] = useState<CitationStyle>('Apa')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [status, setStatus] = useState<string | null>(null)
  const [chapters, setChapters] = useState<{ id: string; title: string }[]>([])
  const [activeChapterId, setActiveChapterId] = useState<string | null>(null)
  const [menu, setMenu] = useState<MenuState>({
    open: false,
    x: 0,
    y: 0,
    text: '',
    start: 0,
    end: 0,
  })
  const [popover, setPopover] = useState<InsightPopoverState>({
    open: false,
    x: 0,
    y: 0,
    type: 'Summary',
    excerpt: '',
    loading: false,
    content: null,
    error: null,
    needsLanguage: false,
  })

  const menuIsHighlighted = useMemo(
    () =>
      menu.open &&
      isSelectionHighlighted(session?.highlights ?? [], menu.start, menu.end),
    [menu.open, menu.start, menu.end, session?.highlights],
  )

  const signals = useMemo(
    () => (article ? getArticleSignals(article) : []),
    [article],
  )

  const readerContent = useMemo<ReadableContent | null>(() => {
    if (!readable || !translation) return readable
    const paragraphs = translation.body
      .split(/\n\s*\n/)
      .map((paragraph) => paragraph.trim())
      .filter(Boolean)
    return {
      ...readable,
      body: translation.body,
      paragraphs: paragraphs.length ? paragraphs : [translation.body],
      message:
        translation.language === 'pt'
          ? 'Artigo traduzido para português pela IA.'
          : 'Article translated into English by AI.',
    }
  }, [readable, translation])

  const closeMenu = useCallback(() => {
    setMenu((m) => ({ ...m, open: false }))
  }, [])

  useEffect(() => {
    return () => setReadingMode(false)
  }, [setReadingMode])

  useEffect(() => {
    if (!id) return
    let cancelled = false
    ;(async () => {
      setLoading(true)
      setReadableLoading(true)
      setError(null)
      setInsight(null)
      setTranslation(null)
      setChapters([])
      try {
        const a = await api.getArticle(id)
        if (cancelled) return
        setArticle(a)

        const [
          sessionResult,
          prefsResult,
          favoritesResult,
          contentResult,
          collectionsResult,
        ] = await Promise.allSettled([
          api.openSession(id),
          api.getPreferences(),
          api.listFavorites(),
          api.getReadableContent(id),
          api.listCollections(),
        ])

        if (cancelled) return

        if (sessionResult.status === 'fulfilled') setSession(sessionResult.value)
        if (prefsResult.status === 'fulfilled') {
          setPrefs(prefsResult.value)
          if (prefsResult.value.darkMode !== (theme === 'dark')) {
            setTheme(prefsResult.value.darkMode ? 'dark' : 'light')
          }
        }
        if (favoritesResult.status === 'fulfilled') {
          setSaved(favoritesResult.value.some((f) => f.articleId === id))
        }
        if (collectionsResult.status === 'fulfilled') {
          setCollections(collectionsResult.value)
        }
        if (contentResult.status === 'fulfilled') {
          const content = contentResult.value
          const hasBody =
            Boolean(content.body?.trim()) || content.paragraphs.length > 0
          setReadable(hasBody ? content : null)
        } else {
          setReadable(null)
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'Artigo não encontrado.')
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
          setReadableLoading(false)
        }
      }
    })()
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  useEffect(() => {
    const type = searchParams.get('insight') as InsightType | null
    if (aiEnabled && type && article) void runInsight(type)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [article, searchParams, aiEnabled])

  // Reading progress from scroll
  useEffect(() => {
    if (!session || !bodyRef.current) return
    let lastSent = session.progressPercent
    let currentPercent = session.progressPercent
    const onScroll = () => {
      const el = document.documentElement
      const max = el.scrollHeight - el.clientHeight
      if (max <= 0) return
      const percent = Math.min(100, Math.round((el.scrollTop / max) * 100))
      currentPercent = Math.max(currentPercent, percent)
      if (Math.abs(percent - lastSent) < 5) return
      lastSent = percent
      void api.updateProgress(session.id, percent).then(setSession).catch(() => undefined)
    }
    const activeTimer = window.setInterval(() => {
      if (document.visibilityState !== 'visible') return
      void api
        .updateProgress(session.id, currentPercent, 30)
        .then(setSession)
        .catch(() => undefined)
    }, 30_000)
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => {
      window.removeEventListener('scroll', onScroll)
      window.clearInterval(activeTimer)
    }
  }, [session?.id])

  function openMenuAt(clientX: number, clientY: number) {
    const container = bodyRef.current
    if (!container) return
    const offsets = getBodySelectionOffsets(container)
    if (!offsets) {
      setStatus('Selecione um trecho do texto.')
      return
    }
    setMenu({
      open: true,
      x: clientX,
      y: clientY,
      text: offsets.text,
      start: offsets.start,
      end: offsets.end,
    })
  }

  function onBodyMouseUp(e: MouseEvent) {
    if (e.button !== 0) return
    const fallbackX = e.clientX
    const fallbackY = e.clientY
    // Small delay so selection settles
    window.setTimeout(() => {
      const container = bodyRef.current
      if (!container) return
      const offsets = getBodySelectionOffsets(container)
      if (!offsets) return
      const anchor = getSelectionAnchorPoint()
      setMenu({
        open: true,
        x: anchor?.x ?? fallbackX,
        y: anchor?.y ?? fallbackY,
        text: offsets.text,
        start: offsets.start,
        end: offsets.end,
      })
    }, 10)
  }

  function onBodyContextMenu(e: MouseEvent) {
    e.preventDefault()
    const anchor = getSelectionAnchorPoint()
    openMenuAt(anchor?.x ?? e.clientX, anchor?.y ?? e.clientY)
  }

  async function persistPrefs(next: ReaderPreferences) {
    setPrefs(next)
    try {
      await api.updatePreferences(next)
    } catch {
      /* keep local */
    }
  }

  async function toggleFavorite() {
    if (!article) return
    try {
      if (saved) {
        await api.unfavorite(article.id)
        setSaved(false)
        setStatus('Removido dos favoritos.')
      } else {
        await api.favorite(article.id)
        setSaved(true)
        setStatus('Salvo nos favoritos.')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao salvar.')
    }
  }

  async function addArticleToCollection(collectionId: string) {
    if (!article) return
    setCollectionBusyId(collectionId)
    setError(null)
    try {
      const name = collections.find((c) => c.id === collectionId)?.name
      await api.addToCollection(collectionId, article.id)
      const refreshed = await api.listCollections()
      setCollections(refreshed)
      setStatus(name ? `Adicionado a “${name}”.` : 'Adicionado à coleção.')
      setShowCollectionPicker(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao adicionar à coleção.')
    } finally {
      setCollectionBusyId(null)
    }
  }

  async function createAndAddToCollection() {
    if (!article) return
    const name = newCollectionName.trim()
    if (!name) return
    setCollectionBusyId('new')
    setError(null)
    try {
      const created = await api.createCollection(name)
      await api.addToCollection(created.id, article.id)
      setCollections((prev) => [{ ...created, itemCount: 1 }, ...prev])
      setNewCollectionName('')
      setStatus(`Adicionado a “${created.name}”.`)
      setShowCollectionPicker(false)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao criar coleção.')
    } finally {
      setCollectionBusyId(null)
    }
  }

  async function runInsight(type: InsightType, targetLanguage?: string) {
    if (!article) return
    if (type === 'Translation' && !targetLanguage) {
      setError('Escolha o idioma da tradução.')
      return
    }
    setInsightLoading(true)
    setError(null)
    try {
      if (!readable) {
        try {
          const content = await api.getReadableContent(article.id)
          setReadable(content)
        } catch {
          /* AI still works with abstract */
        }
      }
      const result = await api.generateInsight(
        article.id,
        type,
        article.language ?? undefined,
        type === 'Translation' ? targetLanguage : undefined,
      )
      if (type === 'Translation' && targetLanguage) {
        setTranslation({
          language: targetLanguage as 'pt' | 'en',
          body: result.content,
        })
        setInsight(null)
        setStatus(
          targetLanguage === 'pt'
            ? 'O artigo agora está em português.'
            : 'O artigo agora está em inglês.',
        )
        window.scrollTo({ top: 0, behavior: 'smooth' })
      } else {
        setInsight(result)
        setStatus('Resposta da IA pronta.')
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'IA indisponível no momento.')
    } finally {
      setInsightLoading(false)
    }
  }

  /** Selection-focused AI: answer appears in a card anchored beside the excerpt. */
  async function runFocusInsight(
    type: InsightType,
    excerpt: string,
    anchorX: number,
    anchorY: number,
    targetLanguage?: string,
  ) {
    if (!article) return
    setPopover((p) => ({
      ...p,
      open: true,
      x: anchorX,
      y: anchorY,
      type,
      excerpt,
      loading: true,
      content: null,
      error: null,
      needsLanguage: false,
    }))
    clearSelection()
    try {
      const result = await api.generateInsight(
        article.id,
        type,
        article.language ?? undefined,
        type === 'Translation' ? targetLanguage ?? 'pt' : undefined,
        excerpt,
      )
      setPopover((p) =>
        p.open ? { ...p, loading: false, content: result.content } : p,
      )
    } catch (err) {
      setPopover((p) =>
        p.open
          ? {
              ...p,
              loading: false,
              error:
                err instanceof Error
                  ? err.message
                  : 'IA indisponível no momento.',
            }
          : p,
      )
    }
  }

  /** Open the popover in "choose target language" mode before translating. */
  function askTranslationLanguage(
    excerpt: string,
    anchorX: number,
    anchorY: number,
  ) {
    setPopover({
      open: true,
      x: anchorX,
      y: anchorY,
      type: 'Translation',
      excerpt,
      loading: false,
      content: null,
      error: null,
      needsLanguage: true,
    })
  }

  const closePopover = useCallback(() => {
    setPopover((p) => ({ ...p, open: false }))
  }, [])

  async function highlightRange(start: number, end: number, quoted: string) {
    if (translation) {
      setStatus('Volte ao texto original para criar ou remover destaques.')
      return
    }
    if (!session) {
      setStatus('Sessão de leitura ainda não está pronta.')
      return
    }
    if (isSelectionHighlighted(session.highlights, start, end)) {
      await removeHighlightsInRange(start, end)
      return
    }
    try {
      const quotedText =
        readable?.body && end <= readable.body.length
          ? readable.body.slice(start, end)
          : quoted
      const updated = await api.addHighlight({
        sessionId: session.id,
        startOffset: start,
        endOffset: end,
        quotedText: quotedText || quoted,
        color: 'Yellow',
      })
      setSession(updated)
      setStatus('Trecho destacado.')
      clearSelection()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao destacar.')
    }
  }

  async function removeHighlightsInRange(start: number, end: number) {
    if (!session) {
      setStatus('Sessão de leitura ainda não está pronta.')
      return
    }
    const targets = highlightsOverlapping(session.highlights, start, end)
    if (!targets.length) {
      setStatus('Nenhum destaque nesse trecho.')
      return
    }
    try {
      let updated = session
      for (const h of targets) {
        updated = await api.removeHighlight(session.id, h.id)
      }
      setSession(updated)
      setStatus(
        targets.length === 1
          ? 'Destaque removido.'
          : `${targets.length} destaques removidos.`,
      )
      clearSelection()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao remover destaque.')
    }
  }

  async function handleMenuAction(action: ReaderMenuAction) {
    const { text, start, end, x, y } = menu
    closeMenu()

    switch (action) {
      case 'highlight':
        await highlightRange(start, end, text)
        break
      case 'unhighlight':
        await removeHighlightsInRange(start, end)
        break
      case 'explain':
        await runFocusInsight('BeginnerExplanation', text, x, y)
        break
      case 'summary':
        await runFocusInsight('Summary', text, x, y)
        break
      case 'translate':
        askTranslationLanguage(text, x, y)
        break
      case 'copy':
        await navigator.clipboard.writeText(text)
        setStatus('Trecho copiado.')
        clearSelection()
        break
    }
  }

  async function highlightSelection() {
    const container = bodyRef.current
    if (!container) {
      setStatus('Selecione um trecho do texto para destacar.')
      return
    }
    const offsets = getBodySelectionOffsets(container)
    if (!offsets) {
      setStatus('Selecione um trecho do texto para destacar.')
      return
    }
    await highlightRange(offsets.start, offsets.end, offsets.text)
  }

  async function loadCitation(style: CitationStyle) {
    if (!article) return
    setCitationStyle(style)
    try {
      const result = await api.citation(article.id, style)
      setCitation(result.citation)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao exportar citação.')
    }
  }

  async function copyCitation() {
    if (!citation) return
    await navigator.clipboard.writeText(citation)
    setStatus('Citação copiada.')
  }

  if (loading) {
    return (
      <div className="page-enter">
        <Spinner label="Abrindo leitor…" />
      </div>
    )
  }

  if (!article) {
    return (
      <div className="page-enter space-y-4">
        {error && <ErrorBanner message={error} />}
        <Link to="/buscar" className="text-accent-ink underline-offset-4 hover:underline">
          Voltar à busca
        </Link>
      </div>
    )
  }

  return (
    <div className={`page-enter ${readingMode ? 'reading-page' : 'space-y-6'}`}>
      {!readingMode && (
        <div className="flex flex-wrap items-center gap-3 text-sm text-muted">
          <Link to="/buscar" className="hover:text-accent-ink">
            Busca
          </Link>
          <span>/</span>
          <span className="text-ink-soft">Leitura</span>
        </div>
      )}

      {error && !readingMode && <ErrorBanner message={error} />}
      {status && !readingMode && (
        <p className="rounded-2xl bg-accent-soft px-4 py-2 text-sm text-accent-ink" role="status">
          {status}
        </p>
      )}

      {readingMode && (
        <div className="reading-toolbar fixed inset-x-0 top-0 z-[60] border-b border-line/70 bg-canvas/90 backdrop-blur-md">
          <div className="mx-auto flex max-w-5xl items-center justify-between gap-3 px-3 py-2.5 sm:px-4">
            <p className="hidden truncate font-display text-sm font-medium text-ink md:block">
              {article.title}
            </p>
            <div className="flex shrink-0 items-center gap-1">
              {aiEnabled && (
                <>
                  <span className="mr-1 hidden items-center gap-1 text-xs text-muted sm:inline-flex">
                    <Translate size={14} /> Artigo:
                  </span>
                  <Button
                    variant={translation?.language === 'pt' ? 'primary' : 'ghost'}
                    className="!px-2 !py-1.5 text-xs"
                    onClick={() => void runInsight('Translation', 'pt')}
                    disabled={insightLoading}
                  >
                    PT
                  </Button>
                  <Button
                    variant={translation?.language === 'en' ? 'primary' : 'ghost'}
                    className="!px-2 !py-1.5 text-xs"
                    onClick={() => void runInsight('Translation', 'en')}
                    disabled={insightLoading}
                  >
                    EN
                  </Button>
                  {translation && (
                    <Button
                      variant="ghost"
                      className="!px-2 !py-1.5 text-xs"
                      onClick={() => {
                        setTranslation(null)
                        setStatus('Texto original restaurado.')
                      }}
                    >
                      Original
                    </Button>
                  )}
                  <span className="mx-1 h-5 w-px bg-line" aria-hidden="true" />
                </>
              )}
              <Button
                variant="ghost"
                className="!px-2 !py-1.5 text-xs"
                onClick={() =>
                  void persistPrefs({
                    ...prefs,
                    fontSize: Math.max(15, prefs.fontSize - 1),
                  })
                }
              >
                A−
              </Button>
              <Button
                variant="ghost"
                className="!px-2 !py-1.5 text-xs"
                onClick={() =>
                  void persistPrefs({
                    ...prefs,
                    fontSize: Math.min(28, prefs.fontSize + 1),
                  })
                }
              >
                A+
              </Button>
              <Button
                variant="secondary"
                className="!py-1.5 !text-xs"
                onClick={() => setReadingMode(false)}
              >
                Sair do modo leitura
              </Button>
            </div>
          </div>
          {status && (
            <p className="border-t border-line/60 px-4 py-1.5 text-center text-xs text-accent-ink">
              {status}
            </p>
          )}
        </div>
      )}

      <div
        className={
          readingMode
            ? 'pt-14'
            : 'grid gap-6 xl:grid-cols-[minmax(0,1fr)_300px]'
        }
      >
        <Panel
          className={`overflow-hidden ${readingMode ? 'border-0 bg-transparent shadow-none' : ''}`}
        >
          {!readingMode && (
            <header className="border-b border-line px-6 py-6 sm:px-10">
              {signals.length > 0 && (
                <div className="mb-3 flex flex-wrap gap-2">
                  {signals.map((s) => (
                    <Badge key={s.key} tone={s.key === 'cited' ? 'signal' : 'accent'}>
                      {s.label}
                    </Badge>
                  ))}
                </div>
              )}
              <h1 className="font-display text-3xl font-semibold tracking-tight text-ink sm:text-[2.35rem] sm:leading-tight">
                {article.title}
              </h1>
              <p className="mt-3 text-sm text-muted">
                {[article.venue, article.year, sourceLabel(article.primarySource)]
                  .filter(Boolean)
                  .join(' · ')}
                {article.citationCount > 0 &&
                  ` · ${article.citationCount.toLocaleString('pt-BR')} citações`}
              </p>
              {article.authors.length > 0 && (
                <p className="mt-2 text-sm text-ink-soft">
                  {article.authors.slice(0, 8).join(', ')}
                  {article.authors.length > 8 ? ' et al.' : ''}
                </p>
              )}
            </header>
          )}

          <div
            onMouseUp={onBodyMouseUp}
            onContextMenu={onBodyContextMenu}
            className="reader-interaction"
          >
            <TextReader
              content={readerContent}
              loading={readableLoading}
              fontSize={prefs.fontSize}
              bodyRef={bodyRef}
              highlights={translation ? [] : session?.highlights ?? []}
              activeChapterId={activeChapterId}
              onActiveChapterChange={setActiveChapterId}
              onChaptersChange={setChapters}
              readingMode={readingMode}
            />
          </div>

          {!readingMode && (
            <div className="flex flex-wrap gap-4 border-t border-line px-6 py-4 sm:px-10">
              {article.url && (
                <a
                  href={article.url}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 text-sm font-medium text-accent-ink hover:underline"
                >
                  Página na fonte
                  <ArrowSquareOut size={14} />
                </a>
              )}
              {article.pdfUrl && (
                <a
                  href={article.pdfUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 text-sm font-medium text-muted hover:text-accent-ink hover:underline"
                >
                  PDF original
                  <ArrowSquareOut size={14} />
                </a>
              )}
            </div>
          )}
        </Panel>

        {!readingMode && (
          <aside className="space-y-4 xl:sticky xl:top-24 xl:max-h-[calc(100vh-7rem)] xl:self-start xl:overflow-y-auto xl:overscroll-contain xl:pr-1">
            <Panel className="space-y-3 p-4">
              <p className="text-sm font-medium text-ink">Leitura</p>
              <Button className="w-full" onClick={toggleReadingMode}>
                <Books size={18} weight="duotone" />
                Modo leitura
              </Button>
              <div className="flex items-center justify-between gap-2">
                <span className="text-sm text-ink-soft">Tema</span>
                <Button
                  variant="secondary"
                  className="!py-2"
                  onClick={() => {
                    const nextDark = theme !== 'dark'
                    setTheme(nextDark ? 'dark' : 'light')
                    void persistPrefs({ ...prefs, darkMode: nextDark })
                  }}
                >
                  {theme === 'dark' ? 'Escuro' : 'Claro'}
                </Button>
              </div>
              <div className="flex items-center justify-between gap-2">
                <span className="inline-flex items-center gap-2 text-sm text-ink-soft">
                  <TextAa size={18} /> Fonte
                </span>
                <div className="flex gap-1">
                  <Button
                    variant="secondary"
                    className="!px-3 !py-2"
                    onClick={() =>
                      void persistPrefs({
                        ...prefs,
                        fontSize: Math.max(15, prefs.fontSize - 1),
                      })
                    }
                  >
                    A−
                  </Button>
                  <Button
                    variant="secondary"
                    className="!px-3 !py-2"
                    onClick={() =>
                      void persistPrefs({
                        ...prefs,
                        fontSize: Math.min(28, prefs.fontSize + 1),
                      })
                    }
                  >
                    A+
                  </Button>
                </div>
              </div>
              <Button
                variant="secondary"
                className="w-full"
                onClick={() => void highlightSelection()}
              >
                <HighlighterCircle size={18} weight="duotone" />
                Destacar seleção
              </Button>
              <Button variant="secondary" className="w-full" onClick={() => void toggleFavorite()}>
                <BookmarkSimple size={18} weight={saved ? 'fill' : 'duotone'} />
                {saved ? 'Nos favoritos' : 'Salvar favorito'}
              </Button>
              <Button
                variant="secondary"
                className="w-full"
                onClick={() => setShowCollectionPicker((open) => !open)}
                aria-expanded={showCollectionPicker}
              >
                <FolderPlus size={18} weight="duotone" />
                Adicionar à coleção
              </Button>
              {showCollectionPicker && (
                <div
                  className="space-y-2 rounded-2xl border border-line bg-surface-2/50 p-3"
                  role="menu"
                  aria-label="Escolher coleção"
                >
                  {collections.length === 0 ? (
                    <p className="px-1 text-xs text-muted">
                      Nenhuma coleção ainda. Crie uma abaixo.
                    </p>
                  ) : (
                    collections.map((collection) => (
                      <button
                        key={collection.id}
                        type="button"
                        role="menuitem"
                        disabled={collectionBusyId !== null}
                        onClick={() => void addArticleToCollection(collection.id)}
                        className="flex w-full items-center justify-between rounded-xl px-3 py-2.5 text-left text-sm text-ink transition hover:bg-surface disabled:opacity-50"
                      >
                        <span className="font-medium">{collection.name}</span>
                        <span className="text-xs text-muted">
                          {collectionBusyId === collection.id
                            ? '…'
                            : `${collection.itemCount} art.`}
                        </span>
                      </button>
                    ))
                  )}
                  <div className="flex gap-2 border-t border-line pt-2">
                    <input
                      value={newCollectionName}
                      onChange={(e) => setNewCollectionName(e.target.value)}
                      placeholder="Nova coleção"
                      className="min-w-0 flex-1 rounded-xl border border-line bg-surface px-3 py-2 text-sm text-ink placeholder:text-muted"
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault()
                          void createAndAddToCollection()
                        }
                      }}
                    />
                    <Button
                      variant="secondary"
                      className="!px-3"
                      disabled={!newCollectionName.trim() || collectionBusyId !== null}
                      onClick={() => void createAndAddToCollection()}
                    >
                      Criar
                    </Button>
                  </div>
                </div>
              )}
              <p className="text-xs leading-relaxed text-muted">
                {aiEnabled
                  ? 'Selecione um trecho ou clique com o botão direito para destacar e usar a IA.'
                  : 'Selecione um trecho ou clique com o botão direito para destacar.'}
              </p>
            </Panel>

            {chapters.length > 1 && (
              <Panel className="p-4">
                <ChapterNav chapters={chapters} activeId={activeChapterId} />
              </Panel>
            )}

            {aiEnabled && (
              <Panel className="space-y-3 p-4">
                <p className="text-sm font-medium text-ink">Assistente IA</p>
                <Button
                  className="w-full"
                  onClick={() => void runInsight('Summary')}
                  disabled={insightLoading}
                >
                  <Sparkle size={18} weight="duotone" />
                  Resumir artigo
                </Button>
                <Button
                  variant="secondary"
                  className="w-full"
                  onClick={() => void runInsight('BeginnerExplanation')}
                  disabled={insightLoading}
                >
                  Explicar para iniciantes
                </Button>
                <div className="rounded-2xl border border-line bg-surface-2/50 p-2.5">
                  <div className="mb-2 flex items-center justify-between gap-2 px-1">
                    <p className="inline-flex items-center gap-2 text-xs font-medium text-ink-soft">
                      <Translate size={16} weight="duotone" /> Idioma do artigo
                    </p>
                    {translation && (
                      <button
                        type="button"
                        className="text-xs font-medium text-accent-ink hover:underline"
                        onClick={() => {
                          setTranslation(null)
                          setStatus('Texto original restaurado.')
                        }}
                      >
                        Ver original
                      </button>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant={translation?.language === 'pt' ? 'primary' : 'secondary'}
                      className="flex-1 !py-2"
                      onClick={() => void runInsight('Translation', 'pt')}
                      disabled={insightLoading}
                    >
                      Português
                    </Button>
                    <Button
                      variant={translation?.language === 'en' ? 'primary' : 'secondary'}
                      className="flex-1 !py-2"
                      onClick={() => void runInsight('Translation', 'en')}
                      disabled={insightLoading}
                    >
                      English
                    </Button>
                  </div>
                </div>
                {insightLoading && (
                  <Spinner
                    label={
                      translation
                        ? 'Atualizando tradução…'
                        : 'Gerando com IA…'
                    }
                  />
                )}
                {insight && (
                  <div className="rounded-2xl bg-surface-2 p-3">
                    <p className="mb-1.5 text-xs font-medium text-accent-ink">
                      {insight.type === 'Summary'
                        ? 'Resumo'
                        : insight.type === 'BeginnerExplanation'
                          ? 'Para iniciantes'
                          : 'Tradução'}
                    </p>
                    <div className="max-h-[min(50vh,22rem)] overflow-y-auto overscroll-contain pr-1 text-sm leading-relaxed text-ink-soft whitespace-pre-wrap">
                      {insight.content}
                    </div>
                  </div>
                )}
              </Panel>
            )}

            <Panel className="space-y-3 p-4">
              <p className="text-sm font-medium text-ink">Citar</p>
              <div className="flex flex-wrap gap-2">
                {(['Apa', 'Abnt', 'BibTeX'] as CitationStyle[]).map((style) => (
                  <Button
                    key={style}
                    variant={citationStyle === style && citation ? 'primary' : 'secondary'}
                    className="!py-2"
                    onClick={() => void loadCitation(style)}
                  >
                    {style === 'Apa' ? 'APA' : style === 'Abnt' ? 'ABNT' : 'BibTeX'}
                  </Button>
                ))}
              </div>
              {citation && (
                <>
                  <pre className="max-h-40 overflow-auto whitespace-pre-wrap rounded-2xl bg-surface-2 p-3 font-serif text-xs leading-relaxed text-ink-soft">
                    {citation}
                  </pre>
                  <Button variant="ghost" className="w-full" onClick={() => void copyCitation()}>
                    <Copy size={16} />
                    Copiar
                  </Button>
                </>
              )}
            </Panel>

            {session && session.highlights.length > 0 && (
              <Panel className="space-y-2 p-4">
                <p className="text-sm font-medium text-ink">Destaques</p>
                <ul className="space-y-2">
                  {session.highlights.map((h) => (
                    <li
                      key={h.id}
                      className="rounded-xl bg-signal-soft/60 px-3 py-2 text-xs leading-relaxed text-ink-soft"
                    >
                      “{h.quotedText}”
                    </li>
                  ))}
                </ul>
              </Panel>
            )}
          </aside>
        )}
      </div>

      {readingMode && insight && (
        <div className="fixed bottom-4 right-4 z-[70] max-w-sm rounded-2xl border border-line bg-surface p-4 shadow-[var(--shadow-lift)]">
          <p className="mb-1 text-xs font-medium text-accent-ink">
            {insight.type === 'Summary'
              ? 'Resumo'
              : insight.type === 'BeginnerExplanation'
                ? 'Explicação'
                : 'Tradução'}
          </p>
          <p className="max-h-48 overflow-y-auto font-serif text-sm leading-relaxed text-ink-soft">
            {insight.content}
          </p>
          <Button
            variant="ghost"
            className="mt-2 !py-1.5 text-xs"
            onClick={() => setInsight(null)}
          >
            Fechar
          </Button>
        </div>
      )}

      <ReaderMenu
        open={menu.open}
        x={menu.x}
        y={menu.y}
        selectedText={menu.text}
        isHighlighted={menuIsHighlighted}
        aiEnabled={aiEnabled}
        onAction={(a) => void handleMenuAction(a)}
        onClose={closeMenu}
      />

      <InsightPopover
        state={popover}
        onClose={closePopover}
        onChooseLanguage={(lang: TranslationLang) =>
          void runFocusInsight(
            'Translation',
            popover.excerpt,
            popover.x,
            popover.y,
            lang,
          )
        }
        onRetry={() => {
          if (popover.type === 'Translation') {
            setPopover((p) => ({
              ...p,
              needsLanguage: true,
              error: null,
              content: null,
            }))
          } else {
            void runFocusInsight(
              popover.type,
              popover.excerpt,
              popover.x,
              popover.y,
            )
          }
        }}
      />
    </div>
  )
}
