import { useEffect, useState } from 'react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../lib/auth'
import { Spinner } from './ui'

export function ProtectedRoute() {
  const { user, loading } = useAuth()
  const location = useLocation()
  const [slow, setSlow] = useState(false)

  useEffect(() => {
    if (!loading) {
      setSlow(false)
      return
    }
    const id = window.setTimeout(() => setSlow(true), 3000)
    return () => window.clearTimeout(id)
  }, [loading])

  if (loading) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-canvas px-6 text-center">
        <Spinner
          label={
            slow
              ? 'Acordando a API (plano free)… pode levar até ~1 min'
              : 'Carregando sua conta…'
          }
        />
        {slow && (
          <p className="max-w-sm text-sm text-muted">
            O frontend na Vercel já abriu; a API no Render dorme sem uso e demora
            na primeira requisição. Nas próximas aberturas fica bem mais rápido.
          </p>
        )}
      </div>
    )
  }

  if (!user) {
    return (
      <Navigate
        to="/entrar"
        replace
        state={{ from: `${location.pathname}${location.search}` }}
      />
    )
  }

  return <Outlet />
}
