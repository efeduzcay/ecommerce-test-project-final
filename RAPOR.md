# Final Projesi – Test Raporu

**Ders:** Yazılım Test ve Kalitesi (MTH2005)
**Öğrenci:** Efe Düzçay – 20230108057
**Teslim Tarihi:** 12.06.2026
**Framework:** .NET 8 / NUnit 3
**Toplam Test:** 25 | **Geçti:** 10 | **Başarısız:** 15

---

## 1. Senaryo ve Gereksinimler

Vize projesinde geliştirilen e-ticaret sistemine final aşamasında üç yeni iş kuralı eklendi ve sistem test edildi.

### 1.1 Mevcut Akış
1. Kullanıcı ürün seçer
2. Sepete ekler
3. Sipariş verir
4. Ödeme yapar

### 1.2 Final Aşamasında Eklenen Kurallar
| Kural | Açıklama |
|-------|----------|
| Stok kontrolü | Sepetteki ürün adedi mevcut stoktan büyükse sipariş **reddedilmeli** (`OutOfStockException`). |
| İndirim uygulaması | Promosyon kodları (`SAVE10`, `SAVE20`, `WELCOME`) yüzdesel indirim sağlar. Geçersiz kod → `0%`. |
| Minimum sipariş tutarı | Sepet toplamı, `OrderService` üzerinden konfigüre edilen minimum tutardan küçükse sipariş **reddedilmeli** (`MinimumOrderNotMetException`). Varsayılan: 100₺. |

---

## 2. STLC – Test Yaşam Döngüsü

### 2.1 Requirement (Gereksinim Analizi)
- E-ticaret akışının dört ana adımı (ürün, sepet, sipariş, ödeme) hâlâ çalışmalı.
- Üç yeni kural sisteme entegre edilmeli.
- Her kural için en az bir kasıtlı hata yerleştirilmeli ki testler bug'ı yakalayabilsin.

### 2.2 Test Plan
- **Test Tools:** NUnit 3 (test runner), NUnit3TestAdapter (CLI keşfi), .NET 8.
- **Test türleri:** Unit (White Box), Black Box, Gray Box, Integration.
- **Teknikler:** Equivalence Partitioning (EP) ve Boundary Value Analysis (BVA).
- **Çıkış Kriteri:** En az 20 test case, tüm türler dengeli, tüm kasıtlı bug'lar en az bir testte yakalanmış olacak.

### 2.3 Test Design
- 25 test case tasarlandı (8 Unit + 8 Black Box + 3 Gray Box + 6 Integration).
- Her test case'in beklenen sonucu (PASS/FAIL) önceden belirlendi.
- FAIL beklenen testler, kasıtlı bug'ları işaret eder (test referans modeli ile gerçek davranış arasındaki farkı yakalar).

### 2.4 Test Execution
- `dotnet test --logger "console;verbosity=normal"` ile çalıştırıldı.
- 25 testin tamamı koşturuldu; sonuçlar bu raporda tablolaştırıldı.

### 2.5 Test Result & Reporting
- Bölüm 7'deki **Test Result** ve bölüm 8'deki **Bug List** çıktıları üretildi.
- Her başarısız testin neden başarısız olduğu (hangi bug) tek tek not edildi.

---

## 3. Test Case Dizayn Teknikleri

### 3.1 Equivalence Partitioning (EP)
Girdileri davranışsal sınıflara böler; her sınıftan tek bir temsilci ile test eder.

**Örnek (DiscountService):**
- P1 – Geçerli kod (`SAVE10`, `WELCOME`)
- P2 – Geçersiz kod (`FAKE123`, `XYZ`)
- P3 – Boş/`null` kod

TC-14 P1'i, TC-15 P2'yi, TC-16 P3'ün sınırını kapsar.

### 3.2 Boundary Value Analysis (BVA)
Sınıfların sınırlarına özel olarak odaklanır; en fazla hata sınırlarda gizlidir.

**Örnek (Stok kontrolü, stok = 5):**
| Test | Quantity | Beklenen |
|------|----------|----------|
| TC-17 | 1 (stok=0 ürünü) | Reddet |
| TC-18 | 5 (stoğa eşit) | Kabul |
| TC-19 | 6 (stok + 1) | Reddet |

**Örnek (Minimum sipariş = 100₺):**
| Test | Total | Beklenen |
|------|-------|----------|
| TC-20 | 60₺ | Reddet |
| TC-21 | 100₺ (sınır) | Kabul |

---

## 4. Hata Kavramları (Error / Fault / Failure / Defect)

| Kavram | Tanım | Bu projeden örnek |
|--------|-------|-------------------|
| **Error** | Geliştiricinin yaptığı insani hata. | `OrderService.PlaceOrder` yazılırken stok karşılaştırmasının `>` yerine `> stock + 1` yazılması (B6'nın kök nedeni). |
| **Fault (Defect)** | Hatalı insan eyleminin kodda bıraktığı yanlış davranışsal düğüm. | `if (item.Quantity > item.Product.Stock + 1)` satırı – kodun gözle görülür kusurlu kısmı. |
| **Failure** | Kullanım sırasında ortaya çıkan, sistemin beklenen davranışından sapması. | Stoğu 0 olan "Mouse" ürünü için `PlaceOrder` çağrısının exception fırlatmaması ve siparişin `Placed` durumuna geçmesi. |
| **Bug** | Genel kullanımdaki "defect" karşılığı; tespit edilmiş, dokümante edilmiş ve giderilmesi planlanmış kusur. | B1 – B8 numaralı maddeler (bkz. Bölüm 8). |

> Kısaca: **Error** insandadır, **Fault** koddadır, **Failure** çalışma anında gözlenir, **Bug** tespit edilmiş Fault'tur.

---

## 5. Test Stratejileri

### 5.1 Agile Testing
Her yeni özellik (stok, indirim, min. sipariş) eklenirken testler aynı iterasyonda yazıldı; "test-as-you-build" yaklaşımı izlendi. Test ve geliştirme döngüleri kısa tutuldu, her commit derlenebilir ve testlerden geçen/geçmesi planlanan haldedir.

### 5.2 Risk-Based Testing
Risk yüksek alanlara (para hesabı, stok düşürümü, minimum tutar kontrolü) daha çok test yazıldı:
- İndirim/B2 → 3 test (TC-02, TC-10, TC-25)
- Stok/B6 → 3 test (TC-17, TC-18, TC-19, +TC-23)
- Min. sipariş/B7 → 3 test (TC-20, TC-21, TC-24)

Daha düşük riskli alanlar (örn. `ToString` çıktısı, `IsEmpty` getter'ı) test kapsamı dışında tutuldu.

### 5.3 Regression Testing
Vize projesindeki 13 testin tümü değiştirilmeden bırakıldı. Final aşamasında eklenen kod, eski testlerin davranışını bozmamalı. Sonuç: eski testlerden geçen 6 test hâlâ geçiyor; başarısız olan 7'si de aynı kalmış (yani yeni kod, vize bug'larının davranışını değiştirmedi). Bu, regression olmadığının kanıtı.

---

## 6. Test Türleri ve Kapsamı

| Tür | Açıklama | Adet |
|-----|----------|------|
| Unit (White Box) | Sınıfın iç yapısı bilinerek metot bazlı testler. `Cart`, `DiscountService` doğrudan test edildi. | 8 |
| Black Box | Sadece arayüz/davranış bilinerek yazılır; iç kod görülmez. EP ve BVA bu kategoride yoğun. | 8 |
| Gray Box | Hem public API üzerinden test edilir hem de Reflection ile private/internal alanlar doğrulanır. | 3 |
| Integration | Birden çok sınıf bir araya gelir: Product → Cart → OrderService → Payment. | 6 |
| **Toplam** | | **25** |

---

## 7. Test Sonuçları

### 7.1 Özet

| Tür | Toplam | Geçti | Başarısız |
|-----|--------|-------|-----------|
| Unit (White Box) | 8 | 3 | 5 |
| Black Box | 8 | 4 | 4 |
| Gray Box | 3 | 2 | 1 |
| Integration | 6 | 1 | 5 |
| **Toplam** | **25** | **10** | **15** |

### 7.2 Detaylı Tablo

| TC | Test | Tür | Teknik | Sonuç | İlgili Bug |
|----|------|-----|--------|-------|------------|
| TC-01 | AddItem_SameProduct_ShouldIncrementQuantity | Unit | EP | FAIL | B1 |
| TC-02 | GetTotal_WithDiscount_ShouldCalculateCorrectly | Unit | EP | FAIL | B2 |
| TC-03 | RemoveItem_NonExistent_ShouldThrowException | Unit | EP | FAIL | B3 |
| TC-04 | AddItem_ValidProduct_ShouldIncreaseItemCount | Unit | EP | PASS | – |
| TC-05 | GetTotal_EmptyCart_ShouldReturnZero | Unit | BVA | PASS | – |
| TC-06 | AddItem_ValidProduct_ItemCountIsOne | Black Box | EP | PASS | – |
| TC-07 | AddItem_NegativePriceProduct_ShouldThrowException | Black Box | BVA | FAIL | Validasyon eksiği |
| TC-08 | GetTotal_TwoProductsAt50_ReturnsWith100 | Black Box | EP | PASS | – |
| TC-09 | PlaceOrder_InternalStatusField_ShouldBePlaced | Gray Box | EP | PASS | – |
| TC-10 | GetTotal_InternalDiscountedTotal_ShouldReflect10PercentDiscount | Gray Box | EP | FAIL | B2 |
| TC-11 | FullFlow_AddProductPlaceOrderProcessPayment_ShouldSucceed | Integration | EP | PASS | – |
| TC-12 | PlaceOrder_EmptyCart_ShouldThrowInvalidOperationException | Integration | EP | FAIL | B4 |
| TC-13 | ProcessPayment_NegativeAmount_ShouldThrowArgumentException | Integration | BVA | FAIL | B5 |
| TC-14 | GetDiscountPercent_KnownCode_ReturnsExpectedPercent | Unit | EP | PASS | – |
| TC-15 | GetDiscountPercent_UnknownCode_ReturnsZero | Unit | EP | FAIL | B8 |
| TC-16 | GetDiscountPercent_EmptyString_ReturnsZero | Unit | BVA | FAIL | B8 |
| TC-17 | PlaceOrder_StockZero_ShouldThrowOutOfStockException | Black Box | BVA | FAIL | B6 |
| TC-18 | PlaceOrder_QuantityEqualsStock_ShouldSucceed | Black Box | BVA | PASS | – |
| TC-19 | PlaceOrder_QuantityOneOverStock_ShouldThrow | Black Box | BVA | FAIL | B6 |
| TC-20 | PlaceOrder_TotalBelowMinimum_ShouldThrow | Black Box | EP | FAIL | B7 |
| TC-21 | PlaceOrder_TotalEqualsMinimum_ShouldSucceed | Black Box | BVA | PASS | – |
| TC-22 | PlaceOrder_InternalCartReference_ShouldMatchPassedCart | Gray Box | EP | PASS | – |
| TC-23 | FullFlow_ProductOutOfStock_ShouldThrowAndPreventPayment | Integration | BVA | FAIL | B6 |
| TC-24 | FullFlow_TotalUnderMinimum_ShouldThrowMinimumOrderNotMet | Integration | EP | FAIL | B7 |
| TC-25 | FullFlow_WithDiscountCode_ShouldChargeDiscountedTotal | Integration | EP | FAIL | B2 |

---

## 8. Tespit Edilen Bug Listesi

| ID | Dosya | Metot | Açıklama | Yakalayan Test(ler) |
|----|-------|-------|----------|---------------------|
| B1 | `Cart.cs` | `AddItem` | Aynı ürün ikinci kez eklenince miktar artmıyor, yeni satır açılıyor. | TC-01 |
| B2 | `Cart.cs` | `GetTotal` | `int / int` bölümü → indirim faktörü her zaman 1, indirim uygulanmıyor. | TC-02, TC-10, TC-25 |
| B3 | `Cart.cs` | `RemoveItem` | Sepette olmayan ürün silinmek istenince exception fırlatılmıyor. | TC-03 |
| B4 | `OrderService.cs` | `PlaceOrder` | Boş sepetle sipariş verilince exception yok, "başarılı" mesajı dönüyor. | TC-12 |
| B5 | `OrderService.cs` | `ProcessPayment` | Negatif tutar reddedilmiyor; `PaidAmount` negatif olabiliyor. | TC-13 |
| B6 | `OrderService.cs` | `PlaceOrder` | Stok kontrolü off-by-one: `qty > stock + 1`. Stok 0/qty 1 ve qty=stock+1 kabul ediliyor. | TC-17, TC-19, TC-23 |
| B7 | `OrderService.cs` | `PlaceOrder` | Min. sipariş eşiği `MinimumOrderAmount / 2` → 60₺ sepet, 100₺ min'i geçiyor. | TC-20, TC-24 |
| B8 | `DiscountService.cs` | `GetDiscountPercent` | Geçersiz/boş kod için fallback `0` yerine `50` döner; bedava %50 indirim. | TC-15, TC-16 |

---

## 9. Fail Eden Testlerin Kök-Neden Analizi

### B1 – AddItem Duplicate
```csharp
// Hatalı
_items.Add(new CartItem(product, quantity));

// Doğrusu
var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
if (existing != null) { existing.Quantity += quantity; return; }
_items.Add(new CartItem(product, quantity));
```

### B2 – İndirim İçin int Bölümü
```csharp
// Hatalı: int / int = 0
decimal discountFactor = 1 - discountPercent / 100;

// Doğrusu
decimal discountFactor = 1 - (decimal)discountPercent / 100m;
```

### B3 – Sessiz RemoveItem
```csharp
// Doğrusu
if (item == null)
    throw new ArgumentException($"Product {productId} not in cart.");
```

### B4 – Boş Sepet Kontrolü
```csharp
// PlaceOrder içine eklenecek
if (cart.IsEmpty)
    throw new InvalidOperationException("Cannot place order with empty cart.");
```

### B5 – Negatif Tutar
```csharp
// ProcessPayment içine eklenecek
if (amount <= 0)
    throw new ArgumentException("Payment amount must be positive.", nameof(amount));
```

### B6 – Stok Off-By-One
```csharp
// Hatalı
if (item.Quantity > item.Product.Stock + 1)

// Doğrusu
if (item.Quantity > item.Product.Stock)
```

### B7 – Yarım Eşik
```csharp
// Hatalı
if (total < MinimumOrderAmount / 2)

// Doğrusu
if (total < MinimumOrderAmount)
```

### B8 – Geçersiz Kod İçin Yanlış Fallback
```csharp
// Hatalı
return 50;

// Doğrusu
return 0;
```

---

## 10. Çalıştırma

```bash
# Derleme
dotnet build

# Tüm testler
dotnet test

# Konsol demo
dotnet run --project ECommerceApp
```

`dotnet test` çıktısında `Toplam test sayısı: 25` ve `Başarısız: 15` görüldüğünde proje beklenen davranışı sergiliyor demektir – başarısız 15 testin tamamı kasıtlı bug'ları tespit etmektedir.
