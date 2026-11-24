using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DogsService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Breeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Breeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kennels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kennels", x => x.Id);
                    table.CheckConstraint("CK_Kennels_Capacity", "[Capacity] > 0");
                });

            migrationBuilder.CreateTable(
                name: "Dogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TailLength = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Available"),
                    BreedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KennelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dogs", x => x.Id);
                    table.CheckConstraint("CK_Dogs_Status", "[Status] IN ('Available', 'Reserved', 'Adopted', 'UnderTreatment')");
                    table.CheckConstraint("CK_Dogs_TailLength", "[TailLength] >= 0");
                    table.CheckConstraint("CK_Dogs_Weight", "[Weight] > 0");
                    table.ForeignKey(
                        name: "FK_Dogs_Breeds_BreedId",
                        column: x => x.BreedId,
                        principalTable: "Breeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Dogs_Kennels_KennelId",
                        column: x => x.KennelId,
                        principalTable: "Kennels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DogFeatures",
                columns: table => new
                {
                    DogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogFeatures", x => new { x.DogId, x.FeatureId });
                    table.ForeignKey(
                        name: "FK_DogFeatures_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DogFeatures_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DogProfiles",
                columns: table => new
                {
                    DogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Temperament = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogProfiles", x => x.DogId);
                    table.CheckConstraint("CK_DogProfiles_Gender", "[Gender] IS NULL OR [Gender] IN ('Male', 'Female', 'Unknown')");
                    table.ForeignKey(
                        name: "FK_DogProfiles_Dogs_DogId",
                        column: x => x.DogId,
                        principalTable: "Dogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Breeds",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Friendly and intelligent dog breed.", "Labrador Retriever" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Intelligent and versatile working dog breed.", "German Shepherd" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Friendly and curious hunting dog breed.", "Beagle" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Dog with mixed ancestry.", "Mixed Breed" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("12345678-1234-1234-1234-123456789012"), "Has a high activity level and enjoys exercise.", "Active" },
                    { new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Friendly towards people and other animals.", "Friendly" },
                    { new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Has completed basic obedience training.", "Trained" },
                    { new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "Comfortable interacting with children.", "Good with children" }
                });

            migrationBuilder.InsertData(
                table: "Kennels",
                columns: new[] { "Id", "Capacity", "Location", "Name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 10, "Shelter Building 1", "Kennel A" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 15, "Shelter Building 2", "Kennel B" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), 8, "Shelter Building 3", "Kennel C" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Breeds_Name",
                table: "Breeds",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DogFeatures_FeatureId_AssignedAt",
                table: "DogFeatures",
                columns: new[] { "FeatureId", "AssignedAt" });

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

            migrationBuilder.CreateIndex(
                name: "IX_Features_Name",
                table: "Features",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kennels_Name",
                table: "Kennels",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DogFeatures");

            migrationBuilder.DropTable(
                name: "DogProfiles");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "Dogs");

            migrationBuilder.DropTable(
                name: "Breeds");

            migrationBuilder.DropTable(
                name: "Kennels");
        }
    }
}
