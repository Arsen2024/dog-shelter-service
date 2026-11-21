using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DogsService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CatalogFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dogs_BreedId",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_IsDeleted",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_KennelId",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_Status",
                table: "Dogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DogProfiles",
                table: "DogProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DogProfiles_DogId",
                table: "DogProfiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "DogProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DogProfiles",
                table: "DogProfiles",
                column: "DogId");

            migrationBuilder.InsertData(
                table: "Dogs",
                columns: new[] { "Id", "BreedId", "Color", "CreatedAt", "CreatedBy", "DeletedAt", "KennelId", "Name", "Status", "TailLength", "UpdatedAt", "UpdatedBy", "Weight" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), "golden", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Rex", "Available", 35, null, null, 29.50m },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222"), "black & tan", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Bella", "Available", 40, null, null, 27.00m },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), new Guid("33333333-3333-3333-3333-333333333333"), "brown & white", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Max", "Reserved", 25, null, null, 12.30m },
                    { new Guid("d0000000-0000-0000-0000-000000000004"), new Guid("44444444-4444-4444-4444-444444444444"), "grey", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Luna", "UnderTreatment", 20, null, null, 9.80m },
                    { new Guid("d0000000-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), "black", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", null, null, "Charlie", "Adopted", 33, null, null, 31.20m }
                });

            migrationBuilder.InsertData(
                table: "DogFeatures",
                columns: new[] { "DogId", "FeatureId", "AssignedAt", "Notes" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Greets everyone" },
                    { new Guid("d0000000-0000-0000-0000-000000000001"), new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Basic commands" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), new Guid("12345678-1234-1234-1234-123456789012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Needs long walks" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tested with kids" },
                    { new Guid("d0000000-0000-0000-0000-000000000004"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("d0000000-0000-0000-0000-000000000005"), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.InsertData(
                table: "DogProfiles",
                columns: new[] { "DogId", "BirthDate", "Description", "Gender", "Temperament" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), new DateTime(2022, 3, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Energetic retriever, loves water.", "Male", "Playful, friendly" },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), new DateTime(2021, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Calm and attentive shepherd.", "Female", "Loyal, calm" },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), new DateTime(2023, 1, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Curious beagle with a great nose.", "Male", "Curious, social" },
                    { new Guid("d0000000-0000-0000-0000-000000000004"), new DateTime(2020, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shy mixed-breed, recovering after surgery.", "Female", "Shy, gentle" },
                    { new Guid("d0000000-0000-0000-0000-000000000005"), new DateTime(2019, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Adopted family dog.", "Male", "Calm, affectionate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_BreedId_Status_Active",
                table: "Dogs",
                columns: new[] { "BreedId", "Status" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_KennelId_Active",
                table: "Dogs",
                column: "KennelId",
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_Name_Active",
                table: "Dogs",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_Status_Active",
                table: "Dogs",
                column: "Status",
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DogProfiles_Gender",
                table: "DogProfiles",
                sql: "[Gender] IS NULL OR [Gender] IN ('Male', 'Female', 'Unknown')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dogs_BreedId_Status_Active",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_KennelId_Active",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_Name_Active",
                table: "Dogs");

            migrationBuilder.DropIndex(
                name: "IX_Dogs_Status_Active",
                table: "Dogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DogProfiles",
                table: "DogProfiles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DogProfiles_Gender",
                table: "DogProfiles");

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000001"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000001"), new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000002"), new Guid("12345678-1234-1234-1234-123456789012") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000002"), new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000003"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000003"), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000004"), new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd") });

            migrationBuilder.DeleteData(
                table: "DogFeatures",
                keyColumns: new[] { "DogId", "FeatureId" },
                keyValues: new object[] { new Guid("d0000000-0000-0000-0000-000000000005"), new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff") });

            migrationBuilder.DeleteData(
                table: "DogProfiles",
                keyColumn: "DogId",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "DogProfiles",
                keyColumn: "DogId",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "DogProfiles",
                keyColumn: "DogId",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "DogProfiles",
                keyColumn: "DogId",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "DogProfiles",
                keyColumn: "DogId",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Dogs",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Dogs",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Dogs",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Dogs",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Dogs",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000005"));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "DogProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_DogProfiles",
                table: "DogProfiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_BreedId",
                table: "Dogs",
                column: "BreedId");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_IsDeleted",
                table: "Dogs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_KennelId",
                table: "Dogs",
                column: "KennelId");

            migrationBuilder.CreateIndex(
                name: "IX_Dogs_Status",
                table: "Dogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DogProfiles_DogId",
                table: "DogProfiles",
                column: "DogId",
                unique: true);
        }
    }
}
