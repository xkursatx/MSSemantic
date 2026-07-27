using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MSSemantic.Plugins
{
    public class ProductsPlugin
    {
        // Mock data for the products
        private readonly List<ProductModel> products = new()
   {
      new ProductModel { 
          Id = 1, 
          Name = "Masa Lambası", 
          Description = "Masaüstünüzde şık ve modern bir aydınlatma çözümü",
          Detail = "Masa lambası, çalışma alanınızı aydınlatmak için tasarlanmış şık ve modern bir aydınlatma çözümüdür. Ayarlanabilir ışık seviyesi ve enerji tasarruflu LED teknolojisi ile kullanıcı dostu bir deneyim sunar. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 25x30x80 cm ebatlarındadır.",
          Price = 300, 
          InStock = 21 
      },
      new ProductModel { 
          Id = 2, 
          Name = "Tavan Aydınlatması", 
          Description = "Dış mekan veranda ışığı", 
          Detail = "Tavan aydınlatması, dış mekan veranda ışığı olarak tasarlanmıştır. Suya dayanıklı malzemelerden üretilmiştir ve enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 50x50x20 cm ebatlarındadır.",
          Price = 500, 
          InStock = 0 },
      new ProductModel { 
          Id = 3, 
          Name = "Avize", 
          Description = "Şık ve modern bir avize", 
          Detail = "Avize, şık ve modern bir tasarıma sahip olup, yaşam alanınıza estetik bir dokunuş katar. Enerji tasarruflu LED teknolojisi ile donatılmıştır. 220V güç kaynağı ile çalışır. 2 yıl garanti ile gelir. 60x60x40 cm ebatlarındadır.",
          Price = 800, 
          InStock = 35 
      }
   };

        [KernelFunction("get_products")]
        [Description("Gets a list of products and names")]
        public async Task<List<ProductModel>> GetProductsAsync()
        {
            return products.Select(p => new ProductModel { Id = p.Id, Name = p.Name }).ToList();
        }

        [KernelFunction("get_detail")]
        [Description("Gets details of a particular product")]
        public async Task<ProductModel?> GetDetailAsync([Description("The ID of the product")] int id)
        {
            // Get the details of the product with the specified ID
            return products.FirstOrDefault(product => product.Id == id);
        }
    }

    public class ProductModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("inStock")]
        public int? InStock { get; set; }
    }
}
