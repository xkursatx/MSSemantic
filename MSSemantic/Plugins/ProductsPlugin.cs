using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using MSSemantic.Data;
using MSSemantic.Models;
using System.ComponentModel;

namespace MSSemantic.Plugins
{
    public class ProductsPlugin
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductsPlugin(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ===================================================================
        // TEMEL FONKSİYONLAR
        // ===================================================================

        [KernelFunction("get_products")]
        [Description("Tüm ürünlerin listesini ID ve isim bilgisi ile getirir")]
        public async Task<List<ProductModel>> GetProductsAsync()
        {
            return await _dbContext.Products
                .Select(p => new ProductModel { Id = p.Id, Name = p.Name })
                .ToListAsync();
        }

        [KernelFunction("get_detail")]
        [Description("Belirtilen ID'ye sahip ürünün tüm detaylarını getirir")]
        public async Task<ProductModel?> GetDetailAsync(
            [Description("Ürün ID'si, örn: 1")] int id)
        {
            return await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // ===================================================================
        // ARAMA VE FİLTRELEME FONKSİYONLARI
        // ===================================================================

        [KernelFunction("search_products_by_name")]
        [Description("İsmi verilen anahtar kelimeyi içeren ürünleri arar (büyük/küçük harf duyarsız)")]
        public async Task<List<ProductModel>> SearchByNameAsync(
            [Description("Aranacak kelime, örn: 'masa', 'lamba'")] string keyword)
        {
            // EF Core ile ILIKE (case-insensitive) arama
            return await _dbContext.Products
                .Where(p => EF.Functions.ILike(p.Name ?? "", $"%{keyword}%"))
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        [KernelFunction("get_products_under_price")]
        [Description("Belirtilen fiyatın altında veya eşit fiyattaki ürünleri listeler")]
        public async Task<List<ProductModel>> GetProductsUnderPriceAsync(
            [Description("Üst fiyat sınırı (TL), örn: 500")] decimal maxPrice)
        {
            return await _dbContext.Products
                .Where(p => p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        [KernelFunction("get_products_by_price_range")]
        [Description("Belirtilen fiyat aralığındaki ürünleri listeler")]
        public async Task<List<ProductModel>> GetProductsByPriceRangeAsync(
            [Description("En düşük fiyat (TL), örn: 200")] decimal minPrice,
            [Description("En yüksek fiyat (TL), örn: 600")] decimal maxPrice)
        {
            return await _dbContext.Products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        [KernelFunction("get_low_stock_products")]
        [Description("Stoğu belirtilen eşiğin altında veya eşit olan ürünleri listeler")]
        public async Task<List<ProductModel>> GetLowStockProductsAsync(
            [Description("Stok eşiği, örn: 5 (varsayılan: 5)")] int threshold = 5)
        {
            return await _dbContext.Products
                .Where(p => p.InStock <= threshold)
                .OrderBy(p => p.InStock)
                .ToListAsync();
        }

        [KernelFunction("get_in_stock_products")]
        [Description("Stokta olan (stok sayısı 0'dan büyük) ürünleri listeler")]
        public async Task<List<ProductModel>> GetInStockProductsAsync()
        {
            return await _dbContext.Products
                .Where(p => p.InStock > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        [KernelFunction("get_out_of_stock_products")]
        [Description("Stoğu tükenmiş (stok sayısı 0) ürünleri listeler")]
        public async Task<List<ProductModel>> GetOutOfStockProductsAsync()
        {
            return await _dbContext.Products
                .Where(p => p.InStock == 0)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // ===================================================================
        // İSTATİSTİK FONKSİYONLARI
        // ===================================================================

        [KernelFunction("get_cheapest_product")]
        [Description("En ucuz ürünü getirir")]
        public async Task<ProductModel?> GetCheapestProductAsync()
        {
            return await _dbContext.Products
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync();
        }

        [KernelFunction("get_most_expensive_product")]
        [Description("En pahalı ürünü getirir")]
        public async Task<ProductModel?> GetMostExpensiveProductAsync()
        {
            return await _dbContext.Products
                .OrderByDescending(p => p.Price)
                .FirstOrDefaultAsync();
        }

        [KernelFunction("get_product_count")]
        [Description("Toplam ürün sayısını getirir")]
        public async Task<int> GetProductCountAsync()
        {
            return await _dbContext.Products.CountAsync();
        }

        [KernelFunction("get_average_price")]
        [Description("Tüm ürünlerin ortalama fiyatını hesaplar")]
        public async Task<decimal> GetAveragePriceAsync()
        {
            var average = await _dbContext.Products.AverageAsync(p => p.Price);
            return average ?? 0;
        }
    }
}
