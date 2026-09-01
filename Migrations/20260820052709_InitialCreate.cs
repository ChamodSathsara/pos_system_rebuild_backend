using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    table_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    record_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    action_time = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_log", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "brand",
                columns: table => new
                {
                    brand_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand", x => x.brand_id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    parent_category_id = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                    table.ForeignKey(
                        name: "FK_category_category_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "company",
                columns: table => new
                {
                    company_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    company_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    registration_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tax_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_company", x => x.company_code);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    customer_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    customer_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    customer_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    loyalty_points = table.Column<int>(type: "int", nullable: false),
                    credit_limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.customer_code);
                });

            migrationBuilder.CreateTable(
                name: "expense_category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "permission",
                columns: table => new
                {
                    permission_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    permission_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission", x => x.permission_id);
                });

            migrationBuilder.CreateTable(
                name: "tax_master",
                columns: table => new
                {
                    tax_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    tax_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_master", x => x.tax_code);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "vendor",
                columns: table => new
                {
                    vendor_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vendor_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    vendor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    contact_person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor", x => x.vendor_id);
                });

            migrationBuilder.CreateTable(
                name: "branch",
                columns: table => new
                {
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    company_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branch", x => x.branch_code);
                    table.ForeignKey(
                        name: "FK_branch_company_company_code",
                        column: x => x.company_code,
                        principalTable: "company",
                        principalColumn: "company_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cheque_register",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    cheque_no = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    customer_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    recipt_no = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    cheque_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    paid_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cheque_register", x => x.id);
                    table.ForeignKey(
                        name: "FK_cheque_register_customer_customer_code",
                        column: x => x.customer_code,
                        principalTable: "customer",
                        principalColumn: "customer_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credit_customer",
                columns: table => new
                {
                    credit_id = table.Column<int>(type: "int", nullable: false),
                    customer_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    credit_limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    receipt_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    return_total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    paid_credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    outstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_activate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_customer", x => x.credit_id);
                    table.ForeignKey(
                        name: "FK_credit_customer_customer_customer_code",
                        column: x => x.customer_code,
                        principalTable: "customer",
                        principalColumn: "customer_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_master",
                columns: table => new
                {
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    item_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    brand_id = table.Column<int>(type: "int", nullable: true),
                    unit_of_measure = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    item_group = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    cost_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    selling_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    reorder_level = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    tax_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_master", x => x.item_code);
                    table.ForeignKey(
                        name: "FK_product_master_brand_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brand",
                        principalColumn: "brand_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_master_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_master_tax_master_tax_code",
                        column: x => x.tax_code,
                        principalTable: "tax_master",
                        principalColumn: "tax_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_role_permission",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role_permission", x => new { x.role_id, x.permission_id });
                    table.ForeignKey(
                        name: "FK_user_role_permission_permission_permission_id",
                        column: x => x.permission_id,
                        principalTable: "permission",
                        principalColumn: "permission_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_permission_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "user_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vendor_ledger",
                columns: table => new
                {
                    ledger_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vendor_id = table.Column<int>(type: "int", nullable: true),
                    grn_total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    return_total = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    paid_credit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    outstanding_balance = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendor_ledger", x => x.ledger_id);
                    table.ForeignKey(
                        name: "FK_vendor_ledger_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendor",
                        principalColumn: "vendor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_user",
                columns: table => new
                {
                    user_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    last_login = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_user", x => x.user_code);
                    table.ForeignKey(
                        name: "FK_system_user_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_system_user_user_role_role_id",
                        column: x => x.role_id,
                        principalTable: "user_role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "warehouse",
                columns: table => new
                {
                    warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    warehouse_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse", x => x.warehouse_code);
                    table.ForeignKey(
                        name: "FK_warehouse_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "discount",
                columns: table => new
                {
                    discount_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    discount_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    discount_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    discount_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    min_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    min_bill_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    applicable_to = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount", x => x.discount_code);
                    table.ForeignKey(
                        name: "FK_discount_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_discount_system_user_created_by",
                        column: x => x.created_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expense",
                columns: table => new
                {
                    expense_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    expense_date = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    paid_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense", x => x.expense_id);
                    table.ForeignKey(
                        name: "FK_expense_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expense_expense_category_category_id",
                        column: x => x.category_id,
                        principalTable: "expense_category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_expense_system_user_paid_by",
                        column: x => x.paid_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "item_log",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    old_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    changed_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_log", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_item_log_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_item_log_system_user_changed_by",
                        column: x => x.changed_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_token",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_token_system_user_user_code",
                        column: x => x.user_code,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order",
                columns: table => new
                {
                    po_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    vendor_id = table.Column<int>(type: "int", nullable: true),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    po_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expected_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order", x => x.po_no);
                    table.ForeignKey(
                        name: "FK_purchase_order_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_system_user_created_by",
                        column: x => x.created_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendor",
                        principalColumn: "vendor_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_token_system_user_user_code",
                        column: x => x.user_code,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale",
                columns: table => new
                {
                    invoice_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    customer_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sale_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tax_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    paid_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    balance_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale", x => x.invoice_no);
                    table.ForeignKey(
                        name: "FK_sale_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_customer_customer_code",
                        column: x => x.customer_code,
                        principalTable: "customer",
                        principalColumn: "customer_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_system_user_created_by",
                        column: x => x.created_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "damage_item",
                columns: table => new
                {
                    damage_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    cost_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    damage_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reported_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_damage_item", x => x.damage_id);
                    table.ForeignKey(
                        name: "FK_damage_item_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_damage_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_damage_item_system_user_reported_by",
                        column: x => x.reported_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_damage_item_warehouse_warehouse_code",
                        column: x => x.warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_inventory",
                columns: table => new
                {
                    stock_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    current_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    last_updated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_inventory", x => x.stock_id);
                    table.ForeignKey(
                        name: "FK_stock_inventory_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_inventory_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_inventory_warehouse_warehouse_code",
                        column: x => x.warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grn_master",
                columns: table => new
                {
                    grn_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    grn_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    po_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    vendor_id = table.Column<int>(type: "int", nullable: true),
                    branch_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    warehouse_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    grn_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    invoice_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    invoice_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    received_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grn_master", x => x.grn_id);
                    table.ForeignKey(
                        name: "FK_grn_master_branch_branch_code",
                        column: x => x.branch_code,
                        principalTable: "branch",
                        principalColumn: "branch_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_master_purchase_order_po_no",
                        column: x => x.po_no,
                        principalTable: "purchase_order",
                        principalColumn: "po_no",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_master_system_user_received_by",
                        column: x => x.received_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_master_vendor_vendor_id",
                        column: x => x.vendor_id,
                        principalTable: "vendor",
                        principalColumn: "vendor_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_master_warehouse_warehouse_code",
                        column: x => x.warehouse_code,
                        principalTable: "warehouse",
                        principalColumn: "warehouse_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_history",
                columns: table => new
                {
                    history_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    po_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    changed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_history", x => x.history_id);
                    table.ForeignKey(
                        name: "FK_purchase_order_history_purchase_order_po_no",
                        column: x => x.po_no,
                        principalTable: "purchase_order",
                        principalColumn: "po_no",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_order_history_system_user_changed_by",
                        column: x => x.changed_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    po_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    received_quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_order_item_purchase_order_po_no",
                        column: x => x.po_no,
                        principalTable: "purchase_order",
                        principalColumn: "po_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    payment_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reference_no = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    received_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_payment_sale_invoice_no",
                        column: x => x.invoice_no,
                        principalTable: "sale",
                        principalColumn: "invoice_no",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_system_user_received_by",
                        column: x => x.received_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sale_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    tax_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_sale_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_item_sale_invoice_no",
                        column: x => x.invoice_no,
                        principalTable: "sale",
                        principalColumn: "invoice_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_return",
                columns: table => new
                {
                    return_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    invoice_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    return_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    total_return_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_return", x => x.return_no);
                    table.ForeignKey(
                        name: "FK_sale_return_sale_invoice_no",
                        column: x => x.invoice_no,
                        principalTable: "sale",
                        principalColumn: "invoice_no",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_return_system_user_created_by",
                        column: x => x.created_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_batch",
                columns: table => new
                {
                    batch_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stock_id = table.Column<int>(type: "int", nullable: false),
                    batch_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    received_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    available_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    received_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_batch", x => x.batch_id);
                    table.ForeignKey(
                        name: "FK_stock_batch_stock_inventory_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stock_inventory",
                        principalColumn: "stock_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grn_item",
                columns: table => new
                {
                    grn_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    grn_id = table.Column<int>(type: "int", nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    batch_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grn_item", x => x.grn_item_id);
                    table.ForeignKey(
                        name: "FK_grn_item_grn_master_grn_id",
                        column: x => x.grn_id,
                        principalTable: "grn_master",
                        principalColumn: "grn_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grn_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grn_return",
                columns: table => new
                {
                    grn_return_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    grn_id = table.Column<int>(type: "int", nullable: true),
                    return_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    return_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    total_return_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grn_return", x => x.grn_return_id);
                    table.ForeignKey(
                        name: "FK_grn_return_grn_master_grn_id",
                        column: x => x.grn_id,
                        principalTable: "grn_master",
                        principalColumn: "grn_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_return_system_user_return_by",
                        column: x => x.return_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_history_change",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    history_id = table.Column<int>(type: "int", nullable: true),
                    field = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    old_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    new_value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_history_change", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_history_change_purchase_order_history_history_id",
                        column: x => x.history_id,
                        principalTable: "purchase_order_history",
                        principalColumn: "history_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sale_return_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    return_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_return_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_sale_return_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_return_item_sale_return_return_no",
                        column: x => x.return_no,
                        principalTable: "sale_return",
                        principalColumn: "return_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_movement",
                columns: table => new
                {
                    movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    batch_id = table.Column<long>(type: "bigint", nullable: false),
                    stock_id = table.Column<int>(type: "int", nullable: false),
                    movement_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    reference_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    previous_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    new_qty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    reference_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movement", x => x.movement_id);
                    table.ForeignKey(
                        name: "FK_stock_movement_stock_batch_batch_id",
                        column: x => x.batch_id,
                        principalTable: "stock_batch",
                        principalColumn: "batch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_movement_stock_inventory_stock_id",
                        column: x => x.stock_id,
                        principalTable: "stock_inventory",
                        principalColumn: "stock_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_movement_system_user_created_by",
                        column: x => x.created_by,
                        principalTable: "system_user",
                        principalColumn: "user_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grn_return_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    grn_return_id = table.Column<int>(type: "int", nullable: true),
                    grn_item_id = table.Column<int>(type: "int", nullable: true),
                    item_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    unit_cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    total_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grn_return_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_grn_return_item_grn_item_grn_item_id",
                        column: x => x.grn_item_id,
                        principalTable: "grn_item",
                        principalColumn: "grn_item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grn_return_item_grn_return_grn_return_id",
                        column: x => x.grn_return_id,
                        principalTable: "grn_return",
                        principalColumn: "grn_return_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_grn_return_item_product_master_item_code",
                        column: x => x.item_code,
                        principalTable: "product_master",
                        principalColumn: "item_code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_branch_company_code",
                table: "branch",
                column: "company_code");

            migrationBuilder.CreateIndex(
                name: "IX_category_parent_category_id",
                table: "category",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_cheque_register_customer_code",
                table: "cheque_register",
                column: "customer_code");

            migrationBuilder.CreateIndex(
                name: "IX_credit_customer_customer_code",
                table: "credit_customer",
                column: "customer_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_email",
                table: "customer",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_customer_mobile",
                table: "customer",
                column: "mobile");

            migrationBuilder.CreateIndex(
                name: "IX_damage_item_branch_code",
                table: "damage_item",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_damage_item_item_code",
                table: "damage_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_damage_item_reported_by",
                table: "damage_item",
                column: "reported_by");

            migrationBuilder.CreateIndex(
                name: "IX_damage_item_warehouse_code",
                table: "damage_item",
                column: "warehouse_code");

            migrationBuilder.CreateIndex(
                name: "IX_discount_created_by",
                table: "discount",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_discount_item_code",
                table: "discount",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_expense_branch_code",
                table: "expense",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_expense_category_id",
                table: "expense",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_expense_paid_by",
                table: "expense",
                column: "paid_by");

            migrationBuilder.CreateIndex(
                name: "IX_grn_item_grn_id",
                table: "grn_item",
                column: "grn_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_item_item_code",
                table: "grn_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_branch_code",
                table: "grn_master",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_po_no",
                table: "grn_master",
                column: "po_no");

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_received_by",
                table: "grn_master",
                column: "received_by");

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_vendor_id",
                table: "grn_master",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_master_warehouse_code",
                table: "grn_master",
                column: "warehouse_code");

            migrationBuilder.CreateIndex(
                name: "IX_grn_return_grn_id",
                table: "grn_return",
                column: "grn_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_return_return_by",
                table: "grn_return",
                column: "return_by");

            migrationBuilder.CreateIndex(
                name: "IX_grn_return_item_grn_item_id",
                table: "grn_return_item",
                column: "grn_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_return_item_grn_return_id",
                table: "grn_return_item",
                column: "grn_return_id");

            migrationBuilder.CreateIndex(
                name: "IX_grn_return_item_item_code",
                table: "grn_return_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_item_log_changed_by",
                table: "item_log",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_item_log_item_code",
                table: "item_log",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_token_token",
                table: "password_reset_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_token_user_code",
                table: "password_reset_token",
                column: "user_code");

            migrationBuilder.CreateIndex(
                name: "IX_payment_invoice_no",
                table: "payment",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "IX_payment_received_by",
                table: "payment",
                column: "received_by");

            migrationBuilder.CreateIndex(
                name: "IX_permission_permission_name",
                table: "permission",
                column: "permission_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_master_barcode",
                table: "product_master",
                column: "barcode");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_brand_id",
                table: "product_master",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_category_id",
                table: "product_master",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_master_tax_code",
                table: "product_master",
                column: "tax_code");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_branch_code",
                table: "purchase_order",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_created_by",
                table: "purchase_order",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_vendor_id",
                table: "purchase_order",
                column: "vendor_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_history_changed_by",
                table: "purchase_order_history",
                column: "changed_by");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_history_po_no",
                table: "purchase_order_history",
                column: "po_no");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_history_change_history_id",
                table: "purchase_order_history_change",
                column: "history_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_item_item_code",
                table: "purchase_order_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_item_po_no",
                table: "purchase_order_item",
                column: "po_no");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_token",
                table: "refresh_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_code",
                table: "refresh_token",
                column: "user_code");

            migrationBuilder.CreateIndex(
                name: "IX_sale_branch_code",
                table: "sale",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_sale_created_by",
                table: "sale",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sale_customer_code",
                table: "sale",
                column: "customer_code");

            migrationBuilder.CreateIndex(
                name: "IX_sale_item_invoice_no",
                table: "sale_item",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "IX_sale_item_item_code",
                table: "sale_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_sale_return_created_by",
                table: "sale_return",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sale_return_invoice_no",
                table: "sale_return",
                column: "invoice_no");

            migrationBuilder.CreateIndex(
                name: "IX_sale_return_item_item_code",
                table: "sale_return_item",
                column: "item_code");

            migrationBuilder.CreateIndex(
                name: "IX_sale_return_item_return_no",
                table: "sale_return_item",
                column: "return_no");

            migrationBuilder.CreateIndex(
                name: "IX_stock_batch_stock_id",
                table: "stock_batch",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_inventory_branch_code",
                table: "stock_inventory",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_stock_inventory_item_code_branch_code_warehouse_code",
                table: "stock_inventory",
                columns: new[] { "item_code", "branch_code", "warehouse_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_inventory_warehouse_code",
                table: "stock_inventory",
                column: "warehouse_code");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_batch_id",
                table: "stock_movement",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_created_by",
                table: "stock_movement",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_stock_id",
                table: "stock_movement",
                column: "stock_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_user_branch_code",
                table: "system_user",
                column: "branch_code");

            migrationBuilder.CreateIndex(
                name: "IX_system_user_role_id",
                table: "system_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_system_user_username",
                table: "system_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_role_role_name",
                table: "user_role",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_role_permission_permission_id",
                table: "user_role_permission",
                column: "permission_id");

            migrationBuilder.CreateIndex(
                name: "IX_vendor_vendor_code",
                table: "vendor",
                column: "vendor_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vendor_ledger_vendor_id",
                table: "vendor_ledger",
                column: "vendor_id",
                unique: true,
                filter: "[vendor_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_branch_code",
                table: "warehouse",
                column: "branch_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log");

            migrationBuilder.DropTable(
                name: "cheque_register");

            migrationBuilder.DropTable(
                name: "credit_customer");

            migrationBuilder.DropTable(
                name: "damage_item");

            migrationBuilder.DropTable(
                name: "discount");

            migrationBuilder.DropTable(
                name: "expense");

            migrationBuilder.DropTable(
                name: "grn_return_item");

            migrationBuilder.DropTable(
                name: "item_log");

            migrationBuilder.DropTable(
                name: "password_reset_token");

            migrationBuilder.DropTable(
                name: "payment");

            migrationBuilder.DropTable(
                name: "purchase_order_history_change");

            migrationBuilder.DropTable(
                name: "purchase_order_item");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "sale_item");

            migrationBuilder.DropTable(
                name: "sale_return_item");

            migrationBuilder.DropTable(
                name: "stock_movement");

            migrationBuilder.DropTable(
                name: "user_role_permission");

            migrationBuilder.DropTable(
                name: "vendor_ledger");

            migrationBuilder.DropTable(
                name: "expense_category");

            migrationBuilder.DropTable(
                name: "grn_item");

            migrationBuilder.DropTable(
                name: "grn_return");

            migrationBuilder.DropTable(
                name: "purchase_order_history");

            migrationBuilder.DropTable(
                name: "sale_return");

            migrationBuilder.DropTable(
                name: "stock_batch");

            migrationBuilder.DropTable(
                name: "permission");

            migrationBuilder.DropTable(
                name: "grn_master");

            migrationBuilder.DropTable(
                name: "sale");

            migrationBuilder.DropTable(
                name: "stock_inventory");

            migrationBuilder.DropTable(
                name: "purchase_order");

            migrationBuilder.DropTable(
                name: "customer");

            migrationBuilder.DropTable(
                name: "product_master");

            migrationBuilder.DropTable(
                name: "warehouse");

            migrationBuilder.DropTable(
                name: "system_user");

            migrationBuilder.DropTable(
                name: "vendor");

            migrationBuilder.DropTable(
                name: "brand");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "tax_master");

            migrationBuilder.DropTable(
                name: "branch");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "company");
        }
    }
}
