import type { ReactNode } from "react";
import { QueryState } from "./query-state";

interface RegionQuery {
  data: unknown;
  isPending: boolean;
  error: Error | null;
  refetch: () => Promise<unknown>;
}

export function QueryRegion({
  queries,
  children,
}: {
  queries: readonly RegionQuery[];
  children: ReactNode;
}) {
  return (
    <QueryState
      isPending={queries.some((query) => query.isPending)}
      error={queries.find((query) => query.error)?.error ?? null}
      hasData={queries.every((query) => query.data !== undefined)}
      refetch={() =>
        Promise.allSettled(queries.map((query) => query.refetch()))
      }
    >
      {children}
    </QueryState>
  );
}
