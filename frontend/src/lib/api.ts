// ---------------------------------------------------------------------------
// API client — generic fetch wrapper for the backend REST API.
//
// Features:
//  • Automatic JSON serialization / deserialization
//  • Typed error class (`ApiError`) with status, statusText, and body
//  • Auth-token injection for admin routes via the `AuthProvider`
//  • Works with TanStack Query for loading / error state management
// ---------------------------------------------------------------------------

const API_BASE = import.meta.env.VITE_API_URL ?? '/api/v1';

// ---------------------------------------------------------------------------
// Error type
// ---------------------------------------------------------------------------

export class ApiError extends Error {
  status: number;
  statusText: string;

  constructor(
    status: number,
    statusText: string,
    message?: string,
  ) {
    super(message ?? `${status} ${statusText}`);
    this.name = 'ApiError';
    this.status = status;
    this.statusText = statusText;
  }
}

// ---------------------------------------------------------------------------
// Token accessor — set by the AuthProvider at startup so the API client can
// attach Bearer tokens without importing React hooks directly.
// ---------------------------------------------------------------------------

type TokenAccessor = () => Promise<string | null>;

let _getAccessToken: TokenAccessor | null = null;

/**
 * Called once by `AuthProvider` to wire up token retrieval.
 * This avoids a circular dependency between the API client and React context.
 */
export function setTokenAccessor(accessor: TokenAccessor): void {
  _getAccessToken = accessor;
}

// ---------------------------------------------------------------------------
// Internal helpers
// ---------------------------------------------------------------------------

async function authHeaders(): Promise<Record<string, string>> {
  if (!_getAccessToken) return {};
  const token = await _getAccessToken();
  if (!token) return {};
  return { Authorization: `Bearer ${token}` };
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const message = await response.text().catch(() => undefined);
    throw new ApiError(response.status, response.statusText, message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export const api = {
  async get<T>(path: string): Promise<T> {
    const auth = await authHeaders();
    const response = await fetch(`${API_BASE}${path}`, {
      headers: { ...auth },
    });
    return handleResponse<T>(response);
  },

  async post<T>(path: string, body?: unknown): Promise<T> {
    const auth = await authHeaders();
    const response = await fetch(`${API_BASE}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...auth },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse<T>(response);
  },

  async put<T>(path: string, body: unknown): Promise<T> {
    const auth = await authHeaders();
    const response = await fetch(`${API_BASE}${path}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json', ...auth },
      body: JSON.stringify(body),
    });
    return handleResponse<T>(response);
  },

  async delete<T>(path: string): Promise<T> {
    const auth = await authHeaders();
    const response = await fetch(`${API_BASE}${path}`, {
      method: 'DELETE',
      headers: { ...auth },
    });
    return handleResponse<T>(response);
  },

  async patch<T>(path: string, body?: unknown): Promise<T> {
    const auth = await authHeaders();
    const response = await fetch(`${API_BASE}${path}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json', ...auth },
      body: body ? JSON.stringify(body) : undefined,
    });
    return handleResponse<T>(response);
  },
};
