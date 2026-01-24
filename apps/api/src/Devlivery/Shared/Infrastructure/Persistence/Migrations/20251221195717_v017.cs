using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Devlivery.Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v017 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            """
            DO
            $$
                declare
                    rec                         UUID;
                    company_expense_category_id UUID;
                    supplier_category_id        UUID;
                begin
                    for rec in select id from public.establishments
                        loop
                            select gen_random_uuid() into company_expense_category_id;
                            select gen_random_uuid() into supplier_category_id;
            
                            insert into public.expense_categories(id,
                                                                  name,
                                                                  is_active,
                                                                  establishment_id,
                                                                  created_at,
                                                                  updated_at,
                                                                  parent_category_id)
                            values (company_expense_category_id, 'Despesas da Empresa', true, rec, now(), now(), null),
                                   (supplier_category_id, 'Fornecedor', true, rec, now(), now(), null);
            
                            -- Subcategories for Company Expense
                            insert into public.expense_categories(id,
                                                                  name,
                                                                  is_active,
                                                                  establishment_id,
                                                                  created_at,
                                                                  updated_at,
                                                                  parent_category_id)
                            values (gen_random_uuid(), 'Aluguel', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Água', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Luz', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Internet', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Telefone', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Impostos', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Salários', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Manutenção', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Marketing', true, rec, now(), now(), company_expense_category_id),
                                   (gen_random_uuid(), 'Outros', true, rec, now(), now(), company_expense_category_id);
            
                            -- Subcategories for Supplier
                            insert into public.expense_categories(id,
                                                                  name,
                                                                  is_active,
                                                                  establishment_id,
                                                                  created_at,
                                                                  updated_at,
                                                                  parent_category_id)
                            values (gen_random_uuid(), 'Alimentos', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Bebidas', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Embalagens', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Equipamentos', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Produtos de Limpeza', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Matéria Prima', true, rec, now(), now(), supplier_category_id),
                                   (gen_random_uuid(), 'Outros', true, rec, now(), now(), supplier_category_id);
            
                        end loop;
                end
            $$;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("truncate table public.expense_categories cascade;");
        }
    }
}