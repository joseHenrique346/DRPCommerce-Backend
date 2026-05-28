using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DropCommerce.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "canal_notificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canal_notificacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "metodo_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_metodo_transacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "severidade_fraude",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_severidade_fraude", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_entrada_fila",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_entrada_fila", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_evento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_evento", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_inscricao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_inscricao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_lista_espera",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_lista_espera", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_notificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_notificacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_pagamento_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_pagamento_pedido", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_pedido", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_reserva",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_reserva", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_sessao_fila",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_sessao_fila", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "status_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_status_transacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_cupom", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_notificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_notificacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_sinal_fraude",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_sinal_fraude", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_transacao", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "evento",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    enterprise_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cover_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    banner_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    drop_event_status_id = table.Column<long>(type: "bigint", nullable: false),
                    total_units_available = table.Column<int>(type: "int", nullable: false),
                    units_reserved = table.Column<int>(type: "int", nullable: false),
                    units_sold = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    requires_registration = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    registration_starts_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    registration_ends_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    queue_opens_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    drop_starts_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    drop_ends_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evento", x => x.id);
                    table.ForeignKey(
                        name: "fk_evento_status_evento_drop_event_status_id",
                        column: x => x.drop_event_status_id,
                        principalTable: "status_evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cupom",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    drop_coupon_type_id = table.Column<long>(type: "bigint", nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    min_order_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    max_discount_cap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    max_uses = table.Column<int>(type: "int", nullable: false),
                    used_count = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_single_use = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_exclusive_to_registered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    starts_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cupom", x => x.id);
                    table.ForeignKey(
                        name: "fk_cupom_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cupom_tipo_cupom_drop_coupon_type_id",
                        column: x => x.drop_coupon_type_id,
                        principalTable: "tipo_cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "entrada_fila",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    session_token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    position = table.Column<int>(type: "int", nullable: false),
                    queue_entry_status_id = table.Column<long>(type: "bigint", nullable: false),
                    device_fingerprint = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    called_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expired_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    checked_out_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrada_fila", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrada_fila_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrada_fila_status_entrada_fila_queue_entry_status_id",
                        column: x => x.queue_entry_status_id,
                        principalTable: "status_entrada_fila",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inscricao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_registration_status_id = table.Column<long>(type: "bigint", nullable: false),
                    is_eligible = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    eligibility_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    registered_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    eligibility_checked_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inscricao", x => x.id);
                    table.ForeignKey(
                        name: "fk_inscricao_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_inscricao_status_inscricao_drop_registration_status_id",
                        column: x => x.drop_registration_status_id,
                        principalTable: "status_inscricao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "log_auditoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    employee_id = table.Column<long>(type: "bigint", nullable: true),
                    action = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<long>(type: "bigint", nullable: false),
                    old_values = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_values = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    occurred_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_log_auditoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_log_auditoria_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notificacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_notification_channel_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_notification_type_id = table.Column<long>(type: "bigint", nullable: false),
                    subject = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    body = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    drop_notification_status_id = table.Column<long>(type: "bigint", nullable: false),
                    scheduled_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    sent_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_notificacao_canal_notificacao_drop_notification_channel_id",
                        column: x => x.drop_notification_channel_id,
                        principalTable: "canal_notificacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacao_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacao_status_notificacao_drop_notification_status_id",
                        column: x => x.drop_notification_status_id,
                        principalTable: "status_notificacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacao_tipo_notificacao_drop_notification_type_id",
                        column: x => x.drop_notification_type_id,
                        principalTable: "tipo_notificacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    units_allocated = table.Column<int>(type: "int", nullable: false),
                    units_sold = table.Column<int>(type: "int", nullable: false),
                    max_per_customer = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produto", x => x.id);
                    table.ForeignKey(
                        name: "fk_produto_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessao_fila",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    queue_entry_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    queue_session_status_id = table.Column<long>(type: "bigint", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_heartbeat_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessao_fila", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessao_fila_entrada_fila_queue_entry_id",
                        column: x => x.queue_entry_id,
                        principalTable: "entrada_fila",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sessao_fila_status_sessao_fila_queue_session_status_id",
                        column: x => x.queue_session_status_id,
                        principalTable: "status_sessao_fila",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sinal_fraude",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    queue_entry_id = table.Column<long>(type: "bigint", nullable: false),
                    fraud_signal_type_id = table.Column<long>(type: "bigint", nullable: false),
                    fraud_severity_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_fingerprint = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_confirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    was_blocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    detected_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sinal_fraude", x => x.id);
                    table.ForeignKey(
                        name: "fk_sinal_fraude_entrada_fila_queue_entry_id",
                        column: x => x.queue_entry_id,
                        principalTable: "entrada_fila",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sinal_fraude_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sinal_fraude_severidade_fraude_fraud_severity_id",
                        column: x => x.fraud_severity_id,
                        principalTable: "severidade_fraude",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sinal_fraude_tipo_sinal_fraude_fraud_signal_type_id",
                        column: x => x.fraud_signal_type_id,
                        principalTable: "tipo_sinal_fraude",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "entrada_lista_espera",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_product_id = table.Column<long>(type: "bigint", nullable: true),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    waitlist_entry_status_id = table.Column<long>(type: "bigint", nullable: false),
                    notification_sent = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notified_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrada_lista_espera", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrada_lista_espera_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrada_lista_espera_produto_drop_product_id",
                        column: x => x.drop_product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrada_lista_espera_status_lista_espera_waitlist_entry_stat~",
                        column: x => x.waitlist_entry_status_id,
                        principalTable: "status_lista_espera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "reserva",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_product_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    queue_entry_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_reservation_status_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    lock_token = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reserved_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reserva", x => x.id);
                    table.ForeignKey(
                        name: "fk_reserva_entrada_fila_queue_entry_id",
                        column: x => x.queue_entry_id,
                        principalTable: "entrada_fila",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reserva_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reserva_produto_drop_product_id",
                        column: x => x.drop_product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_reserva_status_reserva_drop_reservation_status_id",
                        column: x => x.drop_reservation_status_id,
                        principalTable: "status_reserva",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_event_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_reservation_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_coupon_id = table.Column<long>(type: "bigint", nullable: true),
                    drop_order_status_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_order_payment_status_id = table.Column<long>(type: "bigint", nullable: false),
                    sub_total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_address_line = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shipping_city = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shipping_state = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shipping_zip_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_pedido_cupom_drop_coupon_id",
                        column: x => x.drop_coupon_id,
                        principalTable: "cupom",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_evento_drop_event_id",
                        column: x => x.drop_event_id,
                        principalTable: "evento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_reserva_drop_reservation_id",
                        column: x => x.drop_reservation_id,
                        principalTable: "reserva",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_status_pagamento_pedido_drop_order_payment_status_id",
                        column: x => x.drop_order_payment_status_id,
                        principalTable: "status_pagamento_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pedido_status_pedido_drop_order_status_id",
                        column: x => x.drop_order_status_id,
                        principalTable: "status_pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "item_pedido",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_order_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_product_id = table.Column<long>(type: "bigint", nullable: false),
                    item_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_pedido_pedido_drop_order_id",
                        column: x => x.drop_order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_item_pedido_produto_drop_product_id",
                        column: x => x.drop_product_id,
                        principalTable: "produto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transacao",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    drop_order_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_transaction_type_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_transaction_method_id = table.Column<long>(type: "bigint", nullable: false),
                    drop_transaction_status_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    gateway_reference = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gateway_provider = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gateway_payload = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    paid_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    refunded_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_transacao_metodo_transacao_drop_transaction_method_id",
                        column: x => x.drop_transaction_method_id,
                        principalTable: "metodo_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_pedido_drop_order_id",
                        column: x => x.drop_order_id,
                        principalTable: "pedido",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_status_transacao_drop_transaction_status_id",
                        column: x => x.drop_transaction_status_id,
                        principalTable: "status_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transacao_tipo_transacao_drop_transaction_type_id",
                        column: x => x.drop_transaction_type_id,
                        principalTable: "tipo_transacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "canal_notificacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "E-mail" },
                    { 2L, "SMS" },
                    { 3L, "Push" },
                    { 4L, "WhatsApp" }
                });

            migrationBuilder.InsertData(
                table: "metodo_transacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Cartão de crédito" },
                    { 2L, "Pix" },
                    { 3L, "Boleto" },
                    { 4L, "Carteira" }
                });

            migrationBuilder.InsertData(
                table: "severidade_fraude",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Baixa" },
                    { 2L, "Média" },
                    { 3L, "Alta" },
                    { 4L, "Crítica" }
                });

            migrationBuilder.InsertData(
                table: "status_entrada_fila",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Aguardando" },
                    { 2L, "Chamado" },
                    { 3L, "Finalizando compra" },
                    { 4L, "Concluído" },
                    { 5L, "Expirado" },
                    { 6L, "Removido" }
                });

            migrationBuilder.InsertData(
                table: "status_evento",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Rascunho" },
                    { 2L, "Inscrições abertas" },
                    { 3L, "Inscrições encerradas" },
                    { 4L, "Fila aberta" },
                    { 5L, "Ativo" },
                    { 6L, "Esgotado" },
                    { 7L, "Encerrado" },
                    { 8L, "Cancelado" }
                });

            migrationBuilder.InsertData(
                table: "status_inscricao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pendente" },
                    { 2L, "Elegível" },
                    { 3L, "Inelegível" },
                    { 4L, "Na lista de espera" }
                });

            migrationBuilder.InsertData(
                table: "status_lista_espera",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Aguardando" },
                    { 2L, "Notificado" },
                    { 3L, "Expirado" },
                    { 4L, "Atendido" }
                });

            migrationBuilder.InsertData(
                table: "status_notificacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Agendado" },
                    { 2L, "Enviado" },
                    { 3L, "Entregue" },
                    { 4L, "Falhou" },
                    { 5L, "Devolvido" }
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
                table: "status_reserva",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Ativa" },
                    { 2L, "Confirmada" },
                    { 3L, "Expirada" },
                    { 4L, "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "status_sessao_fila",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Ativa" },
                    { 2L, "Expirada" },
                    { 3L, "Invalidada" }
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
                table: "tipo_notificacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Inscrição confirmada" },
                    { 2L, "Abertura da fila" },
                    { 3L, "Chamado na fila" },
                    { 4L, "Reserva expirando" },
                    { 5L, "Pedido confirmado" },
                    { 6L, "Disponível na lista de espera" }
                });

            migrationBuilder.InsertData(
                table: "tipo_sinal_fraude",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "IP duplicado" },
                    { 2L, "Dispositivo duplicado" },
                    { 3L, "Comportamento de bot" },
                    { 4L, "Múltiplas contas" },
                    { 5L, "VPN detectada" },
                    { 6L, "Velocidade anormal" }
                });

            migrationBuilder.InsertData(
                table: "tipo_transacao",
                columns: new[] { "id", "description" },
                values: new object[,]
                {
                    { 1L, "Pagamento" },
                    { 2L, "Reembolso" },
                    { 3L, "Reembolso parcial" },
                    { 4L, "Chargeback" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_cupom_code",
                table: "cupom",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_cupom_drop_coupon_type_id",
                table: "cupom",
                column: "drop_coupon_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_cupom_drop_event_id",
                table: "cupom",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_fila_customer_id",
                table: "entrada_fila",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_fila_drop_event_id",
                table: "entrada_fila",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_fila_queue_entry_status_id",
                table: "entrada_fila",
                column: "queue_entry_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_lista_espera_customer_id",
                table: "entrada_lista_espera",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_lista_espera_drop_event_id",
                table: "entrada_lista_espera",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_lista_espera_drop_product_id",
                table: "entrada_lista_espera",
                column: "drop_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_lista_espera_waitlist_entry_status_id",
                table: "entrada_lista_espera",
                column: "waitlist_entry_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_drop_event_status_id",
                table: "evento",
                column: "drop_event_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_enterprise_id",
                table: "evento",
                column: "enterprise_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_slug",
                table: "evento",
                column: "slug");

            migrationBuilder.CreateIndex(
                name: "ix_inscricao_customer_id",
                table: "inscricao",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_inscricao_drop_event_id",
                table: "inscricao",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_inscricao_drop_registration_status_id",
                table: "inscricao",
                column: "drop_registration_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_drop_order_id",
                table: "item_pedido",
                column: "drop_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_pedido_drop_product_id",
                table: "item_pedido",
                column: "drop_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_drop_event_id",
                table: "log_auditoria",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_entity_name",
                table: "log_auditoria",
                column: "entity_name");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_customer_id",
                table: "notificacao",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_drop_event_id",
                table: "notificacao",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_drop_notification_channel_id",
                table: "notificacao",
                column: "drop_notification_channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_drop_notification_status_id",
                table: "notificacao",
                column: "drop_notification_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacao_drop_notification_type_id",
                table: "notificacao",
                column: "drop_notification_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_customer_id",
                table: "pedido",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_coupon_id",
                table: "pedido",
                column: "drop_coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_event_id",
                table: "pedido",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_order_payment_status_id",
                table: "pedido",
                column: "drop_order_payment_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_order_status_id",
                table: "pedido",
                column: "drop_order_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_drop_reservation_id",
                table: "pedido",
                column: "drop_reservation_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_drop_event_id",
                table: "produto",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_produto_product_id",
                table: "produto",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_customer_id",
                table: "reserva",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_drop_event_id",
                table: "reserva",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_drop_product_id",
                table: "reserva",
                column: "drop_product_id");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_drop_reservation_status_id",
                table: "reserva",
                column: "drop_reservation_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_queue_entry_id",
                table: "reserva",
                column: "queue_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessao_fila_customer_id",
                table: "sessao_fila",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessao_fila_queue_entry_id",
                table: "sessao_fila",
                column: "queue_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessao_fila_queue_session_status_id",
                table: "sessao_fila",
                column: "queue_session_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinal_fraude_customer_id",
                table: "sinal_fraude",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinal_fraude_drop_event_id",
                table: "sinal_fraude",
                column: "drop_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinal_fraude_fraud_severity_id",
                table: "sinal_fraude",
                column: "fraud_severity_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinal_fraude_fraud_signal_type_id",
                table: "sinal_fraude",
                column: "fraud_signal_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_sinal_fraude_queue_entry_id",
                table: "sinal_fraude",
                column: "queue_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_customer_id",
                table: "transacao",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_drop_order_id",
                table: "transacao",
                column: "drop_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_drop_transaction_method_id",
                table: "transacao",
                column: "drop_transaction_method_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_drop_transaction_status_id",
                table: "transacao",
                column: "drop_transaction_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_transacao_drop_transaction_type_id",
                table: "transacao",
                column: "drop_transaction_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entrada_lista_espera");

            migrationBuilder.DropTable(
                name: "inscricao");

            migrationBuilder.DropTable(
                name: "item_pedido");

            migrationBuilder.DropTable(
                name: "log_auditoria");

            migrationBuilder.DropTable(
                name: "notificacao");

            migrationBuilder.DropTable(
                name: "sessao_fila");

            migrationBuilder.DropTable(
                name: "sinal_fraude");

            migrationBuilder.DropTable(
                name: "transacao");

            migrationBuilder.DropTable(
                name: "status_lista_espera");

            migrationBuilder.DropTable(
                name: "status_inscricao");

            migrationBuilder.DropTable(
                name: "canal_notificacao");

            migrationBuilder.DropTable(
                name: "status_notificacao");

            migrationBuilder.DropTable(
                name: "tipo_notificacao");

            migrationBuilder.DropTable(
                name: "status_sessao_fila");

            migrationBuilder.DropTable(
                name: "severidade_fraude");

            migrationBuilder.DropTable(
                name: "tipo_sinal_fraude");

            migrationBuilder.DropTable(
                name: "metodo_transacao");

            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropTable(
                name: "status_transacao");

            migrationBuilder.DropTable(
                name: "tipo_transacao");

            migrationBuilder.DropTable(
                name: "cupom");

            migrationBuilder.DropTable(
                name: "reserva");

            migrationBuilder.DropTable(
                name: "status_pagamento_pedido");

            migrationBuilder.DropTable(
                name: "status_pedido");

            migrationBuilder.DropTable(
                name: "tipo_cupom");

            migrationBuilder.DropTable(
                name: "entrada_fila");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "status_reserva");

            migrationBuilder.DropTable(
                name: "status_entrada_fila");

            migrationBuilder.DropTable(
                name: "evento");

            migrationBuilder.DropTable(
                name: "status_evento");
        }
    }
}
