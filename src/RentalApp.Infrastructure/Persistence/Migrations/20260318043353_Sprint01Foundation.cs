using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint01Foundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cancellation_policies",
                columns: table => new
                {
                    policy_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    policy_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    min_hours_before_start = table.Column<int>(type: "int", nullable: false),
                    refund_strategy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    refund_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    cancel_fee_percent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cancellation_policies", x => x.policy_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "courts",
                columns: table => new
                {
                    court_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    court_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    court_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sort_order = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    maintenance_reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courts", x => x.court_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    role_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.role_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "time_buckets",
                columns: table => new
                {
                    bucket_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    end_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    duration_min = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_buckets", x => x.bucket_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(191)", maxLength: 191, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_login_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "court_buckets",
                columns: table => new
                {
                    court_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bucket_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "None")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    shared_capacity = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)8),
                    shared_reserved = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)0),
                    private_hold_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lock_version = table.Column<uint>(type: "int unsigned", nullable: false, defaultValue: 0u),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_court_buckets", x => new { x.court_id, x.bucket_id });
                    table.ForeignKey(
                        name: "FK_court_buckets_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "court_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_court_buckets_time_buckets_bucket_id",
                        column: x => x.bucket_id,
                        principalTable: "time_buckets",
                        principalColumn: "bucket_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    token_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.token_id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pricing_rules",
                columns: table => new
                {
                    pricing_rule_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rule_name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    applies_to = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    day_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_time = table.Column<TimeOnly>(type: "time(6)", nullable: true),
                    end_time = table.Column<TimeOnly>(type: "time(6)", nullable: true),
                    private_rate = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    shared_rate = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    weekend_markup_pct = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_by_user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rules", x => x.pricing_rule_id);
                    table.ForeignKey(
                        name: "FK_pricing_rules_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    emergency_contact = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    role_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "cancellation_policies",
                columns: new[] { "policy_id", "cancel_fee_percent", "created_at", "is_active", "min_hours_before_start", "policy_name", "refund_percent", "refund_strategy", "updated_at" },
                values: new object[] { "30000000-0000-0000-0000-000000000001", 0m, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, 2, "Standard 2 Hour Refund", 100m, "Full", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "courts",
                columns: new[] { "court_id", "court_code", "court_name", "created_at", "is_active", "maintenance_reason", "sort_order", "updated_at" },
                values: new object[,]
                {
                    { "20000000-0000-0000-0000-000000000001", "C01", "Court 01", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)1, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000002", "C02", "Court 02", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)2, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000003", "C03", "Court 03", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)3, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000004", "C04", "Court 04", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)4, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000005", "C05", "Court 05", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)5, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000006", "C06", "Court 06", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)6, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000007", "C07", "Court 07", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)7, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000008", "C08", "Court 08", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)8, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { "20000000-0000-0000-0000-000000000009", "C09", "Court 09", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), true, null, (byte)9, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "created_at", "role_name" },
                values: new object[,]
                {
                    { "00000000-0000-0000-0000-000000000001", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Admin" },
                    { "00000000-0000-0000-0000-000000000002", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "Customer" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "created_at", "email", "last_login_at", "password_hash", "phone_number", "status", "updated_at", "username" },
                values: new object[] { "10000000-0000-0000-0000-000000000001", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "admin@local.test", null, "DB-SEED-ONLY-NOT-USED-FOR-AUTH", null, "Active", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "admin.local" });

            migrationBuilder.InsertData(
                table: "pricing_rules",
                columns: new[] { "pricing_rule_id", "applies_to", "created_at", "created_by_user_id", "day_type", "effective_from", "effective_to", "end_time", "is_active", "private_rate", "rule_name", "shared_rate", "start_time", "updated_at", "weekend_markup_pct" },
                values: new object[,]
                {
                    { "40000000-0000-0000-0000-000000000001", "Any", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "10000000-0000-0000-0000-000000000001", "Weekday", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new TimeOnly(22, 0, 0), true, 280000m, "Weekday Baseline", 45000m, new TimeOnly(6, 0, 0), new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), 0m },
                    { "40000000-0000-0000-0000-000000000002", "Any", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "10000000-0000-0000-0000-000000000001", "Weekend", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new TimeOnly(22, 0, 0), true, 280000m, "Weekend Baseline", 45000m, new TimeOnly(6, 0, 0), new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), 20m }
                });

            migrationBuilder.InsertData(
                table: "user_profiles",
                columns: new[] { "user_id", "avatar_url", "created_at", "date_of_birth", "emergency_contact", "full_name", "updated_at" },
                values: new object[] { "10000000-0000-0000-0000-000000000001", null, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Admin Local", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "user_id", "assigned_at" },
                values: new object[] { "00000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000001", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_cancellation_policies_is_active",
                table: "cancellation_policies",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_court_buckets_bucket_id",
                table: "court_buckets",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_court_buckets_mode",
                table: "court_buckets",
                column: "mode");

            migrationBuilder.CreateIndex(
                name: "IX_courts_court_code",
                table: "courts",
                column: "court_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courts_is_active",
                table: "courts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_expires_at",
                table: "password_reset_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token_hash",
                table: "password_reset_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_user_id",
                table: "password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_created_by_user_id",
                table: "pricing_rules",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_is_active_effective_from",
                table: "pricing_rules",
                columns: new[] { "is_active", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "IX_roles_role_name",
                table: "roles",
                column: "role_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_time_buckets_start_at",
                table: "time_buckets",
                column: "start_at");

            migrationBuilder.CreateIndex(
                name: "IX_time_buckets_start_at_end_at",
                table: "time_buckets",
                columns: new[] { "start_at", "end_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cancellation_policies");

            migrationBuilder.DropTable(
                name: "court_buckets");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "pricing_rules");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "courts");

            migrationBuilder.DropTable(
                name: "time_buckets");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
