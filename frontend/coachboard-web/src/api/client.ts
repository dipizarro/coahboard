import axios from 'axios'
import { storage } from '../lib/storage'


export const api = axios.create({ baseURL: import.meta.env.VITE_API_BASE_URL })


api.interceptors.request.use((config) => {
const token = storage.get('token')
if (token) config.headers.Authorization = `Bearer ${token}`
return config
})


api.interceptors.response.use(
(r) => r,
(err) => {
if (err.response?.status === 401) {
// opcional: redirigir a login
storage.del('token')
window.location.href = '/login'
}
return Promise.reject(err)
}
)