"use client"

import { useState, type ChangeEvent, type FormEvent, type ComponentType, type ReactNode } from "react"
import { Books, Envelope, Eye, EyeSlash, Lock, User } from "@phosphor-icons/react"

interface SignInCredentials {
  email: string
  password: string
  rememberMe: boolean
}

interface SignInProps {
  onSignIn?: (credentials: SignInCredentials) => void | Promise<void>
  onRegister?: (credentials: SignInCredentials & { displayName: string }) => void | Promise<void>
  onGuest?: () => void | Promise<void>
  forgotPasswordHref?: string
  error?: string | null
  loading?: boolean
}

const Card = ({ children, className = "" }: { children: ReactNode; className?: string }) => (
  <div className={`rounded-3xl border border-line bg-surface shadow-[var(--shadow-lift)] ${className}`}>
    {children}
  </div>
)

const FormHeader = ({ title, subtitle }: { title: string; subtitle: string }) => (
  <div className="space-y-3 text-center">
    <a href="/" className="inline-flex items-baseline gap-2" aria-label="Acervo, página inicial">
      <span className="font-display text-2xl font-semibold tracking-tight text-ink">Acervo</span>
      <span className="text-xs text-muted">biblioteca de descoberta</span>
    </a>
    <h1 className="font-display text-4xl font-semibold tracking-[-0.035em] text-ink">{title}</h1>
    <p className="text-muted">{subtitle}</p>
  </div>
)

interface InputFieldProps {
  id: string
  type: string
  label: string
  placeholder: string
  value: string
  onChange: (event: ChangeEvent<HTMLInputElement>) => void
  icon: ComponentType<{ className?: string }>
  autoComplete?: string
  required?: boolean
}

const InputField = ({
  id,
  type,
  label,
  placeholder,
  value,
  onChange,
  icon: Icon,
  autoComplete,
  required = false,
}: InputFieldProps) => (
  <div className="space-y-2">
    <label htmlFor={id} className="text-sm font-medium text-ink">
      {label}
    </label>
    <div className="relative">
      <Icon className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" aria-hidden />
      <input
        id={id}
        type={type}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        autoComplete={autoComplete}
        className="h-12 w-full rounded-2xl border border-line bg-canvas/55 pl-11 pr-4 text-ink shadow-[var(--shadow-soft)] outline-none transition placeholder:text-muted focus:border-accent focus:ring-2 focus:ring-accent/20"
        required={required}
      />
    </div>
  </div>
)

interface PasswordFieldProps {
  value: string
  onChange: (event: ChangeEvent<HTMLInputElement>) => void
  showPassword: boolean
  onTogglePassword: () => void
}

const PasswordField = ({
  value,
  onChange,
  showPassword,
  onTogglePassword,
}: PasswordFieldProps) => (
  <div className="space-y-2">
    <label htmlFor="password" className="text-sm font-medium text-ink">
      Senha
    </label>
    <div className="relative">
      <Lock aria-hidden="true" className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
      <input
        id="password"
        type={showPassword ? "text" : "password"}
        placeholder="Digite sua senha"
        value={value}
        onChange={onChange}
        autoComplete="current-password"
        className="h-12 w-full rounded-2xl border border-line bg-canvas/55 pl-11 pr-12 text-ink shadow-[var(--shadow-soft)] outline-none transition placeholder:text-muted focus:border-accent focus:ring-2 focus:ring-accent/20"
        required
      />
      <button
        type="button"
        onClick={onTogglePassword}
        className="absolute right-2 top-1/2 inline-flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-xl text-muted transition hover:bg-surface-2 hover:text-ink"
        aria-label={showPassword ? "Ocultar senha" : "Mostrar senha"}
        aria-pressed={showPassword}
      >
        {showPassword ? <EyeSlash aria-hidden="true" className="h-4 w-4" /> : <Eye aria-hidden="true" className="h-4 w-4" />}
      </button>
    </div>
  </div>
)

interface ButtonProps {
  type?: "button" | "submit"
  variant?: "primary" | "outline"
  onClick?: () => void
  children: ReactNode
  disabled?: boolean
}

const Button = ({
  type = "button",
  variant = "primary",
  onClick,
  children,
  disabled = false,
}: ButtonProps) => {
  const variants = {
    primary: "bg-accent text-accent-fg shadow-[var(--shadow-soft)] hover:brightness-110",
    outline: "border border-line bg-canvas/50 text-ink hover:border-accent hover:bg-accent-soft",
  }

  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`flex h-12 w-full items-center justify-center gap-2 rounded-2xl px-4 font-medium transition duration-150 active:scale-[0.99] disabled:cursor-not-allowed disabled:opacity-55 disabled:active:scale-100 ${variants[variant]}`}
    >
      {children}
    </button>
  )
}

const Divider = ({ label = "ou" }: { label?: string }) => (
  <div className="relative" role="separator">
    <div className="absolute inset-0 flex items-center">
      <div className="w-full border-t border-line" />
    </div>
    <div className="relative flex justify-center text-xs">
      <span className="bg-surface px-3 text-muted">{label}</span>
    </div>
  </div>
)

const ProgressDots = () => (
  <div className="flex justify-center gap-2 pt-3" aria-hidden="true">
    <span className="h-1.5 w-6 rounded-full bg-accent-fg/45" />
    <span className="h-1.5 w-6 rounded-full bg-accent-fg/70" />
    <span className="h-1.5 w-6 rounded-full bg-accent-fg" />
  </div>
)

const HeroSection = () => (
  <div className="relative z-10 max-w-lg text-center">
    <div className="mx-auto mb-7 inline-flex rounded-2xl border border-accent-fg/15 bg-accent-fg/10 p-4 text-accent-fg">
      <Books aria-hidden="true" className="h-10 w-10" weight="duotone" />
    </div>
    <h2 className="font-display text-4xl font-semibold tracking-[-0.035em] text-accent-fg xl:text-5xl">
      Sua pesquisa, em um só lugar.
    </h2>
    <p className="mx-auto mt-5 max-w-md text-lg leading-relaxed text-accent-fg/80">
      Descubra artigos, organize sua biblioteca e aprofunde o aprendizado com uma leitura feita para você.
    </p>
    <ProgressDots />
  </div>
)

const GradientBackground = () => (
  <aside className="relative hidden flex-1 overflow-hidden bg-accent lg:flex" aria-label="Sobre o Acervo">
    <div
      aria-hidden="true"
      className="absolute inset-0 opacity-80"
      style={{
        background:
          "radial-gradient(circle at 15% 15%, color-mix(in srgb, var(--glow-a) 40%, transparent), transparent 35%), radial-gradient(circle at 85% 75%, color-mix(in srgb, var(--signal) 24%, transparent), transparent 34%)",
      }}
    />
    <div aria-hidden="true" className="animate-auth-blob absolute -left-20 top-10 h-80 w-80 rounded-full bg-glow-a/25 blur-3xl" />
    <div aria-hidden="true" className="animate-auth-blob animation-delay-2000 absolute -right-24 bottom-5 h-96 w-96 rounded-full bg-signal/15 blur-3xl" />
    <div aria-hidden="true" className="absolute inset-x-0 bottom-0 h-1/3 border-t border-accent-fg/10 bg-ink/10 [clip-path:polygon(0_40%,100%_0,100%_100%,0_100%)]" />
    <div className="flex w-full items-center justify-center p-12">
      <HeroSection />
    </div>
  </aside>
)

export default function SignIn({
  onSignIn,
  onRegister,
  onGuest,
  forgotPasswordHref,
  error,
  loading = false,
}: SignInProps) {
  const [showPassword, setShowPassword] = useState(false)
  const [isRegistering, setIsRegistering] = useState(false)
  const [displayName, setDisplayName] = useState("")
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [rememberMe, setRememberMe] = useState(false)

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    if (isRegistering) {
      void onRegister?.({ email, password, rememberMe, displayName })
    } else {
      void onSignIn?.({ email, password, rememberMe })
    }
  }

  return (
    <div className="flex min-h-screen w-full bg-canvas lg:flex-row">
      <main className="flex flex-1 items-center justify-center px-4 py-10 sm:px-6 lg:px-10">
        <div className="page-enter w-full max-w-md space-y-8">
          <FormHeader
            title={isRegistering ? "Crie sua conta" : "Bem-vindo de volta"}
            subtitle={
              isRegistering
                ? "Salve seu histórico e receba recomendações personalizadas."
                : "Entre para continuar sua jornada de descoberta."
            }
          />

          <Card className="p-6 sm:p-8">
            <form onSubmit={handleSubmit} className="space-y-6">
              {isRegistering && (
                <InputField
                  id="displayName"
                  type="text"
                  label="Nome"
                  placeholder="Como você quer ser chamado?"
                  value={displayName}
                  onChange={(event) => setDisplayName(event.target.value)}
                  icon={User}
                  autoComplete="name"
                  required
                />
              )}
              <InputField
                id="email"
                type="email"
                label="E-mail"
                placeholder="nome@exemplo.com"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                icon={Envelope}
                autoComplete="email"
                required
              />

              <PasswordField
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                showPassword={showPassword}
                onTogglePassword={() => setShowPassword((visible) => !visible)}
              />

              <div className="flex items-center justify-between gap-4 text-sm">
                <label htmlFor="remember" className="flex cursor-pointer items-center gap-2 text-muted">
                  <input
                    id="remember"
                    type="checkbox"
                    checked={rememberMe}
                    onChange={(event) => setRememberMe(event.target.checked)}
                    className="h-4 w-4 rounded border-line accent-[var(--accent)]"
                  />
                  <span>Manter conectado</span>
                </label>
                {forgotPasswordHref ? (
                  <a href={forgotPasswordHref} className="font-medium text-accent-ink transition hover:opacity-75">
                    Esqueci a senha
                  </a>
                ) : (
                  <span className="text-muted/70" title="Recuperação de senha disponível em breve">
                    Esqueci a senha
                  </span>
                )}
              </div>

              {error && (
                <p role="alert" className="rounded-2xl border border-danger/30 bg-danger/10 px-4 py-3 text-sm text-danger">
                  {error}
                </p>
              )}
              <Button type="submit" disabled={loading}>
                {loading ? "Aguarde…" : isRegistering ? "Criar conta" : "Entrar"}
              </Button>
              {onGuest && (
                <>
                  <Divider label="ou explore sem conta" />
                  <Button
                    variant="outline"
                    onClick={() => void onGuest()}
                    disabled={loading}
                  >
                    Explorar sem conta
                  </Button>
                  <p className="text-center text-xs text-muted">
                    Modo visitante para demo — crie uma conta para guardar o seu histórico.
                  </p>
                </>
              )}
            </form>

            <p className="mt-7 text-center text-sm text-muted">
              Ainda não tem uma conta?{" "}
              <button
                type="button"
                className="font-medium text-accent-ink transition hover:opacity-75"
                onClick={() => setIsRegistering((value) => !value)}
              >
                {isRegistering ? "Entrar" : "Criar conta"}
              </button>
            </p>
          </Card>
        </div>
      </main>

      <GradientBackground />
    </div>
  )
}

export type { SignInCredentials, SignInProps }
