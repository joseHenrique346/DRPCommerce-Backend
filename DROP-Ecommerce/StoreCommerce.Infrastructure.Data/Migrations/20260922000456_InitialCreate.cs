using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StoreCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departamento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departamento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "estado",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metodo_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metodo_transacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_envio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_envio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_fatura",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_fatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_pagamento_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_pagamento_pedido", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_pedido", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_transacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_cupom", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_documento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_envio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_envio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_fatura",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_fatura", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_transacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    trade_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<long>(type: "bigint", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_empresa", x => x.id);
                    table.ForeignKey(
                        name: "fk_empresa_estado_state_id",
                        column: x => x.state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    parent_category_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_categoria_categoria_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_categoria_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cliente",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<long>(type: "bigint", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cliente", x => x.id);
                    table.ForeignKey(
                        name: "fk_cliente_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cliente_estado_state_id",
                        column: x => x.state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    coupon_type_id = table.Column<long>(type: "bigint", nullable: false),
                    discount_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    min_order_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    max_discount_cap = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    max_uses = table.Column<int>(type: "integer", nullable: true),
                    used_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_single_use = table.Column<bool>(type: "boolean", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cupom", x => x.id);
                    table.ForeignKey(
                        name: "fk_cupom_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cupom_tipo_cupom_coupon_type_id",
                        column: x => x.coupon_type_id,
                        principalTable: "tipo_cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fornecedor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<long>(type: "bigint", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "fk_fornecedor_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_fornecedor_estado_state_id",
                        column: x => x.state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "funcionario",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    department_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    hired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionario", x => x.id);
                    table.ForeignKey(
                        name: "fk_funcionario_cargo_role_id",
                        column: x => x.role_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_departamento_department_id",
                        column: x => x.department_id,
                        principalTable: "departamento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_funcionario_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servico",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_servico", x => x.id);
                    table.ForeignKey(
                        name: "fk_servico_categoria_category_id",
                        column: x => x.category_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_servico_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_id = table.Column<long>(type: "bigint", nullable: false),
                    reference_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    document_type_id = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    document_status_id = table.Column<long>(type: "bigint", nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento", x => x.id);
                    table.ForeignKey(
                        name: "fk_documento_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documento_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_status_documento_document_status_id",
                        column: x => x.document_status_id,
                        principalTable: "status_documento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_tipo_documento_document_type_id",
                        column: x => x.document_type_id,
                        principalTable: "tipo_documento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    coupon_id = table.Column<long>(type: "bigint", nullable: true),
                    order_status_id = table.Column<long>(type: "bigint", nullable: false),
                    order_payment_status_id = table.Column<long>(type: "bigint", nullable: false),
                    sub_total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipping_state_id = table.Column<long>(type: "bigint", nullable: false),
                    shipping_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedido_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_cupom_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_estado_shipping_state_id",
                        column: x => x.shipping_state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_status_pagamento_pedido_order_payment_status_id",
                        column: x => x.order_payment_status_id,
                        principalTable: "status_pagamento_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_status_pedido_order_status_id",
                        column: x => x.order_status_id,
                        principalTable: "status_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    category_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bar_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cost_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    height = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    width = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    length = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_urls = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_digital = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_categoria_category_id",
                        column: x => x.category_id,
                        principalTable: "categoria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_produto_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_produto_fornecedor_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "envio",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    supplier_id = table.Column<long>(type: "bigint", nullable: true),
                    shipment_type_id = table.Column<long>(type: "bigint", nullable: false),
                    carrier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shipment_status_id = table.Column<long>(type: "bigint", nullable: false),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    address_line = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_id = table.Column<long>(type: "bigint", nullable: false),
                    zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estimated_delivery = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_envio", x => x.id);
                    table.ForeignKey(
                        name: "fk_envio_estado_state_id",
                        column: x => x.state_id,
                        principalTable: "estado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_envio_fornecedor_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "fornecedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_envio_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_envio_status_envio_shipment_status_id",
                        column: x => x.shipment_status_id,
                        principalTable: "status_envio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_envio_tipo_envio_shipment_type_id",
                        column: x => x.shipment_type_id,
                        principalTable: "tipo_envio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nota_fiscal",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    series = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    access_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    invoice_type_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_status_id = table.Column<long>(type: "bigint", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nota_fiscal", x => x.id);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_empresa_enterprise_id",
                        column: x => x.enterprise_id,
                        principalTable: "empresa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_status_fatura_invoice_status_id",
                        column: x => x.invoice_status_id,
                        principalTable: "status_fatura",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nota_fiscal_tipo_fatura_invoice_type_id",
                        column: x => x.invoice_type_id,
                        principalTable: "tipo_fatura",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    transaction_type_id = table.Column<long>(type: "bigint", nullable: false),
                    transaction_method_id = table.Column<long>(type: "bigint", nullable: false),
                    transaction_status_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    gateway_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gateway_provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    gateway_payload = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refunded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_transacao_cliente_customer_id",
                        column: x => x.customer_id,
                        principalTable: "cliente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_metodo_transacao_transaction_method_id",
                        column: x => x.transaction_method_id,
                        principalTable: "metodo_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_status_transacao_transaction_status_id",
                        column: x => x.transaction_status_id,
                        principalTable: "status_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_tipo_transacao_transaction_type_id",
                        column: x => x.transaction_type_id,
                        principalTable: "tipo_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: true),
                    service_id = table.Column<long>(type: "bigint", nullable: true),
                    item_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_pedido_pedido_order_id",
                        column: x => x.order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_pedido_produto_product_id",
                        column: x => x.product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_pedido_servico_service_id",
                        column: x => x.service_id,
                        principalTable: "servico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "estado",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Acre" },
                    { 2L, "Alagoas" },
                    { 3L, "Amapá" },
                    { 4L, "Amazonas" },
                    { 5L, "Bahia" },
                    { 6L, "Ceará" },
                    { 7L, "Distrito Federal" },
                    { 8L, "Espírito Santo" },
                    { 9L, "Goiás" },
                    { 10L, "Maranhão" },
                    { 11L, "Mato Grosso" },
                    { 12L, "Mato Grosso do Sul" },
                    { 13L, "Minas Gerais" },
                    { 14L, "Pará" },
                    { 15L, "Paraíba" },
                    { 16L, "Paraná" },
                    { 17L, "Pernambuco" },
                    { 18L, "Piauí" },
                    { 19L, "Rio de Janeiro" },
                    { 20L, "Rio Grande do Norte" },
                    { 21L, "Rio Grande do Sul" },
                    { 22L, "Rondônia" },
                    { 23L, "Roraima" },
                    { 24L, "Santa Catarina" },
                    { 25L, "São Paulo" },
                    { 26L, "Sergipe" },
                    { 27L, "Tocantins" }
                });

            migrationBuilder.InsertData(
                table: "metodo_transacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Cartão de crédito" },
                    { 2L, "Pix" },
                    { 3L, "Boleto" }
                });

            migrationBuilder.InsertData(
                table: "status_documento",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Aguardando validação" },
                    { 3L, "Validado" },
                    { 4L, "Rejeitado" },
                    { 5L, "Expirado" }
                });

            migrationBuilder.InsertData(
                table: "status_envio",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Em processamento" },
                    { 3L, "Enviado" },
                    { 4L, "Em trânsito" },
                    { 5L, "Entregue" },
                    { 6L, "Cancelado" },
                    { 7L, "Devolvido" }
                });

            migrationBuilder.InsertData(
                table: "status_fatura",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Autorizada" },
                    { 3L, "Emitida" },
                    { 4L, "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "status_pagamento_pedido",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Pago" },
                    { 3L, "Reembolso parcial" },
                    { 4L, "Reembolso total" },
                    { 5L, "Falhou" }
                });

            migrationBuilder.InsertData(
                table: "status_pedido",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Confirmado" },
                    { 3L, "Em processamento" },
                    { 4L, "Enviado" },
                    { 5L, "Entregue" },
                    { 6L, "Cancelado" },
                    { 7L, "Reembolsado" }
                });

            migrationBuilder.InsertData(
                table: "status_transacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Autorizado" },
                    { 3L, "Capturado" },
                    { 4L, "Falhou" },
                    { 5L, "Cancelado" },
                    { 6L, "Reembolsado" }
                });

            migrationBuilder.InsertData(
                table: "tipo_cupom",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Percentual" },
                    { 2L, "Valor fixo" }
                });

            migrationBuilder.InsertData(
                table: "tipo_documento",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "CPF" },
                    { 2L, "CNPJ" },
                    { 3L, "RG" },
                    { 4L, "CNH" }
                });

            migrationBuilder.InsertData(
                table: "tipo_envio",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Normal" },
                    { 2L, "Expresso" },
                    { 3L, "Econômico" },
                    { 4L, "Retirada no local" }
                });

            migrationBuilder.InsertData(
                table: "tipo_fatura",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Nota Fiscal Eletrônica" },
                    { 2L, "Nota Fiscal de Consumidor Eletrônica" },
                    { 3L, "Nota Fiscal de Serviço Eletrônica" },
                    { 4L, "Nota Fiscal" }
                });

            migrationBuilder.InsertData(
                table: "tipo_transacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pagamento" },
                    { 2L, "Reembolso" },
                    { 3L, "Reembolso parcial" },
                    { 4L, "Estorno" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_categoria_enterprise_id",
                table: "categoria",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_categoria_parent_category_id",
                table: "categoria",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_enterprise_id",
                table: "cliente",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_state_id",
                table: "cliente",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ix_cupom_coupon_type_id",
                table: "cupom",
                column: "coupon_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_cupom_enterprise_id",
                table: "cupom",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_customer_id",
                table: "documento",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_document_status_id",
                table: "documento",
                column: "document_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_document_type_id",
                table: "documento",
                column: "document_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_enterprise_id",
                table: "documento",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_reference_id",
                table: "documento",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "ix_empresa_state_id",
                table: "empresa",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ix_envio_order_id",
                table: "envio",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_envio_shipment_status_id",
                table: "envio",
                column: "shipment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_envio_shipment_type_id",
                table: "envio",
                column: "shipment_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_envio_state_id",
                table: "envio",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "ix_envio_supplier_id",
                table: "envio",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_enterprise_id",
                table: "fornecedor",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_state_id",
                table: "fornecedor",
                column: "state_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_order_id",
                table: "item_pedido",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_product_id",
                table: "item_pedido",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_service_id",
                table: "item_pedido",
                column: "service_id");

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_customer_id",
                table: "nota_fiscal",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_enterprise_id",
                table: "nota_fiscal",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_invoice_status_id",
                table: "nota_fiscal",
                column: "invoice_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_invoice_type_id",
                table: "nota_fiscal",
                column: "invoice_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_nota_fiscal_order_id",
                table: "nota_fiscal",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_coupon_id",
                table: "pedido",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_customer_id",
                table: "pedido",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_enterprise_id",
                table: "pedido",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_order_payment_status_id",
                table: "pedido",
                column: "order_payment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_order_status_id",
                table: "pedido",
                column: "order_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_shipping_state_id",
                table: "pedido",
                column: "shipping_state_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_category_id",
                table: "produto",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_enterprise_id",
                table: "produto",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_supplier_id",
                table: "produto",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_category_id",
                table: "servico",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_servico_enterprise_id",
                table: "servico",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_customer_id",
                table: "transacao",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_order_id",
                table: "transacao",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_transaction_method_id",
                table: "transacao",
                column: "transaction_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_transaction_status_id",
                table: "transacao",
                column: "transaction_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_transaction_type_id",
                table: "transacao",
                column: "transaction_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento");

            migrationBuilder.DropTable(
                name: "envio");

            migrationBuilder.DropTable(
                name: "funcionario");

            migrationBuilder.DropTable(
                name: "item_pedido");

            migrationBuilder.DropTable(
                name: "nota_fiscal");

            migrationBuilder.DropTable(
                name: "transacao");

            migrationBuilder.DropTable(
                name: "status_documento");

            migrationBuilder.DropTable(
                name: "tipo_documento");

            migrationBuilder.DropTable(
                name: "status_envio");

            migrationBuilder.DropTable(
                name: "tipo_envio");

            migrationBuilder.DropTable(
                name: "cargo");

            migrationBuilder.DropTable(
                name: "departamento");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "servico");

            migrationBuilder.DropTable(
                name: "status_fatura");

            migrationBuilder.DropTable(
                name: "tipo_fatura");

            migrationBuilder.DropTable(
                name: "metodo_transacao");

            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropTable(
                name: "status_transacao");

            migrationBuilder.DropTable(
                name: "tipo_transacao");

            migrationBuilder.DropTable(
                name: "fornecedor");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "cliente");

            migrationBuilder.DropTable(
                name: "cupom");

            migrationBuilder.DropTable(
                name: "status_pagamento_pedido");

            migrationBuilder.DropTable(
                name: "status_pedido");

            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropTable(
                name: "tipo_cupom");

            migrationBuilder.DropTable(
                name: "estado");
        }
    }
}
