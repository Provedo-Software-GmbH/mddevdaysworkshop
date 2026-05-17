// Auth stub — will be replaced with @azure/msal-react (Entra ID) for admin
// and @azure/msal-browser (Entra External Identities) for customers.
// Provides the proper API surface so all consumers can import from here now
// and MSAL integration is a drop-in replacement later.

import {
  createContext,
  useContext,
  useState,
  useCallback,
  useEffect,
  type ReactNode,
  type PropsWithChildren,
} from "react";
import { Navigate, useLocation } from "react-router-dom";
import { setTokenAccessor } from "@/lib/api";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  roles: string[];
}

export interface AuthContextValue {
  /** The currently signed-in user, or `null` when unauthenticated. */
  user: AuthUser | null;
  /** Whether an authentication check is in progress. */
  isLoading: boolean;
  /** Whether the user is currently authenticated. */
  isAuthenticated: boolean;
  /** Sign in (stub: immediately sets the mock user). */
  login: () => Promise<void>;
  /** Sign out (stub: clears the mock user). */
  logout: () => Promise<void>;
  /** Retrieve an access token for API calls (stub: returns a mock token). */
  getAccessToken: () => Promise<string | null>;
}

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------

const AuthContext = createContext<AuthContextValue | null>(null);

// ---------------------------------------------------------------------------
// Mock data (replaced by MSAL at integration time)
// ---------------------------------------------------------------------------

const MOCK_USER: AuthUser = {
  id: "dev-user",
  name: "Dev Admin",
  email: "admin@devconf.local",
  roles: ["admin"],
};

const MOCK_TOKEN = "dev-token";

// ---------------------------------------------------------------------------
// Provider
// ---------------------------------------------------------------------------

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(MOCK_USER);
  const [isLoading] = useState(false);

  const login = useCallback(async () => {
    // Stub: immediately authenticate with the mock user.
    // Replace with msalInstance.loginPopup() / loginRedirect().
    setUser(MOCK_USER);
  }, []);

  const logout = useCallback(async () => {
    // Stub: clear the user.
    // Replace with msalInstance.logoutPopup() / logoutRedirect().
    setUser(null);
  }, []);

  const getAccessToken = useCallback(async (): Promise<string | null> => {
    // Stub: return a mock token when authenticated.
    // Replace with msalInstance.acquireTokenSilent().
    return user ? MOCK_TOKEN : null;
  }, [user]);

  // Wire token accessor into the API client so it can attach Bearer tokens.
  useEffect(() => {
    setTokenAccessor(getAccessToken);
  }, [getAccessToken]);

  const value: AuthContextValue = {
    user,
    isLoading,
    isAuthenticated: user !== null,
    login,
    logout,
    getAccessToken,
  };

  return <AuthContext value={value}>{children}</AuthContext>;
}

// ---------------------------------------------------------------------------
// Hook
// ---------------------------------------------------------------------------

/**
 * Access the current auth state and helpers.
 * Must be used inside an `<AuthProvider>`.
 */
export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an <AuthProvider>");
  }
  return ctx;
}

// ---------------------------------------------------------------------------
// Route guard
// ---------------------------------------------------------------------------

/**
 * Wraps child routes and redirects to `/` if the user is not authenticated.
 * Shows nothing while the auth check is in progress.
 */
export function ProtectedRoute({ children }: PropsWithChildren) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return null; // or a loading spinner
  }

  if (!isAuthenticated) {
    return <Navigate to="/" state={{ from: location }} replace />;
  }

  return children;
}

// ---------------------------------------------------------------------------
// Legacy helpers (kept for backwards compatibility during migration)
// ---------------------------------------------------------------------------

/** @deprecated Use `useAuth().isAuthenticated` instead. */
export function isAuthenticated(): boolean {
  return true;
}

/** @deprecated Use `useAuth().user` instead. */
export function getCurrentUser(): AuthUser | null {
  return MOCK_USER;
}

/** @deprecated Use `useAuth().getAccessToken()` instead. */
export function getAccessTokenSync(): string | null {
  return MOCK_TOKEN;
}
