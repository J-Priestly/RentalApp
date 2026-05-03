using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace RentalApp.Database.Migrations
{
   
    public partial class AddItemLocationPostGIS : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "items",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "IsAvailabe",
                table: "items",
                newName: "IsAvailable");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "items",
                type: "geography (point)",
                nullable: true);
        }

       
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "items");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "items",
                newName: "LastUpdatedAt");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "items",
                newName: "IsAvailabe");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
