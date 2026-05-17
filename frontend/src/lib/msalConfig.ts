// ---------------------------------------------------------------------------
// MSAL configuration for Microsoft Entra ID (admin authentication).
//
// Environment variables:
//   VITE_AZURE_AD_CLIENT_ID   — App Registration (SPA) client ID
//   VITE_AZURE_AD_TENANT_ID   — Entra ID tenant ID
//   VITE_AZURE_AD_API_SCOPE   — API scope, e.g. "api://<client-id>/access_as_admin"
//
// In development, when VITE_AZURE_AD_CLIENT_ID is not set, the auth provider
// falls back to a mock / dev mode so the app works without an Entra ID tenant.
// ---------------------------------------------------------------------------

import { type Configuration, LogLevel } from "@azure/msal-browser";

const clientId = import.meta.env.VITE_AZURE_AD_CLIENT_ID ?? "";
const tenantId = import.meta.env.VITE_AZURE_AD_TENANT_ID ?? "";

/** Whether real Entra ID authentication is configured. */
export const isMsalConfigured = clientId !== "" && tenantId !== "";

/** MSAL configuration for the SPA. */
export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
      piiLoggingEnabled: false,
    },
  },
};

/** Scopes requested when acquiring tokens for the backend API. */
export const apiScopes: string[] = import.meta.env.VITE_AZURE_AD_API_SCOPE
  ? [import.meta.env.VITE_AZURE_AD_API_SCOPE as string]
  : [];

/** Login request configuration. */
export const loginRequest = {
  scopes: ["openid", "profile", "email", ...apiScopes],
};
