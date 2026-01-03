import { Home } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";

export function NotFoundPage() {
  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="text-center">
        <h1 className="text-9xl sm:text-[12rem] font-bold text-primary mb-4">
          404
        </h1>
        <p className="text-lg sm:text-xl text-muted-foreground mb-8">
          Página não encontrada
        </p>
        <Button asChild size="lg">
          <Link to="/">
            <Home className="size-4" />
            Voltar para Home
          </Link>
        </Button>
      </div>
    </div>
  );
}
