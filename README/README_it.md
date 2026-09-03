<p align="center"><img src="https://github.com/IridiumIO/CompactGUI/assets/1491536/64f66b5d-0710-4f66-8b88-6a69f7eb9b63" width="500"></p>

<p align="center">
<a href="https://github.com/IridiumIO/CompactGUI/releases">
<img alt="GitHub Downloads (all assets, all releases)" src="https://img.shields.io/github/downloads/IridiumIO/CompactGUI/total?style=for-the-badge&logo=github">
<img alt="GitHub Release" src="https://img.shields.io/github/v/release/IridiumIO/CompactGUI?style=for-the-badge">
</a>
</br>
</p>

<p align="center"><b>CompactGUI comprime in modo trasparente i tuoi giochi e programmi, riducendo lo spazio che occupano senza comprometterne la funzionalità. Funziona direttamente con l'API Win32 per ottenere lo stesso risultato del tool nativo a riga di comando <code>compact.exe</code> disponibile da Windows 10 in poi.</b></p>

&nbsp;
&nbsp;

---

<p align="center">
    <a href="README.md">English</a> -
    <a href="README_ru.md">Русский</a> -
    <a href="README_cn.md">简体中文</a> -
    <a href="README_it.md">Italian</a>
</p>
&nbsp;

**Come funziona** :

CompactGUI è un'interfaccia intuitiva che sfrutta gli algoritmi di compressione del filesystem esposti dal driver Windows Overlay Filter (WOF), utilizzando una compressione ad alte prestazioni introdotta per la prima volta in Windows 10. Consente di comprimere qualsiasi file o cartella (con particolare attenzione ai giochi) in modo trasparente, senza alcuna perdita di prestazioni e permettendo notevoli risparmi di spazio su disco.

**Trasparente? Cosa significa?**

La compressione trasparente significa che i file possono ancora essere utilizzati normalmente sul computer come se non fosse successo niente: non vengono impacchettati come i file Zip o Rar. Puoi ancora sfogliare, avviare giochi e programmi esattamente come prima, solo che occupano meno spazio.

**In cosa differisce dalla compressione integrata nelle versioni precedenti di Windows?**

Questo è _simile_ alla vecchia compressione integrata in Windows (Tasto destro > Proprietà > Comprimi per risparmiare spazio), tuttavia i nuovi algoritmi introdotti in Windows 10+ sono di gran lunga superiori, ottenendo rapporti di compressione migliori con quasi nessun impatto sulle prestazioni. [Qui puoi trovare maggiori informazioni](<https://msdn.microsoft.com/it-it/library/windows/desktop/hh920921(v=vs.85).aspx>)

<h2>Installazione </h>

####

<img alt="Static Badge" src="https://img.shields.io/badge/DOWNLOAD%20From%20Github-steelblue?style=for-the-badge&logo=github&link=https%3A%2F%2Fgithub.com%2FIridiumIO%2FCompactGUI%2Freleases">

Oppure installa con Winget:

```py
winget install CompactGUI
```

## Utilizzi

Usa questo strumento per comprimere cartelle rimanendo comunque in grado di utilizzarle/avviarle normalmente:

- Ridurre la dimensione dei giochi (es. ARK-Survival Evolved: 169 GB > 91,2 GB)
- Ridurre la dimensione dei programmi (es. Adobe Photoshop: 1,71 GB > 886 MB)
- Comprimere qualsiasi altra cartella sul tuo computer

## Funzionalità aggiuntive

- Feedback visivo sull'avanzamento e sulle statistiche della compressione
- Elenco configurabile di tipi di file scarsamente compressi che possono essere saltati, modificabile per cartella
- Stima della compressione: realizzata utilizzando oltre 100.000 invii dalla community (onestamente molti di più, ma non mi sono accorto che Google Forms si è interrotto a 100.000, quindi ho perso molti invii) per dati accurati su molti giochi Steam
- I giochi non-Steam possono comunque utilizzare una stima algoritmica che fornisce un'idea ragionevole della comprimibilità.
- Se desideri contribuire, i risultati dei giochi Steam possono essere inviati al database online direttamente da CompactGUI
- Integrazione nei menu contestuali di Windows Explorer per un utilizzo più semplice.
- Analizzare lo stato delle cartelle esistenti
- Watcher in background: tiene traccia delle cartelle e le monitora per rilevare modifiche (es. aggiornamenti di giochi Steam) e le mantiene automaticamente compresse in background.

<h4 align="center"><b>Vedi il <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a> per un elenco di <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results"><img src="https://img.shields.io/badge/12809-Giochi-blue.svg"></a> che sono stati testati da oltre 100.000 invii </b></h3>
<p>&nbsp;</p>

## Avvertenza

**Questo strumento non deve essere utilizzato su giochi che sfruttano DirectStorage su Windows 11.**

DirectStorage è una nuova API che consente ai giochi di caricare asset direttamente dall'SSD, bypassando la CPU. I file compressi dovranno essere decompressi prima di essere inviati alla GPU, il che annullerà eventuali guadagni prestazionali.

## Background

Windows 10 ha introdotto un tool poco noto ma molto utile chiamato \`compact.exe\` che consente di comprimere cartelle e file su disco, decomprimendoli in fase di esecuzione. Con qualsiasi CPU moderna (ho testato fino a un i3-370M del 2010 con impatto trascurabile), questo carico aggiuntivo passa quasi inosservato, e il risparmio di spazio è particolarmente utile per chi ha SSD più piccoli.

Poiché le cartelle dei programmi e i giochi possono essere ridotti fino al 60%, questo ha il vantaggio aggiuntivo di ridurre potenzialmente i tempi di caricamento, soprattutto sugli HDD più lenti.

Maggiori informazioni sulla funzione integrata di Windows possono essere trovate [qui](https://technet.microsoft.com/library/bb490884.aspx) e [qui](<https://msdn.microsoft.com/library/windows/desktop/hh920921(v=vs.85).aspx>) o digitando \`compact /q\` nella riga di comando

Questo strumento è stato progettato intenzionalmente per comprimere solo cartelle e file. Non è possibile modificare intere unità e installazioni complete di Windows da CompactGUI: gli utenti che cercano questa funzionalità devono utilizzare \`compact /compactOS\` dalla riga di comando.

La compressione è completamente trasparente: programmi, giochi e file possono ancora essere accessibili normalmente e apparire in Explorer come sempre — verranno semplicemente decompressi nella RAM in fase di esecuzione, rimanendo compressi su disco.

## Modalità di compressione

Per impostazione predefinita, il programma esegue Compact con l'algoritmo \`XPRESS8K\` attivo. Questo offre un buon equilibrio tra velocità di compressione e riduzione delle dimensioni. L'impostazione predefinita di Windows è \`XPRESS4K\` che è più veloce ma comprime meno.

Modalità di compressione opzionali:

| Algoritmo | Vantaggi principali                    | Descrizione dettagliata                                                                                                                      |
| :-------- | :------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
| XPRESS4K  | Più veloce, ma più debole              | Adatto per file di gioco con requisiti di velocità di lettura estremamente elevati, può massimizzare le prestazioni durante la compressione. |
| XPRESS8K  | Equilibrio tra velocità e compressione | È stato raggiunto un migliore equilibrio tra velocità di compressione e rapporto di compressione.                                            |
| XPRESS16K | Più lento, ma più forte                | Adatto per scenari con spazio di archiviazione limitato e requisiti di velocità di caricamento bassi.                                        |
| LZX       | Il più lento, ma il più forte          | Adatto per archiviare file, dati di backup o dati freddi che non vengono acceduti frequentemente.                                            |

---

### Ti piace questo progetto?

Per favore considera di lasciare una mancia su Ko-Fi :)

<p align="center"><a href='https://ko-fi.com/iridiumio' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Offrimi un caffè su ko-fi.com' /></a></p>
