using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Até aqui o CreatedAt não era preenchido e o Npgsql gravava DateTime.MinValue como -infinity.
            // Corrige todas as tabelas com created_at: usa updated_at quando existir (primeira data conhecida após a criação)
            // e, sem ele, a data desta migration. Linhas com data válida não são alteradas.
            migrationBuilder.Sql(@"
DO $$
DECLARE target record;
BEGIN
    FOR target IN
        SELECT c.table_name,
               EXISTS (
                   SELECT 1 FROM information_schema.columns u
                   WHERE u.table_schema = c.table_schema AND u.table_name = c.table_name AND u.column_name = 'updated_at'
               ) AS has_updated_at
        FROM information_schema.columns c
        JOIN information_schema.tables t
          ON t.table_schema = c.table_schema AND t.table_name = c.table_name AND t.table_type = 'BASE TABLE'
        WHERE c.table_schema = current_schema()
          AND c.column_name = 'created_at'
          AND c.data_type = 'timestamp with time zone'
    LOOP
        EXECUTE format(
            'UPDATE %I SET created_at = %s WHERE created_at = ''-infinity''',
            target.table_name,
            CASE WHEN target.has_updated_at THEN 'COALESCE(NULLIF(updated_at, ''-infinity''), now())' ELSE 'now()' END);
    END LOOP;
END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Sem reversão: voltar as datas para -infinity só reintroduziria o defeito.
        }
    }
}
