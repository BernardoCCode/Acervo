export type SourceSystem =
  | 'OpenAlex'
  | 'ArXiv'
  | 'Crossref'
  | 'PubMed'
  | 'Doaj'
  | 'Scholar'

export type StudyType =
  | 'Unknown'
  | 'Review'
  | 'MetaAnalysis'
  | 'Empirical'
  | 'Theoretical'
  | 'Other'

export type InsightType = 'Summary' | 'BeginnerExplanation' | 'Translation'
export type ReadableContentSource = 'PdfText' | 'HtmlPage' | 'Abstract'

export interface ReadableContent {
  articleId: string
  title: string
  body: string
  paragraphs: string[]
  source: ReadableContentSource
  pageCount: number | null
  isFallback: boolean
  message: string | null
}
export type CitationStyle = 'BibTeX' | 'Apa' | 'Abnt'
export type DifficultyLevel =
  | 'Beginner'
  | 'Intermediate'
  | 'Advanced'
  | 'Classic'
  | 'RecentResearch'
export type TrailStatus = 'Draft' | 'Ready' | 'Failed'
export type HighlightColor = 'Yellow' | 'Green' | 'Blue' | 'Pink' | 'Orange'
export type RecommendationReason =
  | 'FavoriteSimilarity'
  | 'TopicMatch'
  | 'HighlyCited'
  | 'Recent'
  | 'SemanticSimilarity'
  | 'FromLearningTrail'

export interface Article {
  id: string
  title: string
  abstract: string | null
  authors: string[]
  venue: string | null
  year: number | null
  doi: string | null
  url: string | null
  pdfUrl: string | null
  language: string | null
  citationCount: number
  primarySource: SourceSystem
  studyType: StudyType
  topics: string[]
}

export interface SearchFilters {
  yearMin?: number | null
  yearMax?: number | null
  language?: string | null
  studyType?: StudyType | null
  minCitations?: number | null
  sources?: SourceSystem[] | null
}

export interface SearchResult {
  searchQueryId: string
  query: string
  resultCount: number
  articles: Article[]
}

export interface SearchHistoryItem {
  id: string
  query: string
  resultCount: number
  executedAtUtc: string
  lastAccessedAtUtc: string | null
}

export interface Favorite {
  favoriteId: string
  articleId: string
  createdAtUtc: string
  article?: Article | null
}

export interface Collection {
  id: string
  name: string
  description: string | null
  itemCount: number
  createdAtUtc: string
}

export interface CollectionDetail extends Collection {
  articles: Article[]
}

export interface TrailStep {
  id: string
  order: number
  title: string
  difficulty: DifficultyLevel
  articleId: string | null
  articleTitle: string | null
  rationale: string | null
}

export interface LearningTrail {
  id: string
  prompt: string
  topic: string
  status: TrailStatus
  failureReason: string | null
  steps: TrailStep[]
  createdAtUtc: string
}

export interface Insight {
  id: string
  articleId: string
  type: InsightType
  content: string
  sourceLanguage: string | null
  targetLanguage: string | null
  createdAtUtc: string
}

export interface Annotation {
  id: string
  note: string
  createdAtUtc: string
}

export interface Highlight {
  id: string
  startOffset: number
  endOffset: number
  pageNumber: number | null
  quotedText: string
  color: HighlightColor
  annotations: Annotation[]
}

export interface ReadingSession {
  id: string
  articleId: string
  progressPercent: number
  pageNumber: number | null
  isCompleted: boolean
  lastOpenedAtUtc: string
  openCount: number
  activeReadingSeconds: number
  highlights: Highlight[]
}

export interface ReaderPreferences {
  darkMode: boolean
  fontSize: number
  preferredTranslationLanguage: string | null
}

export interface Recommendation {
  id: string
  reason: RecommendationReason
  score: number
  explanation: string | null
  sourceArticleId: string | null
  topicScore: number
  engagementScore: number
  qualityScore: number
  freshnessScore: number
  expiresAtUtc: string
  article: Article
}

export interface AuthUser {
  id: string
  email: string
  displayName: string | null
  preferredLanguage: string
  interests: string[]
}

export interface AuthResponse {
  token: string
  expiresAtUtc: string
  user: AuthUser
}

export interface CitationResponse {
  style: CitationStyle
  citation: string
}
