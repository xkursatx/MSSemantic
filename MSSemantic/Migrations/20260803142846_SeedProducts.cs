using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MSSemantic.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "description", "detail", "in_stock", "name", "price" },
                values: new object[,]
                {
                    { 1, "Masaüstünüzde şık ve modern bir aydınlatma çözümü", "Masa lambası, çalışma alanınızı aydınlatmak için tasarlanmış şık ve modern bir aydınlatma çözümüdür. Ayarlanabilir ışık seviyesi ve enerji tasarruflu LED teknolojisi ile kullanıcı dostu bir deneyim sunar. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 25x30x80 cm ebatlarındadır.", 21, "Masa Lambası", 300m },
                    { 2, "Dış mekan veranda ışığı", "Tavan aydınlatması, dış mekan veranda ışığı olarak tasarlanmıştır. Suya dayanıklı malzemelerden üretilmiştir ve enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 50x50x20 cm ebatlarındadır.", 0, "Tavan Aydınlatması", 500m },
                    { 3, "Şık ve modern bir avize", "Avize, şık ve modern bir tasarıma sahip olup, yaşam alanınıza estetik bir dokunuş katar. Enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 60x60x40 cm ebatlarındadır.", 35, "Avize", 800m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
