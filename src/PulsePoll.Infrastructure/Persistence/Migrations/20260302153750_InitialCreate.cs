using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PulsePoll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    plate_code = table.Column<int>(type: "integer", nullable: false),
                    region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    nuts1code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nuts1region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "education_levels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_education_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lsm_socioeconomic_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lsm_socioeconomic_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    object_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_assets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "professions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_professions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "socioeconomic_statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    min_income = table.Column<decimal>(type: "numeric", nullable: true),
                    max_income = table.Column<decimal>(type: "numeric", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_socioeconomic_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "special_codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_special_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    link_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    brand_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "districts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_districts", x => x.id);
                    table.ForeignKey(
                        name: "fk_districts_cities",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tax_offices",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    city_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_offices", x => x.id);
                    table.ForeignKey(
                        name: "fk_tax_offices_cities",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "admin_user_roles",
                columns: table => new
                {
                    admin_user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_user_roles", x => new { x.admin_user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_admin_user_roles_admin_users_admin_user_id",
                        column: x => x.admin_user_id,
                        principalTable: "admin_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_admin_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    permission_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    marital_status = table.Column<int>(type: "integer", nullable: false),
                    gsm_operator = table.Column<int>(type: "integer", nullable: false),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: false),
                    is_retired = table.Column<bool>(type: "boolean", nullable: false),
                    profession_id = table.Column<int>(type: "integer", nullable: false),
                    education_level_id = table.Column<int>(type: "integer", nullable: false),
                    is_head_of_family = table.Column<bool>(type: "boolean", nullable: false),
                    is_head_of_family_retired = table.Column<bool>(type: "boolean", nullable: false),
                    head_of_family_profession_id = table.Column<int>(type: "integer", nullable: true),
                    head_of_family_education_level_id = table.Column<int>(type: "integer", nullable: true),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    iban_full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    socioeconomic_status_id = table.Column<int>(type: "integer", nullable: false),
                    lsm_socioeconomic_status_id = table.Column<int>(type: "integer", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    referral_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    special_code_id = table.Column<int>(type: "integer", nullable: true),
                    kvkk_approval = table.Column<bool>(type: "boolean", nullable: false),
                    kvkk_detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    kvkk_approval_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    fcm_token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.id);
                    table.ForeignKey(
                        name: "fk_subjects_banks",
                        column: x => x.bank_id,
                        principalTable: "banks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_cities",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_districts",
                        column: x => x.district_id,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_education_levels",
                        column: x => x.education_level_id,
                        principalTable: "education_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_hof_education_levels",
                        column: x => x.head_of_family_education_level_id,
                        principalTable: "education_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_hof_professions",
                        column: x => x.head_of_family_profession_id,
                        principalTable: "professions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_lsm_socioeconomic_statuses",
                        column: x => x.lsm_socioeconomic_status_id,
                        principalTable: "lsm_socioeconomic_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_professions",
                        column: x => x.profession_id,
                        principalTable: "professions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_socioeconomic_statuses",
                        column: x => x.socioeconomic_status_id,
                        principalTable: "socioeconomic_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_subjects_special_codes",
                        column: x => x.special_code_id,
                        principalTable: "special_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    short_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    tax_office_id = table.Column<int>(type: "integer", nullable: false),
                    phone1 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    phone2 = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    mobile = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city_id = table.Column<int>(type: "integer", nullable: false),
                    district_id = table.Column<int>(type: "integer", nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_cities",
                        column: x => x.city_id,
                        principalTable: "cities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_customers_districts",
                        column: x => x.district_id,
                        principalTable: "districts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_customers_tax_offices",
                        column: x => x.tax_office_id,
                        principalTable: "tax_offices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iban_last4 = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    iban_encrypted = table.Column<string>(type: "text", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_accounts_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referrals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    referrer_id = table.Column<int>(type: "integer", nullable: false),
                    referred_subject_id = table.Column<int>(type: "integer", nullable: false),
                    referred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    commission_earned = table.Column<decimal>(type: "numeric", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_referrals", x => x.id);
                    table.ForeignKey(
                        name: "fk_referrals_referred_subject",
                        column: x => x.referred_subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_referrals_referrer",
                        column: x => x.referrer_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    replaced_by_token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    revoked_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_earned = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallets", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallets_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    participant_count = table.Column<int>(type: "integer", nullable: false),
                    total_target_count = table.Column<int>(type: "integer", nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    budget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reward = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    consolation_reward = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    survey_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    subject_parameter_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                    customer_briefing = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    start_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    completed_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    disqualify_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quota_full_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    screen_out_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    cover_media_id = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_cover_media",
                        column: x => x.cover_media_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_projects_customers",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    wallet_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_transactions_wallets",
                        column: x => x.wallet_id,
                        principalTable: "wallets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    earned_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_assignments_projects",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_project_assignments_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "subject_assignment_jobs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    admin_id = table.Column<int>(type: "integer", nullable: false),
                    requested_count = table.Column<int>(type: "integer", nullable: false),
                    assigned_count = table.Column<int>(type: "integer", nullable: false),
                    skipped_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_assignment_jobs", x => x.id);
                    table.ForeignKey(
                        name: "fk_subject_assignment_jobs_project",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "withdrawal_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    bank_account_id = table.Column<int>(type: "integer", nullable: false),
                    wallet_transaction_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_by = table.Column<int>(type: "integer", nullable: true),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    deleted_by = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdrawal_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_withdrawal_requests_bank_accounts",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_withdrawal_requests_subjects",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_withdrawal_requests_wallet_transactions",
                        column: x => x.wallet_transaction_id,
                        principalTable: "wallet_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_user_roles_role_id",
                table: "admin_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "idx_admin_users_email",
                table: "admin_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_subject_id",
                table: "bank_accounts",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_banks_name",
                table: "banks",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cities_name",
                table: "cities",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_customers_email",
                table: "customers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_customers_city_id",
                table: "customers",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_district_id",
                table: "customers",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_tax_office_id",
                table: "customers",
                column: "tax_office_id");

            migrationBuilder.CreateIndex(
                name: "uq_customers_code",
                table: "customers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_customers_tax_number",
                table: "customers",
                column: "tax_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_districts_city_id_name",
                table: "districts",
                columns: new[] { "city_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_education_levels_name",
                table: "education_levels",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_lsm_socioeconomic_statuses_name",
                table: "lsm_socioeconomic_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_media_assets_created_at",
                table: "media_assets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "uq_media_assets_object_key",
                table: "media_assets",
                column: "object_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_notifications_subject_id_is_read",
                table: "notifications",
                columns: new[] { "subject_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "idx_permissions_code",
                table: "permissions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_professions_name",
                table: "professions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_project_assignments_subject_id",
                table: "project_assignments",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_project_assignments_project_subject",
                table: "project_assignments",
                columns: new[] { "project_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_projects_customer_id",
                table: "projects",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_projects_status",
                table: "projects",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_projects_cover_media_id",
                table: "projects",
                column: "cover_media_id");

            migrationBuilder.CreateIndex(
                name: "uq_projects_code",
                table: "projects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_referrals_referred",
                table: "referrals",
                column: "referred_subject_id");

            migrationBuilder.CreateIndex(
                name: "idx_referrals_referrer",
                table: "referrals",
                column: "referrer_id");

            migrationBuilder.CreateIndex(
                name: "uq_referrals_referrer_referred",
                table: "referrals",
                columns: new[] { "referrer_id", "referred_subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_expires_at",
                table: "refresh_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_subject_id",
                table: "refresh_tokens",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "idx_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_socioeconomic_statuses_name",
                table: "socioeconomic_statuses",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_stories_is_active_starts_at_ends_at",
                table: "stories",
                columns: new[] { "is_active", "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "idx_stories_order",
                table: "stories",
                column: "order");

            migrationBuilder.CreateIndex(
                name: "ix_subject_assignment_jobs_project_id",
                table: "subject_assignment_jobs",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_subjects_email",
                table: "subjects",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_subjects_phone_number",
                table: "subjects",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_subjects_referral_code",
                table: "subjects",
                column: "referral_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_bank_id",
                table: "subjects",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_city_id",
                table: "subjects",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_district_id",
                table: "subjects",
                column: "district_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_education_level_id",
                table: "subjects",
                column: "education_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_head_of_family_education_level_id",
                table: "subjects",
                column: "head_of_family_education_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_head_of_family_profession_id",
                table: "subjects",
                column: "head_of_family_profession_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_lsm_socioeconomic_status_id",
                table: "subjects",
                column: "lsm_socioeconomic_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_profession_id",
                table: "subjects",
                column: "profession_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_socioeconomic_status_id",
                table: "subjects",
                column: "socioeconomic_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_special_code_id",
                table: "subjects",
                column: "special_code_id");

            migrationBuilder.CreateIndex(
                name: "idx_tax_offices_city_id",
                table: "tax_offices",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "idx_tax_offices_name",
                table: "tax_offices",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "idx_wallet_transactions_wallet_id",
                table: "wallet_transactions",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallets_subject_id",
                table: "wallets",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_bank_account_id",
                table: "withdrawal_requests",
                column: "bank_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawal_requests_subject_id",
                table: "withdrawal_requests",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ux_withdrawal_requests_wallet_transaction_id",
                table: "withdrawal_requests",
                column: "wallet_transaction_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_user_roles");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "project_assignments");

            migrationBuilder.DropTable(
                name: "referrals");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "stories");

            migrationBuilder.DropTable(
                name: "subject_assignment_jobs");

            migrationBuilder.DropTable(
                name: "withdrawal_requests");

            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "bank_accounts");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "wallets");

            migrationBuilder.DropTable(
                name: "tax_offices");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "banks");

            migrationBuilder.DropTable(
                name: "districts");

            migrationBuilder.DropTable(
                name: "education_levels");

            migrationBuilder.DropTable(
                name: "professions");

            migrationBuilder.DropTable(
                name: "lsm_socioeconomic_statuses");

            migrationBuilder.DropTable(
                name: "socioeconomic_statuses");

            migrationBuilder.DropTable(
                name: "special_codes");

            migrationBuilder.DropTable(
                name: "cities");
        }
    }
}
