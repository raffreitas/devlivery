import type { ReactNode } from "react";
import { useIsMobile } from "../hooks/use-mobile";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbList,
  BreadcrumbPage,
} from "./ui/breadcrumb";
import { Separator } from "./ui/separator";
import { SidebarTrigger, useSidebar } from "./ui/sidebar";

export function PageHeader({
  title,
  actions,
}: {
  title: string;
  actions?: ReactNode;
}) {
  const { open } = useSidebar();
  const isMobile = useIsMobile();
  return (
    <header className="flex min-h-13 flex-wrap items-center justify-between gap-3 border-b px-4 py-2 md:px-6">
      <div className="flex min-w-0 items-center gap-2">
        {!isMobile && (
          <>
            <SidebarTrigger
              className="-ml-1"
              aria-label={open ? "Recolher navegação" : "Expandir navegação"}
              aria-expanded={open}
            />
            <Separator
              orientation="vertical"
              className="mr-2 data-vertical:h-4 data-vertical:self-center"
            />
          </>
        )}
        <Breadcrumb aria-label="Breadcrumb" className="min-w-0">
          <BreadcrumbList>
            <BreadcrumbItem>
              <h1 className="text-sm font-semibold">
                <BreadcrumbPage>{title}</BreadcrumbPage>
              </h1>
            </BreadcrumbItem>
          </BreadcrumbList>
        </Breadcrumb>
      </div>
      {actions && (
        <div className="ml-auto flex shrink-0 items-center gap-2">
          {actions}
        </div>
      )}
    </header>
  );
}
