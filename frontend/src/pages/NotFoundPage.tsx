import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";

export default function NotFoundPage() {
  return (
    <div className="container mx-auto flex min-h-[60vh] flex-col items-center justify-center px-4 text-center">
      <h1 className="text-6xl font-bold text-muted-foreground">404</h1>
      <p className="mt-4 text-xl text-muted-foreground">Page not found</p>
      <Button asChild className="mt-8">
        <Link to="/">Go Home</Link>
      </Button>
    </div>
  );
}
