import { Filter, Search } from "lucide-react";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
} from "@/shared/components/ui/select";

interface ProductsFiltersContentProps {
  searchTerm: string;
  filterCategory: string;
  categories: string[];
  onSearchChange: (value: string) => void;
  onCategoryChange: (value: string) => void;
}

export function ProductsFiltersContent({
  searchTerm,
  filterCategory,
  categories,
  onSearchChange,
  onCategoryChange,
}: ProductsFiltersContentProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-2">
        <Label>Buscar</Label>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <Input
            type="text"
            placeholder="Buscar produtos por nome..."
            value={searchTerm}
            onChange={(e) => onSearchChange(e.target.value)}
            className="pl-9 transition-colors"
          />
        </div>
      </div>

      <div className="flex flex-col gap-2">
        <Label>Categoria</Label>
        <Select onValueChange={onCategoryChange} value={filterCategory}>
          <SelectTrigger className="w-full cursor-pointer">
            <div className="flex items-center gap-2 text-muted-foreground">
              <Filter className="w-4 h-4" />
              <span className="truncate text-foreground">
                {filterCategory === "all" ? "Todas" : filterCategory}
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
  );
}
