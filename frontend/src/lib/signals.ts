import type { Article } from './types'

export type ArticleSignal = {
  key: 'cited' | 'review' | 'recent'
  label: string
  score: number
}

export function getArticleSignals(article: Article): ArticleSignal[] {
  const year = article.year ?? 0
  const currentYear = new Date().getFullYear()
  const signals: ArticleSignal[] = []

  if (article.citationCount >= 200) {
    signals.push({ key: 'cited', label: 'Muito citado', score: 5 })
  } else if (article.citationCount >= 50) {
    signals.push({ key: 'cited', label: 'Bem citado', score: 4 })
  } else if (article.citationCount >= 10) {
    signals.push({ key: 'cited', label: 'Citado', score: 3 })
  }

  if (article.studyType === 'Review' || article.studyType === 'MetaAnalysis') {
    signals.push({ key: 'review', label: 'Revisão', score: 4 })
  }

  if (year >= currentYear - 1) {
    signals.push({ key: 'recent', label: 'Artigo recente', score: 3 })
  } else if (year >= currentYear - 3) {
    signals.push({ key: 'recent', label: 'Recente', score: 2 })
  }

  return signals
}

export function stars(score: number) {
  return '★'.repeat(score) + '☆'.repeat(Math.max(0, 5 - score))
}

export function sourceLabel(source: string) {
  if (source === 'ArXiv') return 'arXiv'
  if (source === 'PubMed') return 'Europe PMC'
  if (source === 'Scholar') return 'Semantic Scholar'
  return source
}

export function difficultyLabel(level: string) {
  const map: Record<string, string> = {
    Beginner: 'Iniciante',
    Intermediate: 'Intermediário',
    Advanced: 'Avançado',
    Classic: 'Clássico',
    RecentResearch: 'Pesquisa recente',
  }
  return map[level] ?? level
}

export function truncate(text: string, max = 220) {
  if (text.length <= max) return text
  return `${text.slice(0, max).trimEnd()}…`
}
