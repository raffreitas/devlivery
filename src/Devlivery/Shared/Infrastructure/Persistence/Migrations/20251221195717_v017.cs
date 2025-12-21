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
                            select uuidv7() into company_expense_category_id;
                            select uuidv7() into supplier_category_id;
            
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
                            values (uuidv7(), 'Aluguel', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Água', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Luz', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Internet', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Telefone', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Impostos', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Salários', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Manutenção', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Marketing', true, rec, now(), now(), company_expense_category_id),
                                   (uuidv7(), 'Outros', true, rec, now(), now(), company_expense_category_id);
            
                            -- Subcategories for Supplier
                            insert into public.expense_categories(id,
                                                                  name,
                                                                  is_active,
                                                                  establishment_id,
                                                                  created_at,
                                                                  updated_at,
                                                                  parent_category_id)
                            values (uuidv7(), 'Alimentos', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Bebidas', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Embalagens', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Equipamentos', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Produtos de Limpeza', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Matéria Prima', true, rec, now(), now(), supplier_category_id),
                                   (uuidv7(), 'Outros', true, rec, now(), now(), supplier_category_id);
            
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
