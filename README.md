# ECommerceApp — Final Test Projesi

**Ders:** Yazılım Test ve Kalitesi (MTH2005)
**Öğrenci:** Efe Düzçay | **Numara:** 20230108057

Vize projesinde geliştirilen e-ticaret sistemine **stok kontrolü**, **indirim kodu** ve **minimum sipariş tutarı** kuralları eklenmiş; sistem üzerinde kasıtlı 8 bug bırakılarak C# / NUnit ile **25 test case** yazılmıştır. Detaylı rapor için `RAPOR.md` dosyasına bakınız.

## Proje Yapısı

```
ECommerceApp/
 ├── Core/
 │    ├── Product.cs
 │    ├── Cart.cs              ← B1, B2, B3
 │    ├── OrderService.cs      ← B4, B5, B6, B7
 │    ├── DiscountService.cs   ← B8
 │    └── Exceptions.cs        (OutOfStockException, MinimumOrderNotMetException)
 ├── Program.cs                (akış demosu)
 └── ECommerceApp.csproj

ECommerceApp.Tests/
 ├── UnitTests/
 │    ├── CartUnitTests.cs                (TC-01..05)
 │    └── DiscountServiceUnitTests.cs     (TC-14..16)
 ├── BlackBoxTests/
 │    ├── CartBlackBoxTests.cs            (TC-06..08)
 │    ├── StockBlackBoxTests.cs           (TC-17..19)
 │    └── MinimumOrderBlackBoxTests.cs    (TC-20..21)
 ├── GrayBoxTests/
 │    └── OrderGrayBoxTests.cs            (TC-09, 10, 22)
 ├── IntegrationTests/
 │    └── OrderIntegrationTests.cs        (TC-11..13, 23..25)
 └── ECommerceApp.Tests.csproj
```

## Teknolojiler

- .NET 8
- NUnit 3
- NUnit3TestAdapter

## Kasıtlı Hatalar (8 Adet)

| ID | Dosya | Açıklama |
|----|-------|----------|
| B1 | `Cart.cs` | Aynı ürün ikinci kez eklenince adet artmıyor (duplicate satır) |
| B2 | `Cart.cs` | İndirim için `int / int` bölümü → indirim hep 0 |
| B3 | `Cart.cs` | Olmayan ürün silinmek istenince exception fırlatılmıyor |
| B4 | `OrderService.cs` | Boş sepetle sipariş kabul ediliyor |
| B5 | `OrderService.cs` | Negatif ödeme tutarı kabul ediliyor |
| B6 | `OrderService.cs` | Stok kontrolü off-by-one (`> stock + 1`) — stok=0 sipariş kabul ediliyor |
| B7 | `OrderService.cs` | Min. sipariş eşiği yarısı kadar (`/ 2`) — 60₺ sepet 100₺ min'i geçiyor |
| B8 | `DiscountService.cs` | Geçersiz kod için %0 yerine %50 indirim dönüyor |

## Test Türleri

- **Unit (White Box) – 8 test:** sınıfın iç yapısı bilinerek doğrudan metot testleri (`Cart`, `DiscountService`).
- **Black Box – 8 test:** sadece spesifikasyondan, kaynak koda bakmadan; EP ve BVA tekniği yoğun kullanıldı (stok ve min. sipariş sınırları).
- **Gray Box – 3 test:** Reflection ile private/internal alan inceleme (`_status`, `_discountedTotal`, `_cart`).
- **Integration – 6 test:** Product → Cart → OrderService → Payment akışı uçtan uca.

## Test Sonucu

```
Toplam test sayısı : 25
Geçti              : 10
Başarısız          : 15   (tamamı kasıtlı bug'ları yakalayan testler)
```

| Tür | Toplam | Geçti | Başarısız |
|-----|--------|-------|-----------|
| Unit (White Box) | 8 | 3 | 5 |
| Black Box | 8 | 4 | 4 |
| Gray Box | 3 | 2 | 1 |
| Integration | 6 | 1 | 5 |
| **Toplam** | **25** | **10** | **15** |

## Çalıştırma

```bash
# Derleme
dotnet build

# Tüm testler
dotnet test

# Konsol demosu
dotnet run --project ECommerceApp
```

Detaylı STLC, test case açıklamaları, Error/Fault/Failure/Defect kavramları, test stratejileri ve bug analizi için **`RAPOR.md`** dosyasına bakınız.
