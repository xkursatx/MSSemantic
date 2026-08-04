# Semantic Kernel AI Chat Uygulaması

## Amaç

Bu proje, **Microsoft Semantic Kernel** ile çalışan yapay zeka modellerinin
(Ollama, OpenAI, Azure OpenAI) uygulama fonksiyonlarını (**Tool Calling / Function Calling**)
nasıl otomatik çağırabildiğini ve veritabanı ile entegre çalışabileceğini gösteren 
gerçek hayata yakın bir örnektir.

Bu örnekte;

-   .NET 10
-   Microsoft Semantic Kernel
-   Entity Framework Core
-   PostgreSQL
-   Ollama (Yerel & Cloud)
-   OpenAI
-   Azure OpenAI
-   Chat History (Veritabanı)
-   System Prompt
-   Tool Calling
-   Plugin Mimarisi
-   Streaming Chat
-   Migration Yönetimi

bir arada kullanılmaktadır.

## Mimari

``` text
Kullanıcı
    │
    ▼
ChatHistory (PostgreSQL)
    │
    ▼
System Prompt
    │
    ▼
Semantic Kernel
    │
 ┌──┴───────────────┐
 ▼                  ▼
AI Models       Pluginler
(Ollama/         │
OpenAI/      ProductsPlugin
Azure)           │
              PostgreSQL
```

## Proje Yapısı

``` text
MSSemantic/
├── Data/
│   ├── ApplicationDbContext.cs          # EF Core DbContext
│   ├── ApplicationDbContextFactory.cs   # Migration için factory
│   └── PostgresChatHistoryRepository.cs # Chat history repository
├── Models/
│   ├── ProductModel.cs                  # Ürün entity
│   ├── SessionModel.cs                  # Chat session entity
│   └── MessageModel.cs                  # Chat message entity
├── Plugins/
│   └── ProductsPlugin.cs                # Ürün sorgulama plugin'i
├── Migrations/                          # EF Core migrations
├── appsettings.json                     # Yapılandırma dosyası
└── Program.cs                           # Ana uygulama
```

## Yapılandırma

### appsettings.json

Uygulama yapılandırması `appsettings.json` dosyasından okunur:

```json
{
  "AiProvider": "LocalOllama",  // LocalOllama, CloudOllama, OpenAI, AzureOpenAI
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=chatdb;Username=user;Password=pass"
  },
  "OllamaSettings": {
    "Local": {
      "Endpoint": "http://localhost:11434",
      "Model": "qwen2.5:3b"
    },
    "Cloud": {
      "Endpoint": "https://your-ollama-cloud.com",
      "Model": "qwen2.5:3b"
    }
  },
  "OpenAiSettings": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  },
  "AzureOpenAiSettings": {
    "Endpoint": "https://your-resource.openai.azure.com",
    "ApiKey": "...",
    "DeploymentName": "gpt-4"
  }
}
```

### AI Provider Seçenekleri

1. **LocalOllama**: Yerel bilgisayarda çalışan Ollama servisi
2. **CloudOllama**: Uzak sunucuda veya cloud'da çalışan Ollama servisi
3. **OpenAI**: OpenAI API
4. **AzureOpenAI**: Azure OpenAI Service

## Öne Çıkan Noktalar

-   **Çoklu AI Desteği**: Ollama (yerel/cloud), OpenAI, Azure OpenAI
-   **Entity Framework Core**: Veritabanı işlemleri için
-   **Migration Yönetimi**: Veritabanı şeması otomatik oluşturulur
-   **Kalıcı Chat History**: Tüm konuşmalar PostgreSQL'de saklanır
-   **Streaming Cevap**: AI cevapları stream olarak gelir
-   **System Prompt**: Asistan karakteri ve kuralları tanımlı
-   **Otomatik Tool Calling**: Model ihtiyaç duyduğunda fonksiyon çağırır
-   **Plugin Mimarisi**: Yeni yetenekler kolayca eklenebilir

## Veritabanı

Uygulama PostgreSQL kullanır ve şu tabloları otomatik oluşturur:

### sessions
- `id` (UUID): Session ID
- `created_at` (Timestamp): Oluşturulma zamanı

### messages
- `id` (BigSerial): Mesaj ID
- `session_id` (UUID): Session ID (FK)
- `role` (Text): user/assistant/system
- `content` (Text): Mesaj içeriği
- `created_at` (Timestamp): Oluşturulma zamanı

### products
- `id` (Integer): Ürün ID
- `name` (Varchar): Ürün adı
- `description` (Varchar): Kısa açıklama
- `detail` (Text): Detaylı açıklama
- `price` (Decimal): Fiyat
- `in_stock` (Integer): Stok adedi

## Kurulum

### 1. Gereksinimler
- .NET 10 SDK
- PostgreSQL 12+
- Ollama (opsiyonel, yerel kullanım için)

### 2. Veritabanı Yapılandırması
`appsettings.json` dosyasında PostgreSQL bağlantı bilgilerini güncelleyin.

### 3. Migration Uygulama
```bash
dotnet ef database update
```

Migration otomatik olarak uygulama başlatıldığında da çalışır.

### 4. Ollama Kurulumu (Opsiyonel)
Yerel Ollama kullanmak için:
```bash
# Ollama'yı indirin ve kurun
# https://ollama.ai

# Model indirin
ollama pull qwen2.5:3b
```

### 5. Uygulamayı Çalıştırma
```bash
dotnet run
```

## Kullanım

Uygulama başladığında:

1. Session ID görüntülenir
2. AI provider bilgisi gösterilir
3. Plugin'ler listelenir
4. Soru-cevap döngüsü başlar

Örnek konuşma:
```
Soru: Hangi ürünleriniz var?
Asistan: UMAI Bilişim olarak üç farklı aydınlatma ürünümüz bulunmaktadır...

Soru: Masa lambasının fiyatı nedir?
Asistan: Masa lambamızın fiyatı 300 TL'dir...
```

## Chat History

Tüm konuşmalar veritabanında saklanır. Her oturum (session) için:
- Benzersiz bir session ID oluşturulur
- Kullanıcı ve asistan mesajları kaydedilir
- İleride session ID ile eski konuşmalara dönülebilir

## System Prompt

Asistanın karakteri ve kuralları:
- **Ad**: Cemile
- **Kimlik**: UMAI Bilişim ürün asistanı
- **Diller**: Türkçe, İngilizce, İspanyolca
- **Görev**: Sadece UMAI Bilişim ürünleri hakkında bilgi verme
- **Kişilik**: Samimi, kibar, profesyonel

## Tool Calling

Semantic Kernel, modelin fonksiyon çağırabilmesini sağlar:

```csharp
FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
```

Model ihtiyaç duyduğunda uygun plugin fonksiyonunu kendisi seçer ve çağırır.

> **Not:** Kullanılan AI modelinin Tool Calling (Function Calling) desteğine
> sahip olması gerekir.

## Plugin Sistemi

### ProductsPlugin

Ürün sorgulama için iki fonksiyon sunar:

1. **get_products**: Ürün listesini döner
2. **get_detail**: Belirli bir ürünün detaylarını döner

Plugin, DbContext üzerinden doğrudan veritabanı ile iletişim kurar.

## Geliştirme Notları

### Yeni Plugin Ekleme

1. `Plugins` klasörüne yeni bir sınıf ekleyin
2. Fonksiyonları `[KernelFunction]` ile işaretleyin
3. Parametreleri `[Description]` ile açıklayın
4. `Program.cs`'de plugin'i kaydedin:

```csharp
kernelBuilder.Plugins.AddFromObject(new YourPlugin(dbContext));
```

### Yeni Entity Ekleme

1. `Models` klasörüne entity ekleyin
2. `ApplicationDbContext`'e DbSet ekleyin
3. `OnModelCreating` içinde yapılandırın
4. Migration oluşturun:

```bash
dotnet ef migrations add YourMigrationName
```

### Cloud Ollama Kullanımı

Cloud/Remote Ollama kullanmak için:

1. `appsettings.json`'da `AiProvider`'ı `"CloudOllama"` yapın
2. Cloud endpoint URL'ini ayarlayın
3. Model adını belirtin

**Not**: Ollama connector temel authentication desteklemez. Eğer API key 
gerekiyorsa, reverse proxy (nginx/caddy) kullanarak authentication ekleyebilirsiniz.

### OpenAI Kullanımı

1. `appsettings.json`'da `AiProvider`'ı `"OpenAI"` yapın
2. OpenAI API key'inizi ekleyin
3. Model adını seçin (gpt-4o-mini, gpt-4, vb.)

### Azure OpenAI Kullanımı

1. Azure OpenAI resource oluşturun
2. Model deploy edin
3. `appsettings.json`'da ayarları yapın:
   - Endpoint URL
   - API Key
   - Deployment Name

## Yol Haritası

Bu repo bir seri halinde geliştirilmektedir.

1.  Yerel AI Agent Kurulumu ✅
2.  Semantic Kernel + Tool Calling ✅
3.  Kalıcı Chat Memory (PostgreSQL) ✅
4.  Multi-Provider Support (Ollama/OpenAI/Azure) ✅
5.  RAG (Retrieval Augmented Generation) 🔄
6.  Multi-Agent Mimarisi 📋
7.  Production Senaryoları 📋

## Katkıda Bulunma

Pull request'ler memnuniyetle karşılanır. Büyük değişiklikler için lütfen önce 
bir issue açarak ne değiştirmek istediğinizi tartışın.

## Lisans

MIT
