import { apiBaseUrl } from '../api/client'

export function resolveAssetUrl(url?: string | null) {
  if (!url) return ''

  if (/^https?:\/\//i.test(url)) {
    return url
  }

  try {
    return new URL(url, apiBaseUrl).toString()
  } catch {
    return url
  }
}
