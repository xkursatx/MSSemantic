# ProductsPlugin - Mevcut Fonksiyonlar

Bu dosya AI asistanının kullanabileceği tüm ürün fonksiyonlarını listeler.

## 📋 Temel Fonksiyonlar

### 1. `get_products`
**Açıklama:** Tüm ürünlerin listesini ID ve isim bilgisi ile getirir  
**Parametreler:** Yok  
**Dönüş:** Ürün listesi (sadece ID ve Name)  
**Örnek Kullanım:** "Hangi ürünleriniz var?"

### 2. `get_detail`
**Açıklama:** Belirtilen ID'ye sahip ürünün tüm detaylarını getirir  
**Parametreler:**
- `id` (int): Ürün ID'si, örn: 1  
**Dönüş:** Tek ürün detayı (tüm bilgiler)  
**Örnek Kullanım:** "1 numaralı ürünün detaylarını göster"

---

## 🔍 Arama ve Filtreleme Fonksiyonları

### 3. `search_products_by_name`
**Açıklama:** İsmi verilen anahtar kelimeyi içeren ürünleri arar (büyük/küçük harf duyarsız)  
**Parametreler:**
- `keyword` (string): Aranacak kelime, örn: 'masa', 'lamba'  
**Dönüş:** Eşleşen ürünler listesi  
**Örnek Kullanım:** "Masa içeren ürünleri göster", "Lamba var mı?"

### 4. `get_products_under_price`
**Açıklama:** Belirtilen fiyatın altında veya eşit fiyattaki ürünleri listeler  
**Parametreler:**
- `maxPrice` (decimal): Üst fiyat sınırı (TL), örn: 500  
**Dönüş:** Fiyata göre sıralanmış ürün listesi  
**Örnek Kullanım:** "500 TL altındaki ürünler neler?"

### 5. `get_products_by_price_range`
**Açıklama:** Belirtilen fiyat aralığındaki ürünleri listeler  
**Parametreler:**
- `minPrice` (decimal): En düşük fiyat (TL), örn: 200
- `maxPrice` (decimal): En yüksek fiyat (TL), örn: 600  
**Dönüş:** Fiyata göre sıralanmış ürün listesi  
**Örnek Kullanım:** "200-600 TL arası ürünler neler?"

### 6. `get_low_stock_products`
**Açıklama:** Stoğu belirtilen eşiğin altında veya eşit olan ürünleri listeler  
**Parametreler:**
- `threshold` (int, varsayılan: 5): Stok eşiği, örn: 5  
**Dönüş:** Stoka göre sıralanmış ürün listesi  
**Örnek Kullanım:** "Stoğu azalan ürünler hangileri?"

### 7. `get_in_stock_products`
**Açıklama:** Stokta olan (stok sayısı 0'dan büyük) ürünleri listeler  
**Parametreler:** Yok  
**Dönüş:** Stokta olan ürün listesi  
**Örnek Kullanım:** "Stoktaki ürünleri göster"

### 8. `get_out_of_stock_products`
**Açıklama:** Stoğu tükenmiş (stok sayısı 0) ürünleri listeler  
**Parametreler:** Yok  
**Dönüş:** Stokta olmayan ürün listesi  
**Örnek Kullanım:** "Tükenen ürünler hangileri?"

---

## 📊 İstatistik Fonksiyonları

### 9. `get_cheapest_product`
**Açıklama:** En ucuz ürünü getirir  
**Parametreler:** Yok  
**Dönüş:** En ucuz ürün  
**Örnek Kullanım:** "En ucuz ürün hangisi?"

### 10. `get_most_expensive_product`
**Açıklama:** En pahalı ürünü getirir  
**Parametreler:** Yok  
**Dönüş:** En pahalı ürün  
**Örnek Kullanım:** "En pahalı ürününüz nedir?"

### 11. `get_product_count`
**Açıklama:** Toplam ürün sayısını getirir  
**Parametreler:** Yok  
**Dönüş:** Ürün sayısı (int)  
**Örnek Kullanım:** "Kaç ürününüz var?"

### 12. `get_average_price`
**Açıklama:** Tüm ürünlerin ortalama fiyatını hesaplar  
**Parametreler:** Yok  
**Dönüş:** Ortalama fiyat (decimal)  
**Örnek Kullanım:** "Ortalama ürün fiyatı nedir?"

---

## 🎯 Kullanım Senaryoları

### Senaryo 1: Ürün Araştırması
```
Kullanıcı: "Hangi ürünleriniz var?"
AI: get_products() çağırır
AI: "Masa Lambası, Tavan Aydınlatması ve Avize ürünlerimiz bulunmaktadır."

Kullanıcı: "Masa lambasının detaylarını göster"
AI: get_detail(1) çağırır
AI: Detaylı ürün bilgilerini gösterir
```

### Senaryo 2: Fiyat Odaklı Arama
```
Kullanıcı: "500 TL altı ürünler neler?"
AI: get_products_under_price(500) çağırır
AI: "Masa Lambası (300 TL)"
```

### Senaryo 3: Stok Durumu
```
Kullanıcı: "Stoğu azalan ürünler var mı?"
AI: get_low_stock_products(5) çağırır
AI: "Tavan Aydınlatması stoğu tükenmiş durumda."
```

### Senaryo 4: İstatistiksel Sorgular
```
Kullanıcı: "Ortalama fiyatınız ne kadar?"
AI: get_average_price() çağırır
AI: "Ürünlerimizin ortalama fiyatı 533 TL'dir."
```

---

## ✅ İyileştirmeler

**Önceki Versiyona Göre Değişiklikler:**

1. ✅ **Raw SQL kaldırıldı** → EF Core LINQ sorguları kullanılıyor
2. ✅ **Nullable güvenlik** → Tüm nullable alanlar güvenli şekilde işleniyor
3. ✅ **Mimari tutarlılık** → Tüm fonksiyonlar DbContext kullanıyor
4. ✅ **Performans** → IQueryable optimizasyonları
5. ✅ **Yeni fonksiyonlar eklendi**:
   - Fiyat aralığı araması
   - Stokta olan/olmayan ürünler
   - İstatistik fonksiyonları (en ucuz, en pahalı, ortalama fiyat)
6. ✅ **Daha iyi açıklamalar** → Her fonksiyon için detaylı Description

---

## 🔒 Güvenlik Notları

- ✅ SQL Injection korumalı (EF Core parametreli sorgular kullanıyor)
- ✅ Null reference korumalı (nullable operatörler kullanılıyor)
- ✅ Type-safe (LINQ compile-time kontrolü)
