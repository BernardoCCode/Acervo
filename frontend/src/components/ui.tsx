import type { ButtonHTMLAttributes, InputHTMLAttributes, ReactNode } from 'react'

export function Button({
  variant = 'primary',
  className = '',
  children,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'ghost' | 'danger'
}) {
  const styles = {
    primary:
      'bg-accent text-accent-fg hover:brightness-110 shadow-[var(--shadow-soft)] disabled:opacity-50',
    secondary:
      'bg-surface-2 text-ink hover:bg-line/60 border border-line disabled:opacity-50',
    ghost: 'bg-transparent text-ink-soft hover:bg-surface-2 disabled:opacity-50',
    danger: 'bg-danger/15 text-danger hover:bg-danger/25 disabled:opacity-50',
  }[variant]

  return (
    <button
      className={`inline-flex cursor-pointer items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium transition duration-150 active:scale-[0.98] disabled:cursor-not-allowed disabled:active:scale-100 ${styles} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}

export function IconButton({
  className = '',
  label,
  children,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { label: string }) {
  return (
    <button
      aria-label={label}
      title={label}
      className={`inline-flex h-11 w-11 cursor-pointer items-center justify-center rounded-xl text-ink-soft transition hover:bg-surface-2 active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-50 ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}

export function Field({
  className = '',
  ...props
}: InputHTMLAttributes<HTMLInputElement>) {
  return (
    <input
      className={`w-full rounded-2xl border border-line bg-surface px-4 py-3.5 text-ink placeholder:text-muted/80 shadow-[var(--shadow-soft)] transition focus:border-accent ${className}`}
      {...props}
    />
  )
}

export function Panel({
  children,
  className = '',
}: {
  children: ReactNode
  className?: string
}) {
  return (
    <div
      className={`rounded-3xl border border-line bg-surface shadow-[var(--shadow-soft)] ${className}`}
    >
      {children}
    </div>
  )
}

export function Badge({
  children,
  tone = 'default',
}: {
  children: ReactNode
  tone?: 'default' | 'accent' | 'signal'
}) {
  const toneClass = {
    default: 'bg-surface-2 text-ink-soft',
    accent: 'bg-accent-soft text-accent-ink',
    signal: 'bg-signal-soft text-signal',
  }[tone]

  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium tracking-wide ${toneClass}`}
    >
      {children}
    </span>
  )
}

export function EmptyState({
  title,
  description,
  action,
}: {
  title: string
  description: string
  action?: ReactNode
}) {
  return (
    <div className="flex flex-col items-start gap-3 rounded-3xl border border-dashed border-line bg-surface/60 px-6 py-10">
      <h3 className="font-display text-xl font-semibold text-ink">{title}</h3>
      <p className="max-w-md text-sm leading-relaxed text-muted">{description}</p>
      {action}
    </div>
  )
}

export function Spinner({ label = 'Carregando…' }: { label?: string }) {
  return (
    <div className="flex items-center gap-3 text-sm text-muted" role="status">
      <span className="h-4 w-4 animate-spin rounded-full border-2 border-line border-t-accent" />
      {label}
    </div>
  )
}

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div
      role="alert"
      className="rounded-2xl border border-danger/30 bg-danger/10 px-4 py-3 text-sm text-danger"
    >
      {message}
    </div>
  )
}
