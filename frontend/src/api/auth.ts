import axios from 'axios'
import { API } from '../api'

export interface Account {
  userId: string
  email: string
  name: string
  organizationId: string
  organizationName: string
  organizationType: 'Agency' | 'Contractor'
  role: 'AgencyAdmin' | 'AgencyStaff' | 'Contractor'
}
export interface Session { accessToken: string; expiresAt: string; account: Account }
export async function signIn(email: string, password: string) {
  return (await API.getClient().post<Session>('/auth/login', { email, password })).data
}
export async function registerAgency(email: string, password: string, name: string, agencyName: string) {
  return (await API.getClient().post<Session>('/auth/register', { email, password, name, agencyName })).data
}
export async function getAccount() { return (await API.getClient().get<Account>('/auth/me')).data }
export function errorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 429) return 'Too many attempts. Please wait a minute and try again.'
    if (error.response?.data?.errors) return Object.values(error.response.data.errors).flat().join(' ')
    if (error.response?.data?.title && error.response.status < 500) return String(error.response.data.title)
    if (error.response?.status === 401) return 'Your session has expired. Please sign in again.'
  }
  return 'Unable to connect. Please check that the API and PostgreSQL are running and try again.'
}
