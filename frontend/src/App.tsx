import { lazy, Suspense } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './components/AppShell'
import { ProtectedRoute } from './components/ProtectedRoute'
import { Spinner } from './components/ui'
import { AuthProvider } from './lib/auth'
import { FeaturesProvider } from './lib/features'
import { ReadingModeProvider } from './lib/readingMode'
import { ThemeProvider } from './lib/theme'

const ArticlePage = lazy(() =>
  import('./pages/ArticlePage').then((m) => ({ default: m.ArticlePage })),
)
const AuthPage = lazy(() =>
  import('./pages/AuthPage').then((m) => ({ default: m.AuthPage })),
)
const HomePage = lazy(() =>
  import('./pages/HomePage').then((m) => ({ default: m.HomePage })),
)
const LibraryPage = lazy(() =>
  import('./pages/LibraryPage').then((m) => ({ default: m.LibraryPage })),
)
const SearchPage = lazy(() =>
  import('./pages/SearchPage').then((m) => ({ default: m.SearchPage })),
)
const TrailsPage = lazy(() =>
  import('./pages/TrailsPage').then((m) => ({ default: m.TrailsPage })),
)

function RouteFallback() {
  return (
    <div className="flex min-h-[40vh] items-center justify-center">
      <Spinner />
    </div>
  )
}

export default function App() {
  return (
    <ThemeProvider>
      <ReadingModeProvider>
        <AuthProvider>
          <FeaturesProvider>
            <BrowserRouter>
              <Suspense fallback={<RouteFallback />}>
                <Routes>
                  <Route path="entrar" element={<AuthPage />} />
                  <Route element={<ProtectedRoute />}>
                    <Route element={<AppShell />}>
                      <Route index element={<HomePage />} />
                      <Route path="buscar" element={<SearchPage />} />
                      <Route path="artigo/:id" element={<ArticlePage />} />
                      <Route path="trilhas" element={<TrailsPage />} />
                      <Route path="biblioteca" element={<LibraryPage />} />
                      <Route path="*" element={<Navigate to="/" replace />} />
                    </Route>
                  </Route>
                </Routes>
              </Suspense>
            </BrowserRouter>
          </FeaturesProvider>
        </AuthProvider>
      </ReadingModeProvider>
    </ThemeProvider>
  )
}
