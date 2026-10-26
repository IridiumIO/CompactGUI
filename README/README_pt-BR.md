<p align="center"><img src="https://github.com/IridiumIO/CompactGUI/assets/1491536/64f66b5d-0710-4f66-8b88-6a69f7eb9b63" width="500"></p>

<p align="center">
  <a href="https://github.com/IridiumIO/CompactGUI/releases">
    <img alt="GitHub Downloads (all assets, all releases)" src="https://img.shields.io/github/downloads/IridiumIO/CompactGUI/total?style=for-the-badge&logo=github"></a>&nbsp;&nbsp;&nbsp;&nbsp;
  
  <a href="https://github.com/IridiumIO/CompactGUI/releases">
    <img alt="GitHub Release" src="https://img.shields.io/github/v/release/IridiumIO/CompactGUI?style=for-the-badge"></a>
  </br> 
</p>



<p align="center"><b>O CompactGUI comprime seus jogos e programas de forma transparente, reduzindo o espaço que ocupam sem afetar sua funcionalidade. Ele trabalha diretamente com a API Win32 para alcançar a mesma coisa que a ferramenta de linha de comando nativa compact.exe disponível a partir do Windows 10.</b></p>

&nbsp;
&nbsp;

<p align="center"><img src="CompactGUI/assets/Home.png" width="750"/></p>
<p align="center">
<img src="CompactGUI/assets/Compression.png" width="250"/>
<img src="CompactGUI/assets/Database.png" width="250"/>
<img src="CompactGUI/assets/Watcher.png" width="250"/>
</p>

---

<p align="center">
  <a href="README/"><img src="https://img.shields.io/badge/Readme%20Translations-606070?style=for-the-badge&logo=data:image/svg%2bxml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIGhlaWdodD0iNDhweCIgdmlld0JveD0iMCAtOTYwIDk2MCA5NjAiIHdpZHRoPSI0OHB4IiBmaWxsPSIjYTFlMWZmIj48cGF0aCBkPSJNMzIzLTExMS41UTI1MC0xNDMgMTk2LTE5N3QtODUtMTI3LjVRODAtMzk4IDgwLTQ4MnQzMS0xNTYuNVExNDItNzExIDE5Ni03NjV0MTI3LTg0LjVRMzk2LTg4MCA0ODAtODgwdDE1NyAzMC41UTcxMC04MTkgNzY0LTc2NXQ4NSAxMjYuNVE4ODAtNTY2IDg4MC00ODJ0LTMxIDE1Ny41UTgxOC0yNTEgNzY0LTE5N3QtMTI3IDg1LjVRNTY0LTgwIDQ4MC04MHQtMTU3LTMxLjVaTTQ4MC0xMzhxMzUtMzYgNTguNS04Mi41VDU3Ny0zMzFIMzg0cTE0IDYwIDM3LjUgMTA4dDU4LjUgODVabS04NS0xMnEtMjUtMzgtNDMtODJ0LTMwLTk5SDE3MnEzOCA3MSA4OCAxMTEuNVQzOTUtMTUwWm0xNzEtMXE3Mi0yMyAxMjkuNS02OVQ3ODgtMzMxSDYzOXEtMTMgNTQtMzAuNSA5OFQ1NjYtMTUxWk0xNTItMzkxaDE1OXEtMy0yNy0zLjUtNDguNVQzMDctNDgycTAtMjUgMS00NC41dDQtNDMuNUgxNTJxLTcgMjQtOS41IDQzdC0yLjUgNDVxMCAyNiAyLjUgNDYuNVQxNTItMzkxWm0yMjEgMGgyMTVxNC0zMSA1LTUwLjV0MS00MC41cTAtMjAtMS0zOC41dC01LTQ5LjVIMzczcS00IDMxLTUgNDkuNXQtMSAzOC41cTAgMjEgMSA0MC41dDUgNTAuNVptMjc1IDBoMTYwcTctMjQgOS41LTQ0LjVUODIwLTQ4MnEwLTI2LTIuNS00NXQtOS41LTQzSDY0OXEzIDM1IDQgNTMuNXQxIDM0LjVxMCAyMi0xLjUgNDEuNVQ2NDgtMzkxWm0tMTAtMjM5aDE1MHEtMzMtNjktOTAuNS0xMTVUNTY1LTgxMHEyNSAzNyA0Mi41IDgwVDYzOC02MzBabS0yNTQgMGgxOTRxLTExLTUzLTM3LTEwMi41VDQ4MC04MjBxLTMyIDI3LTU0IDcxdC00MiAxMTlabS0yMTIgMGgxNTFxMTEtNTQgMjgtOTYuNXQ0My04Mi41cS03NSAxOS0xMzEgNjR0LTkxIDExNVoiLz48L3N2Zz4="/></a>
</p>

<p align="center">
  <a href="../README.md">English</a> -
  <a href="README_ru.md">Русский</a> -
  <a href="README_cn.md">简体中文</a> -
  <a href="README_it.md">Italian</a> -
  <a href="README_id.md">Bahasa Indonesia</a> -
  <a href="README_ja.md">日本語</a> -
  Português
</p>

&nbsp;

**Como funciona:**

O CompactGUI é uma interface amigável que utiliza os algoritmos de compressão de sistema de arquivos expostos pelo driver Windows Overlay Filter (WOF), utilizando compressão de alto desempenho introduzida pela primeira vez no Windows 10. Permite que a compressão de qualquer arquivo ou pasta (com foco em jogos) ocorra de forma transparente, sem qualquer perda de desempenho resultante e permitindo grandes economias de armazenamento em disco.

**Transparente? O que isso significa?**

Compressão transparente significa que os arquivos ainda podem ser usados normalmente no computador, como se nada tivesse acontecido - eles não são reempacotados como arquivos Zip e Rar. Você ainda pode navegar, iniciar jogos e programas exatamente como antes, exceto que ocupam menos espaço.

**Como isso é diferente da compressão integrada em versões antigas do Windows?**

Isso é similar à compressão antiga integrada ao Windows (clique com o botão direito > Propriedades > Comprimir para economizar espaço), no entanto os novos algoritmos introduzidos no Windows 10+ são muito superiores, resultando em maiores razões de compressão com quase nenhum impacto de desempenho. [Mais informações podem ser encontradas aqui](https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx)

<h2>Instalação</h2>

####

<img alt="Static Badge" src="https://img.shields.io/badge/DOWNLOAD%20From%20Github-steelblue?style=for-the-badge&logo=github&link=https%3A%2F%2Fgithub.com%2FIridiumIO%2FCompactGUI%2Freleases">

Ou instale com o Winget:

```py
winget install CompactGUI
```

## Usos

Use esta ferramenta para comprimir pastas mantendo a possibilidade de usá-las e abri-las normalmente:

- Reduzir o tamanho de jogos (ex: ARK-Survival Evolved: 169 GB > 91.2 GB)
- Reduzir o tamanho de programas (ex: Adobe Photoshop: 1.71 GB > 886 MB)
- Comprimir qualquer outra pasta no seu computador

## Recursos Extras

- Feedback visual sobre o progresso da compressão e estatísticas
- Lista configurável de tipos de arquivos que comprimem mal e podem ser pulados, editável por pasta
- Estimativa de compressão - construída com mais de 100.000 submissões da comunidade (honestamente foi muito mais, mas eu não percebi que o Google Forms quebrou em 100.000 então perdi muitas submissões) para dados precisos em muitos jogos Steam
  - Jogos não Steam ainda podem utilizar uma estimativa algorítmica em vez disso que ainda dá uma ideia razoável de compressibilidade.
  - Se você quiser contribuir, os resultados de jogos Steam podem ser submetidos ao banco de dados a partir do CompactGUI
- Integração nos menus de contexto do Explorador do Windows para uso mais fácil.
- Analisar o status de pastas existentes
- Observador em segundo plano - monitora pastas e acompanha alterações (ex: atualizações de jogos Steam) e as mantém comprimidas automaticamente em segundo plano.

<h4 align="center"><b>Veja o <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a> para uma lista de <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results"><img src="https://img.shields.io/badge/12809-Games-blue.svg"></a> que foram testados a partir de mais de 100.000 submissões </b></h4>
<p>&nbsp;</p>

## Advertência

**Esta ferramenta não deve ser usada em jogos que utilizam DirectStorage no Windows 11.**

DirectStorage é uma nova API que permite que jogos carreguem ativos diretamente do SSD, ignorando a CPU. Arquivos comprimidos precisarão ser descomprimidos antes de serem enviados para a GPU, o que anulará quaisquer ganhos de desempenho.

## Histórico

O Windows 10 introduziu uma ferramenta pouco conhecida, mas muito útil chamada compact.exe que permite comprimir pastas e arquivos em disco, descomprimindo-os em tempo de execução. Com qualquer CPU moderna (testei até mesmo em um i3-370M de 2010 com impacto negligenciável), esta carga adicionada quase não é notada, e as economias de espaço são de maior uso para aqueles com SSDs menores.

Como pastas de programas e jogos podem ser reduzidas em até 60%, isso tem o bônus adicional de reduzir potencialmente os tempos de carregamento - especialmente em HDDs mais lentos.

Mais informações sobre a função integrada do Windows podem ser encontradas [aqui](https://technet.microsoft.com/library/bb490884.aspx) e [aqui](https://msdn.microsoft.com/library/windows/desktop/hh920921(v=vs.85).aspx) ou digitando compact /q na linha de comando.

Esta ferramenta foi projetada intencionalmente apenas para comprimir pastas e arquivos. Discos inteiros e instalações completas do Windows não podem ser modificados a partir do CompactGUI - usuários que necessitam dessa funcionalidade devem usar compact /compactOS a partir da linha de comando.

A compressão é totalmente transparente - programas, jogos e arquivos ainda podem ser acessados normalmente, e aparecem no Explorador como de costume - eles simplesmente são descomprimidos na RAM em tempo de execução, permanecendo comprimidos no disco.

## Modos de Compressão

Por padrão, o programa executa o Compact com o algoritmo XPRESS8K ativo. Isso proporciona um bom equilíbrio entre velocidade de compressão e redução de tamanho. O padrão que o Windows usa é XPRESS4K que é mais rápido, mas comprime menos.

Modos Opcionais de Compressão:

| Algoritmo | Vantagens Principais | Descrição Detalhada |
| :-------- | :------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| XPRESS4K  | Mais rápido, mas mais fraco | Adequado para arquivos de jogos com requisitos extremamente altos de velocidade de leitura, pode maximizar o desempenho enquanto comprime. |
| XPRESS8K  | Equilíbrio entre velocidade e compressão | Um melhor equilíbrio foi alcançado entre velocidade de compressão e razão de compressão. |
| XPRESS16K | Mais lento, mas mais forte | Adequado para cenários com espaço de armazenamento limitado e baixos requisitos de velocidade de carregamento. |
| LZX       | Mais lento, mas mais forte | Adequado para armazenar arquivos arquivados, dados de backup ou dados "frios" que não são acessados com frequência. |

---

### Gostou deste projeto?

Por favor, considere deixar uma gorjeta no Ko-Fi :)

 <p align="center"><a href='https://ko-fi.com/iridiumio' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Compre um café para mim no ko-fi.com' /></a></p>

