import type { ReactNode } from "react";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "./ui/breadcrumb";

export function PageHeader({
  title,
  context,
  actions,
}: {
  title: string;
  context?: ReactNode;
  actions?: ReactNode;
}) {
  return (
    <header className="flex min-h-13 flex-wrap items-center justify-between gap-3 border-b px-4 py-2 md:px-6">
      <Breadcrumb aria-label="Contexto da página" className="min-w-0">
        <BreadcrumbList>
          <BreadcrumbItem>
            <h1 className="text-sm font-semibold text-foreground">{title}</h1>
          </BreadcrumbItem>
          {context && (
            <>
              <BreadcrumbSeparator>/</BreadcrumbSeparator>
              <BreadcrumbItem className="min-w-0">
                <BreadcrumbPage className="text-ui">{context}</BreadcrumbPage>
              </BreadcrumbItem>
            </>
          )}
        </BreadcrumbList>
      </Breadcrumb>
      {actions && (
        <div className="ml-auto flex shrink-0 items-center gap-2">
          {actions}
        </div>
      )}
    </header>
  );
}
