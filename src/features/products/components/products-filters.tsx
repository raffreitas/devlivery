import { Filter, Search } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/components/ui/select";

interface ProductsFiltersProps {
  searchTerm: string;
  filterCategory: string;
  categories: string[];
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
  onOpenFilters: () => void;
}

export function ProductsFilters({
  searchTerm,
  filterCategory,
  categories,
  onSearchChange,
  onCategoryChange,
  onOpenFilters,
}: ProductsFiltersProps) {
  // Contagem de filtros ativos
  const activeFiltersCount =
    (searchTerm ? 1 : 0) + (filterCategory !== "all" ? 1 : 0);

  return (
    <>
      {/* Mobile: Botão Filtros */}
      <div className="sm:hidden">
        <Button variant="secondary" onClick={onOpenFilters} className="w-full">
          <Filter className="w-4 h-4" />
          <span className="ml-2">Filtros</span>
          {activeFiltersCount > 0 && (
            <span className="ml-2 px-1.5 py-0.5 bg-orange-500 text-white text-xs font-bold rounded-full min-w-5 text-center">
              {activeFiltersCount}
            </span>
          )}
        </Button>
      </div>

      {/* Desktop: Inline Filters */}
      <div className="hidden sm:block bg-card rounded-lg border border-border shadow-sm p-4">
        <div className="flex flex-col sm:flex-row gap-4 items-center">
          <div className="relative flex-1 w-full">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
            <Input
              type="text"
              placeholder="Buscar produtos por nome..."
              value={searchTerm}
              onChange={(e) => onSearchChange(e.target.value)}
              className="pl-9 transition-colors"
            />
          </div>

          <div className="w-full sm:w-[200px]">
            <Select onValueChange={onCategoryChange} value={filterCategory}>
              <SelectTrigger className="w-full">
                <div className="flex items-center gap-2 text-muted-foreground">
                  <Filter className="w-4 h-4" />
                  <span className="truncate text-foreground">
                    {filterCategory === "all"
                      ? "Todas Categorias"
                      : filterCategory}
                  </span>
                </div>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all" onSelect={() => onCategoryChange("all")}>
                  Todas as categorias
                </SelectItem>
                {categories.map((category) => (
                  <SelectItem
                    key={category}
                    value={category}
                    onSelect={() => onCategoryChange(category)}
                    className="cursor-pointer"
                  >
                    {category}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>
    </>
  );
}

