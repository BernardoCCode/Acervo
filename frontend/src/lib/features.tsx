import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api } from './api'

interface FeaturesContextValue {
  aiEnabled: boolean
  loading: boolean
}

const FeaturesContext = createContext<FeaturesContextValue | null>(null)

export function FeaturesProvider({ children }: { children: ReactNode }) {
  const [aiEnabled, setAiEnabled] = useState(false)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    void api
      .getFeatures()
      .then((features) => {
        if (!cancelled) setAiEnabled(features.aiEnabled)
      })
      .catch(() => {
        if (!cancelled) setAiEnabled(false)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  const value = useMemo(
    () => ({ aiEnabled, loading }),
    [aiEnabled, loading],
  )

  return (
    <FeaturesContext.Provider value={value}>{children}</FeaturesContext.Provider>
  )
}

export function useFeatures() {
  const value = useContext(FeaturesContext)
  if (!value) throw new Error('useFeatures must be used inside FeaturesProvider')
  return value
}
