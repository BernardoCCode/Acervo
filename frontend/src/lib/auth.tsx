import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { api, ApiError } from './api'
import type { AuthUser } from './types'

export const GUEST_EMAIL = 'guest@acervo.local'

interface AuthContextValue {
  user: AuthUser | null
  loading: boolean
  isGuest: boolean
  login: (email: string, password: string, rememberMe?: boolean) => Promise<void>
  register: (
    email: string,
    password: string,
    displayName?: string,
    rememberMe?: boolean,
  ) => Promise<void>
  enterAsGuest: () => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    const token = localStorage.getItem('acervo-token')
    if (!token) {
      setLoading(false)
      return
    }
    void api
      .me()
      .then((current) => {
        if (!cancelled) setUser(current)
      })
      .catch((error) => {
        if (!cancelled && error instanceof ApiError && error.status === 401) {
          localStorage.removeItem('acervo-token')
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      loading,
      isGuest: user?.email === GUEST_EMAIL,
      async login(email, password, rememberMe = false) {
        const result = await api.login(email, password, rememberMe)
        localStorage.setItem('acervo-token', result.token)
        setUser(result.user)
      },
      async register(email, password, displayName, rememberMe = false) {
        const result = await api.register(email, password, displayName, rememberMe)
        localStorage.setItem('acervo-token', result.token)
        setUser(result.user)
      },
      async enterAsGuest() {
        const result = await api.guest()
        localStorage.setItem('acervo-token', result.token)
        setUser(result.user)
      },
      logout() {
        localStorage.removeItem('acervo-token')
        setUser(null)
      },
    }),
    [loading, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside AuthProvider')
  return value
}
