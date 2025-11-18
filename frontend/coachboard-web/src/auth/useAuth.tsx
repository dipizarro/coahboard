import { useAuthCtx } from './AuthContext'

export function useAuth() {
  const ctx = useAuthCtx()

  return {
    ...ctx,
    isAuthenticated: Boolean(ctx.token),
    role: ctx.user?.role ?? null,
    coachId: ctx.user?.coachId ?? null,
    email: ctx.user?.email ?? null,
  }
}
