import { Component, type ErrorInfo, type ReactNode } from "react";
import { AlertTriangle, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { trackError, startSpan } from "@/lib/telemetry";

interface ErrorBoundaryProps {
  children: ReactNode;
  fallback?: ReactNode;
}

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    const span = startSpan("error.boundary", {
      "error.message": error.message,
      "error.componentStack": errorInfo.componentStack ?? "",
    });
    trackError(span, error);
    span.end();
  }

  handleRetry = () => {
    this.setState({ hasError: false, error: null });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback;

      return (
        <Card className="mx-auto mt-8 max-w-lg">
          <CardContent className="flex flex-col items-center gap-4 py-12 text-center">
            <AlertTriangle className="h-12 w-12 text-destructive" />
            <div>
              <p className="text-lg font-semibold">Something went wrong</p>
              <p className="mt-1 text-sm text-muted-foreground">
                {this.state.error?.message ?? "An unexpected error occurred."}
              </p>
            </div>
            <Button variant="outline" onClick={this.handleRetry}>
              <RotateCcw className="mr-2 h-4 w-4" />
              Try Again
            </Button>
          </CardContent>
        </Card>
      );
    }

    return this.props.children;
  }
}
