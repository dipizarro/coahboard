// src/api/client.ts
import axios from 'axios'
import { storage } from '../lib/storage'

export const apiBaseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5152'

export const api = axios.create({
  baseURL: apiBaseUrl,
})

const STORAGE_TOKEN_KEY = 'coachboard_token' // MISMA que en AuthContext

type UnauthorizedHandler = () => void
let unauthorizedHandler: UnauthorizedHandler | null = null

export function setUnauthorizedHandler(handler: UnauthorizedHandler | null) {
  unauthorizedHandler = handler
}

api.interceptors.request.use(config => {
  const token = storage.get(STORAGE_TOKEN_KEY)
  if (token) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  response => response,
  error => {
    if (error?.response?.status === 401 && typeof unauthorizedHandler === 'function') {
      unauthorizedHandler()
    }
    return Promise.reject(error)
  },
)
