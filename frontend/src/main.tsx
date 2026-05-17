import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { initTelemetry } from './lib/telemetry'
import './index.css'
import App from './App.tsx'

// Initialize OpenTelemetry before rendering — traces fetch calls, document load,
// and provides a tracer for custom spans throughout the app.
initTelemetry();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
