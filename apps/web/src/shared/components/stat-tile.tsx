import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "./ui/card";

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
    <Card size="sm">
      <CardHeader>
        <CardDescription className="flex items-center gap-2">
          {emphasis && (
            <span
              aria-hidden="true"
              className="size-2 shrink-0 rounded-sm bg-primary-text"
            />
          )}
          {label}
        </CardDescription>
        <CardTitle className="font-semibold tracking-tight tabular-nums group-data-[size=sm]/card:text-2xl">
          {value}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground">{foot}</p>
      </CardContent>
    </Card>
  );
}
