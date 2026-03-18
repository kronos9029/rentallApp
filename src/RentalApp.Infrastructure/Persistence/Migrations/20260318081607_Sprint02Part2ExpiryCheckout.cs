using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint02Part2ExpiryCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    booking_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hold_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    booking_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    end_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    cancellation_policy_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cancelled_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    confirmed_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.booking_id);
                    table.ForeignKey(
                        name: "FK_bookings_cancellation_policies_cancellation_policy_id",
                        column: x => x.cancellation_policy_id,
                        principalTable: "cancellation_policies",
                        principalColumn: "policy_id");
                    table.ForeignKey(
                        name: "FK_bookings_holds_hold_id",
                        column: x => x.hold_id,
                        principalTable: "holds",
                        principalColumn: "hold_id");
                    table.ForeignKey(
                        name: "FK_bookings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkout_orders",
                columns: table => new
                {
                    order_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subtotal_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    total_amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, defaultValue: "VND")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_checkout_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "booking_items",
                columns: table => new
                {
                    booking_item_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    booking_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    court_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bucket_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    booking_mode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slot_qty = table.Column<byte>(type: "tinyint unsigned", nullable: false, defaultValue: (byte)1),
                    unit_price = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    line_total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_items", x => x.booking_item_id);
                    table.ForeignKey(
                        name: "FK_booking_items_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_items_courts_court_id",
                        column: x => x.court_id,
                        principalTable: "courts",
                        principalColumn: "court_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_items_time_buckets_bucket_id",
                        column: x => x.bucket_id,
                        principalTable: "time_buckets",
                        principalColumn: "bucket_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "checkout_order_items",
                columns: table => new
                {
                    order_item_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    booking_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_order_items", x => x.order_item_id);
                    table.ForeignKey(
                        name: "FK_checkout_order_items_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_checkout_order_items_checkout_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "checkout_orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_booking",
                table: "booking_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_booking_items_bucket_id",
                table: "booking_items",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "ix_booking_items_court_bucket",
                table: "booking_items",
                columns: new[] { "court_id", "bucket_id" });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_cancellation_policy_id",
                table: "bookings",
                column: "cancellation_policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_bookings_status_date",
                table: "bookings",
                columns: new[] { "status", "booking_date" });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_user_created",
                table: "bookings",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_bookings_hold",
                table: "bookings",
                column: "hold_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkout_order_items_booking",
                table: "checkout_order_items",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "uq_checkout_order_booking",
                table: "checkout_order_items",
                columns: new[] { "order_id", "booking_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkout_orders_status_created",
                table: "checkout_orders",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_checkout_orders_user_created",
                table: "checkout_orders",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_items");

            migrationBuilder.DropTable(
                name: "checkout_order_items");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "checkout_orders");
        }
    }
}
