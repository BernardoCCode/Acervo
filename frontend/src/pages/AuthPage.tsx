import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import SignIn from '../components/ui/signin-page'
import { useAuth } from '../lib/auth'

export function AuthPage() {
  const { user, login, register, enterAsGuest } = useAuth()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const navigate = useNavigate()
  const location = useLocation()
  const destination =
    (location.state as { from?: string } | null)?.from ?? '/'

  if (user) return <Navigate to={destination} replace />

  async function execute(action: () => Promise<void>) {
    setLoading(true)
    setError(null)
    try {
      await action()
      navigate(destination, { replace: true })
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Não foi possível entrar.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <SignIn
      loading={loading}
      error={error}
      onSignIn={({ email, password, rememberMe }) =>
        execute(() => login(email, password, rememberMe))
      }
      onRegister={({ email, password, displayName, rememberMe }) =>
        execute(() => register(email, password, displayName, rememberMe))
      }
      onGuest={() => execute(() => enterAsGuest())}
    />
  )
}
