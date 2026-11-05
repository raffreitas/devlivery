import { Card, CardContent, CardDescription, CardHeader } from "./ui/card";

export function StatTile({
  label,
  value,
  foot,
  emphasis = false,
}: {
  label: string;
  value: string;
  foot: string;
  emphasis?: boolean;
}) {
  return (
    <Card size="sm" className="gap-1 rounded-lg bg-background shadow-none">
      <CardHeader>
        <CardDescription className="flex items-center gap-2">
          {emphasis && (
            <span aria-hidden="true" className="size-2 rounded-sm bg-primary" />
          )}
          {label}
        </CardDescription>
        <p className="text-metric leading-tight font-semibold tabular-nums">
          {value}
        </p>
      </CardHeader>
      <CardContent>
        <p className="text-meta text-muted-foreground">{foot}</p>
      </CardContent>
    </Card>
  );
}
