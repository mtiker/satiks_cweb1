using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace App.DAL.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequiresTraining = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingName = table.Column<string>(type: "jsonb", nullable: false),
                    Floor = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    RoomNumber = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Manufacturers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Website = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SupportPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SupportEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Laboratories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "jsonb", nullable: true),
                    MaxOccupancy = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Laboratories_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Laboratories_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpirationDT = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PreviousRefreshToken = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PreviousExpirationDT = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCertifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertifiedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CertificateReference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ValidationComment = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingCertifications_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrainingCertifications_AspNetUsers_ValidatedByUserId",
                        column: x => x.ValidatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingCertifications_EquipmentCategories_EquipmentCategor~",
                        column: x => x.EquipmentCategoryId,
                        principalTable: "EquipmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "jsonb", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ModelName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Description = table.Column<string>(type: "jsonb", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WarrantyExpiry = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsAvailableForBooking = table.Column<bool>(type: "boolean", nullable: false),
                    EquipmentCondition = table.Column<int>(type: "integer", nullable: false),
                    LaboratoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManufacturerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_EquipmentCategories_EquipmentCategoryId",
                        column: x => x.EquipmentCategoryId,
                        principalTable: "EquipmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Laboratories_LaboratoryId",
                        column: x => x.LaboratoryId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Equipment_Manufacturers_ManufacturerId",
                        column: x => x.ManufacturerId,
                        principalTable: "Manufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    BookingStatus = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalibrationRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CalibrationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NextCalibrationDue = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalibratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalibrationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalibrationRecords_AspNetUsers_CalibratedByUserId",
                        column: x => x.CalibratedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CalibrationRecords_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Resolution = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsScheduled = table.Column<bool>(type: "boolean", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DepartmentId",
                table: "AspNetUsers",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_AppUserId",
                table: "Bookings",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EquipmentId",
                table: "Bookings",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationRecords_CalibratedByUserId",
                table: "CalibrationRecords",
                column: "CalibratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CalibrationRecords_EquipmentId",
                table: "CalibrationRecords",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentCategoryId",
                table: "Equipment",
                column: "EquipmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_LaboratoryId",
                table: "Equipment",
                column: "LaboratoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_ManufacturerId",
                table: "Equipment",
                column: "ManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratories_DepartmentId",
                table: "Laboratories",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Laboratories_LocationId",
                table: "Laboratories",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_EquipmentId",
                table: "MaintenanceRecords",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_PerformedByUserId",
                table: "MaintenanceRecords",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_AppUserId",
                table: "RefreshTokens",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_AppUserId",
                table: "TrainingCertifications",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_EquipmentCategoryId",
                table: "TrainingCertifications",
                column: "EquipmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertifications_ValidatedByUserId",
                table: "TrainingCertifications",
                column: "ValidatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "CalibrationRecords");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "TrainingCertifications");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "EquipmentCategories");

            migrationBuilder.DropTable(
                name: "Laboratories");

            migrationBuilder.DropTable(
                name: "Manufacturers");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
