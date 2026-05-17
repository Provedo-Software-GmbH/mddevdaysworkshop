// ---------------------------------------------------------------------------
// Auth provider — wraps Microsoft Entra ID via MSAL for admin authentication.
//
// When MSAL is configured (VITE_AZURE_AD_CLIENT_ID set), this uses the real
// @azure/msal-react provider. In development without Entra ID credentials,
// it falls back to a mock user so the app remains functional.
// ---------------------------------------------------------------------------

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
import {
  PublicClientApplication,
  InteractionRequiredAuthError,
  type AccountInfo,
} from "@azure/msal-browser";
import {
  MsalProvider,
  useMsal,
  useIsAuthenticated,
} from "@azure/msal-react";
import { setTokenAccessor } from "@/lib/api";
import {
  msalConfig,
  loginRequest,
  apiScopes,
  isMsalConfigured,
} from "@/lib/msalConfig";

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
  /** Sign in via Entra ID (or mock in dev mode). */
  login: () => Promise<void>;
  /** Sign out. */
  logout: () => Promise<void>;
  /** Retrieve an access token for API calls. */
  getAccessToken: () => Promise<string | null>;
}

// ---------------------------------------------------------------------------
// Context
// ---------------------------------------------------------------------------

const AuthContext = createContext<AuthContextValue | null>(null);

// ---------------------------------------------------------------------------
// MSAL instance (singleton) — only created when configured
// ---------------------------------------------------------------------------

let msalInstance: PublicClientApplication | null = null;

function getMsalInstance(): PublicClientApplication {
  if (!msalInstance) {
    msalInstance = new PublicClientApplication(msalConfig);
  }
  return msalInstance;
}

// ---------------------------------------------------------------------------
// Helper: map MSAL AccountInfo to AuthUser
// ---------------------------------------------------------------------------

function accountToUser(account: AccountInfo): AuthUser {
  const roles =
    (account.idTokenClaims?.["roles"] as string[] | undefined) ?? [];
  return {
    id: account.localAccountId,
    name: account.name ?? "Unknown",
    email: account.username ?? "",
    roles,
  };
}

// ---------------------------------------------------------------------------
// Inner provider that uses MSAL hooks (must be inside MsalProvider)
// ---------------------------------------------------------------------------

function MsalAuthInner({ children }: { children: ReactNode }) {
  const { instance, accounts, inProgress } = useMsal();
  const isMsalAuthenticated = useIsAuthenticated();

  const account = accounts[0] ?? null;
  const user = account ? accountToUser(account) : null;
  const isLoading = inProgress !== "none";

  const login = useCallback(async () => {
    await instance.loginPopup(loginRequest);
  }, [instance]);

  const logout = useCallback(async () => {
    await instance.logoutPopup({ postLogoutRedirectUri: "/" });
  }, [instance]);

  const getAccessToken = useCallback(async (): Promise<string | null> => {
    if (!account) return null;
    try {
      const response = await instance.acquireTokenSilent({
        scopes: apiScopes,
        account,
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        const response = await instance.acquireTokenPopup({
          scopes: apiScopes,
        });
        return response.accessToken;
      }
      console.error("Failed to acquire token:", error);
      return null;
    }
  }, [instance, account]);

  useEffect(() => {
    setTokenAccessor(getAccessToken);
  }, [getAccessToken]);

  const value: AuthContextValue = {
    user,
    isLoading,
    isAuthenticated: isMsalAuthenticated,
    login,
    logout,
    getAccessToken,
  };

  return <AuthContext value={value}>{children}</AuthContext>;
}

// ---------------------------------------------------------------------------
// Mock data (used in development without Entra ID)
// ---------------------------------------------------------------------------

const MOCK_USER: AuthUser = {
  id: "dev-user",
  name: "Dev Admin",
  email: "admin@devconf.local",
  roles: ["Admin"],
};

const MOCK_TOKEN = "dev-token";

// ---------------------------------------------------------------------------
// Dev-mode provider (no real MSAL)
// ---------------------------------------------------------------------------

function DevAuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(MOCK_USER);

  const login = useCallback(async () => {
    setUser(MOCK_USER);
  }, []);

  const logout = useCallback(async () => {
    setUser(null);
  }, []);

  const getAccessToken = useCallback(
    async (): Promise<string | null> => (user ? MOCK_TOKEN : null),
    [user],
  );

  useEffect(() => {
    setTokenAccessor(getAccessToken);
  }, [getAccessToken]);

  const value: AuthContextValue = {
    user,
    isLoading: false,
    isAuthenticated: user !== null,
    login,
    logout,
    getAccessToken,
  };

  return <AuthContext value={value}>{children}</AuthContext>;
}

// ---------------------------------------------------------------------------
// Public AuthProvider — delegates to MSAL or dev-mode
// ---------------------------------------------------------------------------

export function AuthProvider({ children }: { children: ReactNode }) {
  if (!isMsalConfigured) {
    return <DevAuthProvider>{children}</DevAuthProvider>;
  }

  const instance = getMsalInstance();
  return (
    <MsalProvider instance={instance}>
      <MsalAuthInner>{children}</MsalAuthInner>
    </MsalProvider>
  );
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
