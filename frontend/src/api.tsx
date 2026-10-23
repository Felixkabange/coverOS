import axios, { type AxiosInstance } from 'axios'

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5170'
const baseURL = `${configuredBaseUrl.replace(/\/+$/, '')}/api`

let client: AxiosInstance | undefined
let accessToken: string | undefined

function getClient(): AxiosInstance {
  if (!client) {
    client = axios.create({
      baseURL,
      headers: { Accept: 'application/json' },
      timeout: 15_000,
    })

    client.interceptors.request.use((config) => {
      if (accessToken) {
        config.headers.Authorization = `Bearer ${accessToken}`
      }

      return config
    })
  }

  return client
}

// This is a shared API utility rather than a component/HMR boundary.
// eslint-disable-next-line react-refresh/only-export-components
export const API = {
  getClient,
  setAccessToken(token: string | undefined) {
    accessToken = token
  },
  clearAccessToken() {
    accessToken = undefined
  },
}

export default API