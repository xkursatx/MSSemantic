# Semantic Kernel Local Tool Calling Sample

## Amaç

Bu proje, **Microsoft Semantic Kernel** ile çalışan yerel bir LLM'in
(Ollama) uygulama fonksiyonlarını (**Tool Calling / Function Calling**)
nasıl otomatik çağırabildiğini gösteren basit ama gerçek hayata yakın
bir örnektir.

Bu örnekte;

-   .NET 9
-   Microsoft Semantic Kernel
-   Ollama
-   Chat History
-   System Prompt
-   Tool Calling
-   Plugin Mimarisi
-   Streaming Chat

bir arada kullanılmaktadır.

## Mimari

``` text
Kullanıcı
    │
    ▼
ChatHistory
    │
    ▼
System Prompt
    │
    ▼
Semantic Kernel
    │
 ┌──┴───────────────┐
 ▼                  ▼
LLM             Pluginler
                     │
               Ürün Servisi
                     │
          In-Memory / Veritabanı / API
```

## Proje Yapısı

``` text
Program.cs
ProductsPlugin.cs
```

## Öne Çıkan Noktalar

-   Yerel çalışan LLM
-   Streaming cevap üretimi
-   Chat History kullanımı
-   Başlangıç **System Prompt**
-   Otomatik Tool Calling
-   Plugin tabanlı mimari

## In-Memory Veri

Örneğin sade kalması için ürünler bellek içerisinde tutulmaktadır.

Gerçek projelerde aynı plugin;

-   PostgreSQL
-   SQL Server
-   Entity Framework Core
-   Dapper
-   REST API
-   ERP sistemleri

üzerinden veri okuyabilir. Semantic Kernel açısından mimari değişmez;
yalnızca plugin'in veri kaynağı değişir.

## Chat History

Konuşmalar `ChatHistory` içerisinde tutulmaktadır. Böylece model önceki
mesajlardan bağlam çıkarabilir.

İlerleyen bölümlerde bu yapı PostgreSQL gibi kalıcı bir veri kaynağına
taşınarak oturumların yeniden devam ettirilmesi gösterilecektir.

## System Prompt

Asistanın karakteri, dili ve çalışma kuralları başlangıçta System Prompt
ile tanımlanmaktadır.

Bu örnekte prompt kod içerisinde tanımlıdır. Üretim ortamlarında ise bu
yapı veritabanından veya bir yönetim panelinden dinamik olarak
okunabilir.

## Tool Calling

Semantic Kernel aşağıdaki ayarlar sayesinde modeli fonksiyon çağırabilir
hale getirir.

-   `FunctionChoiceBehavior.Auto()`
-   `ToolCallBehavior.AutoInvokeKernelFunctions`

Model ihtiyaç duyduğunda uygun plugin fonksiyonunu kendisi seçer.

> **Not:** Kullanılan LLM'in Tool Calling (Function Calling) desteğine
> sahip olması gerekir.

## Yol Haritası

Bu repo bir seri halinde geliştirilmektedir.

1.  Yerel AI Agent Kurulumu
2.  Semantic Kernel + Tool Calling ✅
3.  Kalıcı Chat Memory
4.  RAG
5.  Multi-Agent Mimarisi
6.  Production Senaryoları
