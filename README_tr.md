<p align="center"><img src="https://github.com/IridiumIO/CompactGUI/assets/1491536/64f66b5d-0710-4f66-8b88-6a69f7eb9b63" width="500"></p>

<p align="center">
  <a href="https://github.com/IridiumIO/CompactGUI/releases">
    <img alt="GitHub İndirmeleri (tüm varlıklar, tüm sürümler)" src="https://img.shields.io/github/downloads/IridiumIO/CompactGUI/total?style=for-the-badge&logo=github">
    <img alt="GitHub Sürümü" src="https://img.shields.io/github/v/release/IridiumIO/CompactGUI?style=for-the-badge">
  </a>
  </br> 
</p>


<p align="center"><b>CompactGUI, oyunlarınızı ve programlarınızı işlevselliklerini etkilemeden şeffaf bir şekilde sıkıştırarak kullandıkları alanı azaltır. Windows 10 ve sonrasında bulunan yerel <code>compact.exe</code> komut satırı aracıyla aynı şeyi başarmak için doğrudan Win32 API'si ile çalışır.</b></p>

&nbsp;
&nbsp;

<p align="center"><img src="CompactGUI/assets/Home.png" width="750"/></p>


---
<p align="center">
  <a href="README.md">English</a> -
  <a href="README_ru.md">Русский</a> -
  <a href="README_cn.md">简体中文</a> -
  <a href="README_tr.md">Türkçe</a>
</p>
&nbsp;

**compact.exe nedir?**
Windows 10'da tanıtılan ve oyunları, programları ve diğer klasörleri neredeyse hiç performans kaybı olmadan şeffaf bir şekilde sıkıştırmanıza olanak tanıyan yeni algoritmalar topluluğuna sahip bir komutçuluktur. CompactGUI, bu işlevselliği daha kullanıcı dostu bir şekilde sunmak için ayn API'yi kullanır.

**Şeffaf bir şekilde? Bu ne anlama geliyor?**
Şeffaf sıkıştırma, dosyaların sanki hiçbir şey olmamış gibi bilgisayarda normal şekilde kullanılmaya devam edilebileceği anlamına gelir - Zip ve Rar dosyaları gibi yeniden paketlenmezler. Oyunları ve programları daha önce yaptığınız gibi tam olarak tarayabilir ve başlatabilirsiniz, tek fark daha az yer kaplamalarıdır.

**Bu, Windows'un eski sürümlerindeki yerleşik sıkıştırmadan ne farkı var?**
Bu, Windows'ta yerleşik olan eski sıkıştırmaya benzer (Sağ tık > Özellikler > Alan kazanmak için içeriği sıkıştır), ancak Windows 10+'da tanıtılan daha yeni algoritmalar çok daha üstündür ve neredeyse hiç performans etkisi olmadan daha yüksek sıkıştırma oranları sağlar. Eski HDD'lere sahip olanlar, azaltılmış yükleme süreleri şeklinde bir performans artışı bile görebilirler - daha küçük dosyalar RAM'e daha hızlı okunabilir ve CPU bunları uçuş sırasında tipik bir HDD'nin sağlayabileceğinden çok daha hızlı bir şekilde açabilir. [Daha fazla bilgi burada bulunabilir](https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx)

<h2>Kurulum  </h2> 
   
####

<img alt="Statik Rozet" src="https://img.shields.io/badge/GitHub'dan%20%C4%B0ND%C4%B0R-steelblue?style=for-the-badge&logo=github&link=https%3A%2F%2Fgithub.com%2FIridiumIO%2FCompactGUI%2Freleases">

Veya Winget ile kurun: 
```py
winget install CompactGUI
```

## Kullanım Alanları

Bu aracı, klasörleri normal şekilde kullanmaya/çalıştırmaya devam ederken sıkıştırmak için kullanın:

- Oyunların boyutunu azaltın (örn. ARK-Survival Evolved: 169 GB > 91.2 GB)
- Programların boyutunu azaltın (örn. Adobe Photoshop: 1.71 GB > 886 MB)
- Bilgisayarınızdaki diğer tüm klasörleri sıkıştırın
  
## Ek Özellikler

- Sıkıştırma ilerlemesi ve istatistikleri hakkında görsel geri bildirim
- Atlanabilecek, kötü sıkıştırılmış dosya türlerinin yapılandırılabilir listesi
- Sıkıştırma tahminleri almak için topluluk kaynaklı [veritabanı](https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results) ile çevrimiçi entegrasyon
  - Steam oyun sonuçları CompactGUI içinden çevrimiçi veritabanına gönderilebilir
- Daha kolay kullanım için Windows Explorer bağlam menülerine entegrasyon
- Mevcut klasörlerin durumunu analiz etme
- Background Watcher (Arkaplan İzleyici) - klasörleri takip eder ve değişiklikleri (örn. Steam oyun güncellemeleri) izleyerek onları arkaplanda otomatik olarak sıkıştırılmış halde tutar.

<h4 align="center"><b>100.000'den fazla gönderimden test edilen <img src="https://img.shields.io/badge/12809-Oyun-blue.svg"> listesi için <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a>'ye göz atın </b></h3>
<p>&nbsp;</p>

<p align="center"><img src="CompactGUI/assets/Watcher.png" width="750"/></p>

## Önemli Uyarı

**Bu araç, Windows 11'de DirectStorage kullanan oyunlarda kullanılmamalıdır.**

DirectStorage, oyunların varlıkları CPU'yu atlayarak doğrudan SSD'den yüklemesine olanak tanıyan yeni bir API'dir. Sıkıştırılmış dosyaların GPU'ya gönderilmeden önce açılması gerekecektir, bu da herhangi bir performans kazanımını ortadan kaldıracaktır.

## Arkaplan

Windows 10, diskteki klasörleri ve dosyaları sıkıştırmaya ve çalışma zamanında açmaya olanak tanıyan `compact.exe` adlı az bilinen ancak çok kullanışlı bir araç tanıttı. Herhangi bir modern CPU ile bu ek yük neredeyse hiç fark edilmez ve alan tasarrufu en çok küçük SSD'lere sahip olanlar için yararlıdır.

Program klasörleri ve oyunlar %60'a kadar küçültülebildiğinden, bu durum - özellikle yavaş HDD'lerde - yükleme sürelerini potansiyel olarak azaltma gibi ek bir avantaja sahiptir.

Daha fazla bilgi için [buraya](https://technet.microsoft.com/library/bb490884.aspx) ve [buraya](https://msdn.microsoft.com/library/windows/desktop/hh920921(v=vs.85).aspx) bakabilir veya komut satırına <code>compact /q</code> yazabilirsiniz.

Bu araç kasıtlı olarak sadece klasörleri ve dosyaları sıkıştırmak için tasarlanmıştır. Tüm sürücüler ve tüm Windows kurulumları CompactGUI içinden değiştirilemez - bu işlevselliği arayan kullanıcılar komut satırından <code>compact /compactOS</code> kullanmalıdır.

Sıkıştırma tamamen şeffaftır - programlara, oyunlara ve dosyalara normal şekilde erişilebilir ve Gezgin'de normalde oldukları gibi görünürler; sadece çalışma zamanında RAM'e açılırlar, diskte sıkıştırılmış halde kalırlar.

## Sıkıştırma Modları

Varsayılan olarak program, `XPRESS8K` algoritması aktifken Compact'ı çalıştırır. Bu, sıkıştırma hızı ve boyut azaltma arasında iyi bir denge sağlar. Windows'un kullandığı varsayılan `XPRESS4K`'dır, daha hızlıdır ancak daha az sıkıştırır.

Opsiyonel Sıkıştırma Modları:

Algoritma|Temel Avantajlar|Detaylı Açıklama
:---|:---|:---
XPRESS4K|En hızlı, ancak en zayıf|Aşırı yüksek okuma hızı gereksinimi olan oyun dosyaları için uygundur, sıkıştırırken performansı maksimize edebilir.
XPRESS8K|Hız ve sıkıştırma dengesi|Sıkıştırma hızı ve sıkıştırma oranı arasında daha iyi bir denge sağlanmıştır.
XPRESS16K|Daha yavaş, ancak daha güçlü|Depolama alanının kısıtlı olduğu ve yükleme hızı gereksinimlerinin düşük olduğu senaryolar için uygundur.
LZX|En yavaş, ancak en güçlü|Arşivlenen dosyaları, yedekleme verilerini veya sık erişilmeyen soğuk verileri saklamak için uygundur.

 ---

### Ekran Görüntüleri
<p align="center"><img src="CompactGUI/assets/Compression.png" width="750"/></p>
<p align="center"><img src="CompactGUI/assets/Database.png" width="750"/></p>

### Bu projeyi beğendiniz mi?

 Lütfen Ko-Fi üzerinden bir bahşiş bırakmayı düşünün :)

 <p align="center"><a href='https://ko-fi.com/iridiumio' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Ko-fi.com' /></a></p>

 ---

 **Türkçe Çeviri: [Abdullah Ertürk](https://github.com/abdullah-erturk)**

 <p align="center"><a href='https://buymeacoffee.com/abdullaherturk' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png' border='0' alt='Buy Me A Coffee' /></a></p>
