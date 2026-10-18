using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class IdentityAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy ownership is a business decision; never invent owners or discard employee credentials.
            // A populated installation needs a reviewed import/backfill before this evolution can run.
            migrationBuilder.Sql("""
                DO $identity_preflight$
                BEGIN
                    IF EXISTS (SELECT 1 FROM empresa)
                       OR EXISTS (SELECT 1 FROM funcionario)
                       OR EXISTS (SELECT 1 FROM cargo)
                       OR EXISTS (SELECT 1 FROM departamento) THEN
                        RAISE EXCEPTION 'IdentityAndSessions requires an empty store or an explicit reviewed legacy import. Existing data has not been modified.';
                    END IF;
                END $identity_preflight$;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_funcionario_cargo_role_id",
                table: "funcionario");

            migrationBuilder.DropForeignKey(
                name: "fk_funcionario_departamento_department_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_department_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_enterprise_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_role_id",
                table: "funcionario");

            migrationBuilder.DropColumn(
                name: "email",
                table: "funcionario");

            migrationBuilder.DropColumn(
                name: "password_hash",
                table: "funcionario");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "funcionario",
                newName: "position_id");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "funcionario",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<long>(
                name: "enterprise_membership_id",
                table: "funcionario",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "departamento",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "departamento",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "departamento",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "enterprise_id",
                table: "departamento",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "departamento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "normalized_name",
                table: "departamento",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "cliente",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "cliente",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<bool>(
                name: "email_confirmed",
                table: "cliente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "failed_login_count",
                table: "cliente",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_failed_login_at",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_until",
                table: "cliente",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "normalized_email",
                table: "cliente",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "security_version",
                table: "cliente",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "cargo",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "cargo",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "cargo",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "enterprise_id",
                table: "cargo",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "cargo",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "normalized_name",
                table: "cargo",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "ak_departamento_enterprise_id_id",
                table: "departamento",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_cliente_enterprise_id_id",
                table: "cliente",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.AddUniqueConstraint(
                name: "ak_cargo_enterprise_id_id",
                table: "cargo",
                columns: new[] { "enterprise_id", "id" });

            migrationBuilder.CreateTable(
                name: "cliente_refresh_token",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_version_at_issue = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    family_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_refresh_token_cliente_enterprise_id_customer_id",
                        columns: x => new { x.enterprise_id, x.customer_id },
                        principalTable: "cliente",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cliente_token_acao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    security_version_at_issue = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente_token_acao", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_token_acao_cliente_enterprise_id_customer_id",
                        columns: x => new { x.enterprise_id, x.customer_id },
                        principalTable: "cliente",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dominio_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    normalized_host = table.Column<string>(type: "character varying(253)", maxLength: 253, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    verification_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dominio_empresa", x => x.id);
                    table.ForeignKey(
                        name: "fk_dominio_empresa_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "papel_acesso",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_papel_acesso", x => x.id);
                    table.UniqueConstraint("ak_papel_acesso_enterprise_id_id", x => new { x.enterprise_id, x.id });
                    table.ForeignKey(
                        name: "fk_papel_acesso_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permissao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_membro_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_membro_empresa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    lockout_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failed_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    security_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "papel_acesso_permissao",
                columns: table => new
                {
                    access_role_id = table.Column<long>(type: "bigint", nullable: false),
                    permission_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_papel_acesso_permissao", x => new { x.access_role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_papel_acesso_permissao_papel_acesso_enterprise_id_access_ro~",
                        columns: x => new { x.enterprise_id, x.access_role_id },
                        principalTable: "papel_acesso",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_papel_acesso_permissao_permissao_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membro_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    kind_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    access_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membro_empresa", x => x.id);
                    table.UniqueConstraint("ak_membro_empresa_enterprise_id_id", x => new { x.enterprise_id, x.id });
                    table.ForeignKey(
                        name: "fk_membro_empresa_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membro_empresa_tipo_membro_empresa_kind_id",
                        column: x => x.kind_id,
                        principalTable: "tipo_membro_empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membro_empresa_usuario_user_id",
                        column: x => x.user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_refresh_token",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_membership_id = table.Column<long>(type: "bigint", nullable: true),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_version_at_issue = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    family_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_refresh_token_usuario_user_id",
                        column: x => x.user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_token_acao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    security_version_at_issue = table.Column<long>(type: "bigint", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario_token_acao", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_token_acao_usuario_user_id",
                        column: x => x.user_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "convite_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    invited_normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    invited_kind_id = table.Column<long>(type: "bigint", nullable: false),
                    issued_by_membership_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convite_empresa", x => x.id);
                    table.UniqueConstraint("ak_convite_empresa_enterprise_id_id", x => new { x.enterprise_id, x.id });
                    table.ForeignKey(
                        name: "fk_convite_empresa_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_convite_empresa_membro_empresa_enterprise_id_issued_by_memb~",
                        columns: x => new { x.enterprise_id, x.issued_by_membership_id },
                        principalTable: "membro_empresa",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "membro_empresa_papel_acesso",
                columns: table => new
                {
                    enterprise_membership_id = table.Column<long>(type: "bigint", nullable: false),
                    access_role_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membro_empresa_papel_acesso", x => new { x.enterprise_membership_id, x.access_role_id });
                    table.ForeignKey(
                        name: "fk_membro_empresa_papel_acesso_membro_empresa_enterprise_id_en~",
                        columns: x => new { x.enterprise_id, x.enterprise_membership_id },
                        principalTable: "membro_empresa",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_membro_empresa_papel_acesso_papel_acesso_enterprise_id_acce~",
                        columns: x => new { x.enterprise_id, x.access_role_id },
                        principalTable: "papel_acesso",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transferencia_propriedade",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    issued_by_membership_id = table.Column<long>(type: "bigint", nullable: false),
                    recipient_membership_id = table.Column<long>(type: "bigint", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transferencia_propriedade", x => x.id);
                    table.ForeignKey(
                        name: "fk_transferencia_propriedade_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transferencia_propriedade_membro_empresa_enterprise_id_issu~",
                        columns: x => new { x.enterprise_id, x.issued_by_membership_id },
                        principalTable: "membro_empresa",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transferencia_propriedade_membro_empresa_enterprise_id_reci~",
                        columns: x => new { x.enterprise_id, x.recipient_membership_id },
                        principalTable: "membro_empresa",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "convite_empresa_papel_acesso",
                columns: table => new
                {
                    enterprise_invitation_id = table.Column<long>(type: "bigint", nullable: false),
                    access_role_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_convite_empresa_papel_acesso", x => new { x.enterprise_invitation_id, x.access_role_id });
                    table.ForeignKey(
                        name: "fk_convite_empresa_papel_acesso_convite_empresa_enterprise_id_~",
                        columns: x => new { x.enterprise_id, x.enterprise_invitation_id },
                        principalTable: "convite_empresa",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_convite_empresa_papel_acesso_papel_acesso_enterprise_id_acc~",
                        columns: x => new { x.enterprise_id, x.access_role_id },
                        principalTable: "papel_acesso",
                        principalColumns: new[] { "enterprise_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissao",
                columns: new[] { "id", "code", "created_at", "description", "is_active", "module", "name", "updated_at" },
                values: new object[,]
                {
                    { 1L, "products.view", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar produtos", true, "Products", "Consultar produtos", null },
                    { 2L, "products.create", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Cadastrar produtos", true, "Products", "Cadastrar produtos", null },
                    { 3L, "products.edit", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Editar produtos", true, "Products", "Editar produtos", null },
                    { 4L, "products.change_price", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Alterar precos", true, "Products", "Alterar precos", null },
                    { 5L, "products.delete", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Excluir produtos", true, "Products", "Excluir produtos", null },
                    { 6L, "orders.view", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar pedidos", true, "Orders", "Consultar pedidos", null },
                    { 7L, "orders.manage", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar pedidos", true, "Orders", "Gerenciar pedidos", null },
                    { 8L, "customers.view", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar clientes", true, "Customers", "Consultar clientes", null },
                    { 9L, "customers.manage", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar clientes", true, "Customers", "Gerenciar clientes", null },
                    { 10L, "employees.manage", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar perfis funcionais", true, "Employees", "Gerenciar perfis funcionais", null },
                    { 11L, "drop.events.view", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Consultar drops", true, "Drop", "Consultar drops", null },
                    { 12L, "drop.events.manage", new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Gerenciar drops", true, "Drop", "Gerenciar drops", null }
                });

            migrationBuilder.InsertData(
                table: "tipo_membro_empresa",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Dono principal" },
                    { 2L, "Socio-proprietario" },
                    { 3L, "Funcionario" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_enterprise_id_department_id",
                table: "funcionario",
                columns: new[] { "enterprise_id", "department_id" });

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_enterprise_id_enterprise_membership_id",
                table: "funcionario",
                columns: new[] { "enterprise_id", "enterprise_membership_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_enterprise_id_position_id",
                table: "funcionario",
                columns: new[] { "enterprise_id", "position_id" });

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_enterprise_membership_id",
                table: "funcionario",
                column: "enterprise_membership_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departamento_enterprise_id_normalized_name",
                table: "departamento",
                columns: new[] { "enterprise_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_enterprise_id_normalized_email",
                table: "cliente",
                columns: new[] { "enterprise_id", "normalized_email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cargo_enterprise_id_normalized_name",
                table: "cargo",
                columns: new[] { "enterprise_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_refresh_token_customer_id_family_id",
                table: "cliente_refresh_token",
                columns: new[] { "customer_id", "family_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_refresh_token_enterprise_id_customer_id",
                table: "cliente_refresh_token",
                columns: new[] { "enterprise_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_refresh_token_family_expires_at",
                table: "cliente_refresh_token",
                column: "family_expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_refresh_token_token_hash",
                table: "cliente_refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cliente_token_acao_customer_id_expires_at",
                table: "cliente_token_acao",
                columns: new[] { "customer_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_token_acao_enterprise_id_customer_id",
                table: "cliente_token_acao",
                columns: new[] { "enterprise_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cliente_token_acao_token_hash",
                table: "cliente_token_acao",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_enterprise_id_invited_normalized_email",
                table: "convite_empresa",
                columns: new[] { "enterprise_id", "invited_normalized_email" });

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_enterprise_id_issued_by_membership_id",
                table: "convite_empresa",
                columns: new[] { "enterprise_id", "issued_by_membership_id" });

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_expires_at",
                table: "convite_empresa",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_token_hash",
                table: "convite_empresa",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_papel_acesso_enterprise_id_access_role_id",
                table: "convite_empresa_papel_acesso",
                columns: new[] { "enterprise_id", "access_role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_convite_empresa_papel_acesso_enterprise_id_enterprise_invit~",
                table: "convite_empresa_papel_acesso",
                columns: new[] { "enterprise_id", "enterprise_invitation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_dominio_empresa_normalized_host",
                table: "dominio_empresa",
                column: "normalized_host",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_dominio_empresa_primario",
                table: "dominio_empresa",
                column: "enterprise_id",
                unique: true,
                filter: "is_primary = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_membro_empresa_enterprise_id_user_id",
                table: "membro_empresa",
                columns: new[] { "enterprise_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_membro_empresa_kind_id",
                table: "membro_empresa",
                column: "kind_id");

            migrationBuilder.CreateIndex(
                name: "ix_membro_empresa_user_id",
                table: "membro_empresa",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_membro_empresa_dono_principal_ativo",
                table: "membro_empresa",
                column: "enterprise_id",
                unique: true,
                filter: "kind_id = 1 AND is_active = TRUE");

            migrationBuilder.CreateIndex(
                name: "ix_membro_empresa_papel_acesso_enterprise_id_access_role_id",
                table: "membro_empresa_papel_acesso",
                columns: new[] { "enterprise_id", "access_role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_membro_empresa_papel_acesso_enterprise_id_enterprise_member~",
                table: "membro_empresa_papel_acesso",
                columns: new[] { "enterprise_id", "enterprise_membership_id" });

            migrationBuilder.CreateIndex(
                name: "ix_papel_acesso_enterprise_id_normalized_name",
                table: "papel_acesso",
                columns: new[] { "enterprise_id", "normalized_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_papel_acesso_permissao_enterprise_id_access_role_id",
                table: "papel_acesso_permissao",
                columns: new[] { "enterprise_id", "access_role_id" });

            migrationBuilder.CreateIndex(
                name: "ix_papel_acesso_permissao_permission_id",
                table: "papel_acesso_permissao",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "ix_permissao_code",
                table: "permissao",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_propriedade_enterprise_id_expires_at",
                table: "transferencia_propriedade",
                columns: new[] { "enterprise_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_propriedade_enterprise_id_issued_by_membershi~",
                table: "transferencia_propriedade",
                columns: new[] { "enterprise_id", "issued_by_membership_id" });

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_propriedade_enterprise_id_recipient_membershi~",
                table: "transferencia_propriedade",
                columns: new[] { "enterprise_id", "recipient_membership_id" });

            migrationBuilder.CreateIndex(
                name: "ix_transferencia_propriedade_token_hash",
                table: "transferencia_propriedade",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_normalized_email",
                table: "usuario",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_refresh_token_family_expires_at",
                table: "usuario_refresh_token",
                column: "family_expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_refresh_token_token_hash",
                table: "usuario_refresh_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_refresh_token_user_id_family_id",
                table: "usuario_refresh_token",
                columns: new[] { "user_id", "family_id" });

            migrationBuilder.CreateIndex(
                name: "ix_usuario_token_acao_token_hash",
                table: "usuario_token_acao",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_token_acao_user_id_expires_at",
                table: "usuario_token_acao",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.AddForeignKey(
                name: "fk_cargo_empresa_enterprise_id",
                table: "cargo",
                column: "enterprise_id",
                principalTable: "empresa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_departamento_empresa_enterprise_id",
                table: "departamento",
                column: "enterprise_id",
                principalTable: "empresa",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_funcionario_cargo_enterprise_id_position_id",
                table: "funcionario",
                columns: new[] { "enterprise_id", "position_id" },
                principalTable: "cargo",
                principalColumns: new[] { "enterprise_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_funcionario_departamento_enterprise_id_department_id",
                table: "funcionario",
                columns: new[] { "enterprise_id", "department_id" },
                principalTable: "departamento",
                principalColumns: new[] { "enterprise_id", "id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_funcionario_membro_empresa_enterprise_id_enterprise_members~",
                table: "funcionario",
                columns: new[] { "enterprise_id", "enterprise_membership_id" },
                principalTable: "membro_empresa",
                principalColumns: new[] { "enterprise_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $identity_rollback$
                BEGIN
                    IF EXISTS (SELECT 1 FROM usuario)
                       OR EXISTS (SELECT 1 FROM membro_empresa)
                       OR EXISTS (SELECT 1 FROM cliente) THEN
                        RAISE EXCEPTION 'Identity rollback would discard account data. Restore a reviewed backup instead.';
                    END IF;
                END $identity_rollback$;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_cargo_empresa_enterprise_id",
                table: "cargo");

            migrationBuilder.DropForeignKey(
                name: "fk_departamento_empresa_enterprise_id",
                table: "departamento");

            migrationBuilder.DropForeignKey(
                name: "fk_funcionario_cargo_enterprise_id_position_id",
                table: "funcionario");

            migrationBuilder.DropForeignKey(
                name: "fk_funcionario_departamento_enterprise_id_department_id",
                table: "funcionario");

            migrationBuilder.DropForeignKey(
                name: "fk_funcionario_membro_empresa_enterprise_id_enterprise_members~",
                table: "funcionario");

            migrationBuilder.DropTable(
                name: "cliente_refresh_token");

            migrationBuilder.DropTable(
                name: "cliente_token_acao");

            migrationBuilder.DropTable(
                name: "convite_empresa_papel_acesso");

            migrationBuilder.DropTable(
                name: "dominio_empresa");

            migrationBuilder.DropTable(
                name: "membro_empresa_papel_acesso");

            migrationBuilder.DropTable(
                name: "papel_acesso_permissao");

            migrationBuilder.DropTable(
                name: "transferencia_propriedade");

            migrationBuilder.DropTable(
                name: "usuario_refresh_token");

            migrationBuilder.DropTable(
                name: "usuario_token_acao");

            migrationBuilder.DropTable(
                name: "convite_empresa");

            migrationBuilder.DropTable(
                name: "papel_acesso");

            migrationBuilder.DropTable(
                name: "permissao");

            migrationBuilder.DropTable(
                name: "membro_empresa");

            migrationBuilder.DropTable(
                name: "tipo_membro_empresa");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_enterprise_id_department_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_enterprise_id_enterprise_membership_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_enterprise_id_position_id",
                table: "funcionario");

            migrationBuilder.DropIndex(
                name: "ix_funcionario_enterprise_membership_id",
                table: "funcionario");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_departamento_enterprise_id_id",
                table: "departamento");

            migrationBuilder.DropIndex(
                name: "ix_departamento_enterprise_id_normalized_name",
                table: "departamento");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_cliente_enterprise_id_id",
                table: "cliente");

            migrationBuilder.DropIndex(
                name: "ix_cliente_enterprise_id_normalized_email",
                table: "cliente");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_cargo_enterprise_id_id",
                table: "cargo");

            migrationBuilder.DropIndex(
                name: "ix_cargo_enterprise_id_normalized_name",
                table: "cargo");

            migrationBuilder.DropColumn(
                name: "enterprise_membership_id",
                table: "funcionario");

            migrationBuilder.DropColumn(
                name: "enterprise_id",
                table: "departamento");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "departamento");

            migrationBuilder.DropColumn(
                name: "normalized_name",
                table: "departamento");

            migrationBuilder.DropColumn(
                name: "email_confirmed",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "failed_login_count",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "last_failed_login_at",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "lockout_until",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "normalized_email",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "security_version",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "enterprise_id",
                table: "cargo");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "cargo");

            migrationBuilder.DropColumn(
                name: "normalized_name",
                table: "cargo");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "funcionario",
                newName: "role_id");

            migrationBuilder.AlterColumn<string>(
                name: "full_name",
                table: "funcionario",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "funcionario",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "password_hash",
                table: "funcionario",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "departamento",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "departamento",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "departamento",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                table: "cliente",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "cliente",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "cargo",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "cargo",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "cargo",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_department_id",
                table: "funcionario",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_enterprise_id",
                table: "funcionario",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_funcionario_role_id",
                table: "funcionario",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_funcionario_cargo_role_id",
                table: "funcionario",
                column: "role_id",
                principalTable: "cargo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_funcionario_departamento_department_id",
                table: "funcionario",
                column: "department_id",
                principalTable: "departamento",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
