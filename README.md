# E-Commerce Sample
Bu projede mikroservis mimarisi ile sipariş süreci ele alınmıştır. Asp.NET Web Api, PostgreSQL, EntityFramework, Docker, RabbitMQ teknolojileri kullanılarak geliştirilen bu yapıda akış SAGA (orchestration) design patterni ile sağlanmıştır.

## Akış
![Flowchart](https://github.com/user-attachments/assets/81dfc8ae-455f-45af-8f9a-998781224e39)

## Servisler
### Order
#### Sorumlulukları
- Gelen siparişi kendi database'ine pending statusu ile kayıt eder. Daha sonra bu siparişi 'StockUpdateQueue' kuyruğuna bırakır.
- 'OrderApproveQueue' kuyruğunu dinler ve gelen bir mesaj varsa ilgili siparişi onaylar. Başarısız olma durumunda 5 kez dener. Denemeler sonucu da başarısız ise mesajı 'ErrorOrderApproveQueue' kuyruğuna bırakır.
- 'OrderCancelQueue' kuyruğunu dinler ve gelen bir mesaj varsa ilgili siparişi iptal eder. Başarısız olma durumunda 5 kez dener. Denemeler sonucu da başarısız ise mesajı 'ErrorOrderCancelQueue' kuyruğuna bırakır.
### Stock
#### Sorumlulukları
- 'StockUpdateQueue' kuyruğunu dinler ve gelen bir mesaj varsa ilgili sipariş içeriğindeki ürünleri miktarı kadar stoktan düşer. Yeterli stok yoksa siparişi 'OrderCancelQueue', yeterli stok var ise 'OrderApproveQueue' kuyruğuna iletir.
### Notification
#### Sorumlulukları
- NotificationNotifyQueue kuyruğunu dinler ve gelen bir mesaj varsa console'a mesaj yazar.

  ## Projenin Başlatılması
- Projenin root dizininde 'docker compose up' komutu ile tüm containerlar çalıştırılır. Burada container bağımlılıkları olduğundan tüm containerların hazır olma süresi ~20sn sürecektir. Detay için docker-compose.yml dosyasını inceleyin.
- Order ve Stock servislerinin databaselerinin hazırlanması için migration yapılması ve databaselerin update edilmesi gerekmektedir. 

## Örnek İstek Atılması
#### Ürün

```http
  POST /api/productStock
  
  Ürün stoğu ekler
```

| Body Özelliği | Tip     | Açıklama                |
| :-------- | :------- | :------------------------- |
| `ProductId` | `guid` | **Gerekli**. Ürün idsi |
| `Quantity` | `long` | **Gerekli**. Stok miktarı |


```http
  GET /api/productStock
  
  Ürün stoğu sorgular
```

| Body Özelliği | Tip     | Açıklama                |
| :-------- | :------- | :------------------------- |
| `ProductId` | `guid` | **Gerekli**. Ürün idsi |

#### Sipariş

```http
  POST /api/order
  
  Sipariş oluşturur (tüm akış bu sorgu ile başlar)
```

| Parametre | Tip     | Açıklama                       |
| :-------- | :------- | :-------------------------------- |
| `UserId`      | `guid` | **Gerekli**. Siparişi oluşturan kullanıcı |
| `OrderProducts`      | `OrderProduct` | **Gerekli**. Sipariş edilen ürünler |

OrderProduct nesnesi
| Parametre | Tip     | Açıklama                       |
| :-------- | :------- | :-------------------------------- |
| `ProductId`      | `guid` | **Gerekli**. Sipariş edilen ürün |
| `Quantity`      | `long` | **Gerekli**. Miktar |

Sipariş idsi döndürür.



```http
  GET /api/order
  
  Sipariş sorgulama
```

| Parametre | Tip     | Açıklama                       |
| :-------- | :------- | :-------------------------------- |
| `OrderId`      | `guid` | **Gerekli**. Siparişi idsi |
