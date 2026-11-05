import type { ReactNode } from "react";
import { Fragment } from "react";
import { Link } from "react-router-dom";
import { useIsMobile } from "../hooks/use-mobile";
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "./ui/breadcrumb";
import { Separator } from "./ui/separator";
import { SidebarTrigger, useSidebar } from "./ui/sidebar";

export function PageHeader({
  title,
  actions,
  ancestors = [],
}: {
  title: string;
  actions?: ReactNode;
  ancestors?: { label: string; href: string }[];
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
            {ancestors.map((item) => (
              <Fragment key={item.href}>
                <BreadcrumbItem>
                  <BreadcrumbLink render={<Link to={item.href} />}>
                    {item.label}
                  </BreadcrumbLink>
                </BreadcrumbItem>
                <BreadcrumbSeparator />
              </Fragment>
            ))}
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
