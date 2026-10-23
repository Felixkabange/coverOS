import API from '../api'

export type ApiHealth =
  | { status: 'checking' | 'offline' }
  | { status: 'healthy'; timestampUtc: string }

export async function getApiHealth(signal?: AbortSignal): Promise<ApiHealth> {
  const response = await API.getClient().get<{ status: string; timestampUtc: string }>(
    '/health',
    { signal },
  )
  const result = response.data
  return { status: 'healthy', timestampUtc: result.timestampUtc }
}