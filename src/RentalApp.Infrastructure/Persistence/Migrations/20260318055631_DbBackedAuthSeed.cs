using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DbBackedAuthSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: "10000000-0000-0000-0000-000000000001",
                column: "password_hash",
                value: "AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v+9B/m68fN7pVomQZIk2QhdaT58yv9z9zo2k0tHeC3mUA==");

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "created_at", "email", "last_login_at", "password_hash", "phone_number", "status", "updated_at", "username" },
                values: new object[] { "10000000-0000-0000-0000-000000000002", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "customer@local.test", null, "AQAAAAIAAYagAAAAEBAhMkNUZXaHmKm6y9zt/g/dvnXaKV7KW02eQ7Sj/tJyjyBsxoyog7jpqOJeHhupEg==", null, "Active", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), "customer.local" });

            migrationBuilder.InsertData(
                table: "user_profiles",
                columns: new[] { "user_id", "avatar_url", "created_at", "date_of_birth", "emergency_contact", "full_name", "updated_at" },
                values: new object[] { "10000000-0000-0000-0000-000000000002", null, new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Customer Local", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "user_id", "assigned_at" },
                values: new object[] { "00000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000002", new DateTime(2026, 3, 18, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "user_profiles",
                keyColumn: "user_id",
                keyValue: "10000000-0000-0000-0000-000000000002");

            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { "00000000-0000-0000-0000-000000000002", "10000000-0000-0000-0000-000000000002" });

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: "10000000-0000-0000-0000-000000000002");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: "10000000-0000-0000-0000-000000000001",
                column: "password_hash",
                value: "DB-SEED-ONLY-NOT-USED-FOR-AUTH");
        }
    }
}
