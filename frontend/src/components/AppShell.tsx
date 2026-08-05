import { NavLink, Outlet } from 'react-router-dom'
import { useState } from 'react'
import {
  Books,
  House,
  MagnifyingGlass,
  Moon,
  Path,
  Sun,
  SignOut,
  UserCircle,
} from '@phosphor-icons/react'
import { useAuth } from '../lib/auth'
import { useFeatures } from '../lib/features'
import { useReadingMode } from '../lib/readingMode'
import { useTheme } from '../lib/theme'
import { IconButton } from './ui'

const allLinks = [
  { to: '/', label: 'Início', icon: House, end: true, requiresAi: false },
  { to: '/buscar', label: 'Buscar', icon: MagnifyingGlass, requiresAi: false },
  { to: '/trilhas', label: 'Trilhas', icon: Path, requiresAi: true },
  { to: '/biblioteca', label: 'Biblioteca', icon: Books, requiresAi: false },
]

export function AppShell() {
  const { theme, toggle } = useTheme()
  const { readingMode } = useReadingMode()
  const { user, logout, isGuest } = useAuth()
  const { aiEnabled } = useFeatures()
  const [accountOpen, setAccountOpen] = useState(false)
  const links = allLinks.filter((link) => !link.requiresAi || aiEnabled)

  return (
    <div className={`min-h-screen ${readingMode ? 'is-reading' : ''}`}>
      {!readingMode && isGuest && (
        <div
          role="status"
          className="border-b border-signal/25 bg-signal/10 px-4 py-2 text-center text-xs text-ink-soft sm:text-sm"
        >
          Modo visitante: ótimo pra explorar. Crie uma conta para salvar favoritos e histórico só seus.
          <NavLink
            to="/entrar"
            onClick={logout}
            className="ml-2 font-medium text-accent-ink underline-offset-2 hover:underline"
          >
            Criar conta
          </NavLink>
        </div>
      )}
      {!readingMode && (
        <header className="sticky top-0 z-40 border-b border-line/80 bg-canvas/80 backdrop-blur-md">
          <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-3 sm:px-6">
            <NavLink to="/" className="group flex items-baseline gap-2">
              <span className="font-display text-xl font-semibold tracking-tight text-ink sm:text-2xl">
                Acervo
              </span>
              <span className="hidden text-xs text-muted sm:inline">
                biblioteca de descoberta
              </span>
            </NavLink>

            <nav className="hidden items-center gap-1 md:flex" aria-label="Principal">
              {links.map(({ to, label, icon: Icon, end }) => (
                <NavLink
                  key={to}
                  to={to}
                  end={end}
                  className={({ isActive }) =>
                    `inline-flex items-center gap-2 rounded-xl px-3 py-2 text-sm transition ${
                      isActive
                        ? 'bg-accent text-accent-fg'
                        : 'text-muted hover:bg-surface-2 hover:text-ink'
                    }`
                  }
                >
                  <Icon size={18} weight="duotone" />
                  {label}
                </NavLink>
              ))}
            </nav>

            <div className="flex items-center gap-1">
              <IconButton
                label={theme === 'dark' ? 'Ativar tema claro' : 'Ativar tema escuro'}
                onClick={toggle}
              >
                {theme === 'dark' ? (
                  <Sun size={20} weight="duotone" />
                ) : (
                  <Moon size={20} weight="duotone" />
                )}
              </IconButton>
              <div className="relative">
                <IconButton
                  label="Acessar minha conta"
                  onClick={() => setAccountOpen((value) => !value)}
                >
                  <UserCircle size={22} weight="duotone" />
                </IconButton>
                {accountOpen && (
                  <div className="absolute right-0 mt-2 w-64 rounded-2xl border border-line bg-surface p-2 shadow-[var(--shadow-lift)]">
                    <div className="border-b border-line px-3 py-2">
                      <p className="truncate text-sm font-medium text-ink">
                        {isGuest ? 'Visitante' : user?.displayName || 'Minha conta'}
                      </p>
                      <p className="truncate text-xs text-muted">
                        {isGuest ? 'Modo demo compartilhado' : user?.email}
                      </p>
                    </div>
                    <NavLink
                      to="/biblioteca"
                      onClick={() => setAccountOpen(false)}
                      className="mt-1 flex items-center gap-2 rounded-xl px-3 py-2 text-sm text-ink-soft hover:bg-surface-2"
                    >
                      <Books size={18} weight="duotone" />
                      Histórico e recomendações
                    </NavLink>
                    <button
                      type="button"
                      onClick={logout}
                      className="flex w-full items-center gap-2 rounded-xl px-3 py-2 text-sm text-danger hover:bg-danger/10"
                    >
                      <SignOut size={18} weight="duotone" />
                      Sair
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </header>
      )}

      <main
        className={
          readingMode
            ? 'mx-auto max-w-3xl px-4 py-6 sm:px-6'
            : 'mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10'
        }
      >
        <Outlet />
      </main>

      {!readingMode && (
        <>
          <nav
            className="fixed inset-x-0 bottom-0 z-40 border-t border-line bg-canvas/95 backdrop-blur-md md:hidden"
            aria-label="Mobile"
          >
            <div className="mx-auto grid max-w-lg grid-cols-4 gap-1 px-2 py-2 pb-[max(0.5rem,env(safe-area-inset-bottom))]">
              {links.map(({ to, label, icon: Icon, end }) => (
                <NavLink
                  key={to}
                  to={to}
                  end={end}
                  className={({ isActive }) =>
                    `flex flex-col items-center gap-1 rounded-xl px-2 py-2 text-[11px] ${
                      isActive ? 'text-accent-ink' : 'text-muted'
                    }`
                  }
                >
                  <Icon size={22} weight="duotone" />
                  {label}
                </NavLink>
              ))}
            </div>
          </nav>
          <div className="h-20 md:hidden" />
        </>
      )}
    </div>
  )
}
