// Auth stub — will be replaced with MSAL integration
// Provides a placeholder for admin authentication

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  roles: string[];
}

export function isAuthenticated(): boolean {
  // Stub: always returns true in development
  return true;
}

export function getCurrentUser(): AuthUser | null {
  // Stub: returns a mock admin user
  return {
    id: 'dev-user',
    name: 'Dev Admin',
    email: 'admin@devconf.local',
    roles: ['admin'],
  };
}

export function getAccessToken(): string | null {
  // Stub: returns a mock token
  return 'dev-token';
}
