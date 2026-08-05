import type {
  Article,
  AuthResponse,
  AuthUser,
  CitationResponse,
  CitationStyle,
  Collection,
  CollectionDetail,
  Favorite,
  Insight,
  InsightType,
  LearningTrail,
  ReaderPreferences,
  ReadingSession,
  ReadableContent,
  Recommendation,
  SearchFilters,
  SearchHistoryItem,
  SearchResult,
} from './types'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

const API_BASE = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '') ?? ''

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = localStorage.getItem('acervo-token')
  const response = await fetch(`${API_BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
  })

  if (!response.ok) {
    let message = `Erro ${response.status}`
    try {
      const body = (await response.json()) as {
        message?: string
        detail?: string
        title?: string
      }
      message = body.detail ?? body.message ?? body.title ?? message
    } catch {
      /* ignore */
    }
    throw new ApiError(response.status, message)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  register(email: string, password: string, displayName?: string, rememberMe = false) {
    return request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify({ email, password, displayName, rememberMe }),
    })
  },

  login(email: string, password: string, rememberMe = false) {
    return request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password, rememberMe }),
    })
  },

  guest() {
    return request<AuthResponse>('/api/auth/guest', { method: 'POST' })
  },

  me(init?: RequestInit) {
    return request<AuthUser>('/api/auth/me', init)
  },

  getFeatures() {
    return request<{ aiEnabled: boolean }>('/api/features')
  },


  search(query: string, filters?: SearchFilters) {
    return request<SearchResult>('/api/search', {
      method: 'POST',
      body: JSON.stringify({ query, filters: filters ?? null }),
    })
  },

  searchHistory(take = 20) {
    return request<SearchHistoryItem[]>(`/api/search/history?take=${take}`)
  },

  getArticle(id: string) {
    return request<Article>(`/api/articles/${id}`)
  },

  getReadableContent(id: string) {
    return request<ReadableContent>(`/api/articles/${id}/content`)
  },

  citation(id: string, style: CitationStyle) {
    return request<CitationResponse>(`/api/articles/${id}/citation?style=${style}`)
  },

  listFavorites() {
    return request<Favorite[]>('/api/library/favorites')
  },

  favorite(articleId: string) {
    return request<Favorite>(`/api/library/favorites/${articleId}`, { method: 'POST' })
  },

  unfavorite(articleId: string) {
    return request<void>(`/api/library/favorites/${articleId}`, { method: 'DELETE' })
  },

  listCollections() {
    return request<Collection[]>('/api/library/collections')
  },

  createCollection(name: string, description?: string) {
    return request<Collection>('/api/library/collections', {
      method: 'POST',
      body: JSON.stringify({ name, description: description ?? null }),
    })
  },

  getCollection(collectionId: string) {
    return request<CollectionDetail>(`/api/library/collections/${collectionId}`)
  },

  addToCollection(collectionId: string, articleId: string) {
    return request<void>(`/api/library/collections/${collectionId}/articles/${articleId}`, {
      method: 'POST',
    })
  },

  removeFromCollection(collectionId: string, articleId: string) {
    return request<void>(`/api/library/collections/${collectionId}/articles/${articleId}`, {
      method: 'DELETE',
    })
  },

  listTrails() {
    return request<LearningTrail[]>('/api/learning-trails')
  },

  getTrail(id: string) {
    return request<LearningTrail>(`/api/learning-trails/${id}`)
  },

  createTrail(prompt: string) {
    return request<LearningTrail>('/api/learning-trails', {
      method: 'POST',
      body: JSON.stringify({ prompt }),
    })
  },

  generateInsight(
    articleId: string,
    type: InsightType,
    sourceLanguage?: string,
    targetLanguage?: string,
    focusText?: string,
  ) {
    return request<Insight>('/api/insights', {
      method: 'POST',
      body: JSON.stringify({
        articleId,
        type,
        sourceLanguage: sourceLanguage ?? null,
        targetLanguage: targetLanguage ?? null,
        focusText: focusText ?? null,
      }),
    })
  },

  openSession(articleId: string) {
    return request<ReadingSession>(`/api/reading/sessions/${articleId}`, { method: 'POST' })
  },

  updateProgress(sessionId: string, percent: number, activeSeconds = 0) {
    return request<ReadingSession>('/api/reading/sessions/progress', {
      method: 'PUT',
      body: JSON.stringify({ sessionId, percent, activeSeconds }),
    })
  },

  addHighlight(payload: {
    sessionId: string
    startOffset: number
    endOffset: number
    quotedText: string
    color?: string
    note?: string
  }) {
    return request<ReadingSession>('/api/reading/sessions/highlights', {
      method: 'POST',
      body: JSON.stringify({
        color: 'Yellow',
        ...payload,
      }),
    })
  },

  removeHighlight(sessionId: string, highlightId: string) {
    return request<ReadingSession>(
      `/api/reading/sessions/${sessionId}/highlights/${highlightId}`,
      { method: 'DELETE' },
    )
  },

  getPreferences() {
    return request<ReaderPreferences>('/api/reading/preferences')
  },

  updatePreferences(prefs: ReaderPreferences) {
    return request<ReaderPreferences>('/api/reading/preferences', {
      method: 'PUT',
      body: JSON.stringify(prefs),
    })
  },

  recommendations(take = 12) {
    return request<Recommendation[]>(`/api/recommendations?take=${take}`)
  },

  refreshRecommendations() {
    return request<{ generatedCount: number }>('/api/recommendations/refresh', {
      method: 'POST',
    })
  },

  dismissRecommendation(id: string) {
    return request<void>(`/api/recommendations/${id}`, { method: 'DELETE' })
  },
}
