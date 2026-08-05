import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'

type ReadingModeContextValue = {
  readingMode: boolean
  setReadingMode: (value: boolean) => void
  toggleReadingMode: () => void
}

const ReadingModeContext = createContext<ReadingModeContextValue | null>(null)

export function ReadingModeProvider({ children }: { children: ReactNode }) {
  const [readingMode, setReadingModeState] = useState(false)

  const setReadingMode = useCallback((value: boolean) => {
    setReadingModeState(value)
  }, [])

  const toggleReadingMode = useCallback(() => {
    setReadingModeState((v) => !v)
  }, [])

  useEffect(() => {
    document.documentElement.classList.toggle('reading-mode', readingMode)
    return () => document.documentElement.classList.remove('reading-mode')
  }, [readingMode])

  useEffect(() => {
    if (!readingMode) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setReadingModeState(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [readingMode])

  const value = useMemo(
    () => ({ readingMode, setReadingMode, toggleReadingMode }),
    [readingMode, setReadingMode, toggleReadingMode],
  )

  return (
    <ReadingModeContext.Provider value={value}>
      {children}
    </ReadingModeContext.Provider>
  )
}

export function useReadingMode() {
  const ctx = useContext(ReadingModeContext)
  if (!ctx) {
    throw new Error('useReadingMode must be used within ReadingModeProvider')
  }
  return ctx
}
