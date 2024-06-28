<p align="center"><img src="https://user-images.githubusercontent.com/1491536/171987806-e7f290b4-91ed-451c-a7ef-d30f77a24931.svg" width="500"></p>

&nbsp;

<p align="center"><b>CompactGUI transparently compresses your games and programs reducing the space they use without affecting their functionality. It works directly with the Win32 API to achieve the same thing as the native <code>compact.exe</code> command-line tool available from Windows 10 onwards.</b></p> 

&nbsp;
&nbsp;

<p align="center"><img src="https://user-images.githubusercontent.com/1491536/172040389-62932137-11ae-49c8-8749-95c0b67f3aab.png" width="250"/><img src="https://user-images.githubusercontent.com/1491536/172040455-6cd06756-6323-44da-b350-daa47f31c5e3.png" width="250"/><img src="https://user-images.githubusercontent.com/1491536/172040456-09c069e3-093a-4c5e-8d69-f52d4dc2f982.png" width="250"/></>


------
&nbsp;

**What is `compact.exe`?**
It's a commandlet with a collection of new algorithms introduced in Windows 10 that allow you to transparently compress games, programs and other folders with virtually no performance loss.

**Transparently? What does that mean?**
Transparent compression means that files can still be used normally on the computer as if nothing had happened - they don't get repackaged like Zip and Rar files do. 

**How is this different from the built-in compression in older versions of Windows?**
This is similar to the NTFS-LZNT1 compression built-in to Windows (Right click > Properties > Compress to save space) however the newer algorithms introduced in Windows 10+ are far superior, resulting in greater compression ratios with almost no performance impact.Those with older HDDs may even see a decent performance gain in the form of reduced loading times as the smaller files means it takes less time to read programs and games into RAM. [More information can be found here](https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx) 



<h2>Installation  </h> <a href="https://github.com/ImminentFate/CompactGUI/releases"><img src="https://img.shields.io/github/release/ImminentFate/compactgui/all.svg""></a>  <a href="https://chocolatey.org/packages/compactgui/"><img src="https://img.shields.io/chocolatey/v/compactgui.svg""></a>  <a href="https://github.com/ImminentFate/CompactGUI/releases"><img src="https://img.shields.io/github/downloads/ImminentFate/CompactGUI/total.svg""></a>

####
 
<p>Download from <a href="https://github.com/IridiumIO/CompactGUI/releases"><b>GitHub Releases</b></a></p>
  
## Uses
Use this tool to compress folders while still being able to use/run them normally: 
- Reduce the size of games (e.g. ARK-Survival Evolved: 169 GB > 91.2 GB)
- Reduce the size of programs (e.g. Adobe Photoshop: 1.71 GB > 886 MB)
- Compress any other folder on your computer
  
## Extra Features
 - Visual feedback on compression progress and statistics
 - Configurable list of poorly-compressed filetypes that can be skipped.
 - Online integration with community-sourced [database](https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results) to get compression estimates
      - Steam game results can be submitted to the online database from within CompactGUI 
 - Integration into Windows Explorer context menus for easier use.
 - Analyze the status of existing folders
 - Background monitor to keep track of compressed folders and easily see / recompress them if they've been recently updated (such as Steam games) or decompressed. 
 

## Caveat
  - **This tool should not be used on games that utilise DirectStorage on Windows 11. DirectStorage is a new API that allows games to load assets directly from the SSD, bypassing the CPU. Compressed files will need to be decompressed before being sent to the GPU, which will negate any performance gains.**

<h4 align="center"><b>See the <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a> for a list of <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results"><img src="https://img.shields.io/badge/8530-Games-blue.svg"></a> that have been tested from <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results"><img src="https://img.shields.io/badge/-57947-lightgrey.svg"></a> submissions</b></h3>
<p>&nbsp;</p>




## Background

Windows 10 introduced a little-known but very useful tool called `compact.exe` that allows one to compress folders and files on disk, decompressing them at runtime. With any modern CPU (I have tested as old as an i3-370M from 2010 with negligible impact), this added load is hardly noticed, and the space savings are of most use on those with smaller SSDs. 

As program folders and games can be shrunk by up to 60%, this has the added bonus of potentially reducing load times - especially on slower HDDs. 

More information on the inbuilt Windows function can be found [here](https://technet.microsoft.com/en-au/library/bb490884.aspx) and [here](https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx) or by typing `compact /q` into the commandline

This tool is intentionally designed to only compress folders and files. Whole drives and entire Windows installations cannot be modified from within CompactGUI - users seeking that functionality should use `compact /compactOS` from the commandline. 

The compression is fully transparent - programs, games and files can still be accessed as normal, and show up in Explorer as they normally would — they'll just be decompressed into RAM at runtime, staying compressed on disk.

## Options
By default, the program runs Compact with the `XPRESS8K` algorithm active. This provides a good balance between compression speed and size reduction. The default that Windows uses is `XPRESS4K` which is faster but compresses less. 
The options available are: 
- XPRESS4K: Fastest, but weakest
- XPRESS8K: Reasonable balance between speed and compression
- XPRESS16K: Slower, but stronger
- LZX: Slowest, but strongest - note it has a higher overhead, so use it on programs/games only if your CPU is reasonably strong or the program/game is older. 

 
 -----
 ### Like this project?
 Please consider leaving a tip on Ko-Fi :) 
 
 <p align="center"><a href='https://ko-fi.com/iridiumio' target='_blank'><img height='42' style='border:0px;height:42px;' src='https://cdn.ko-fi.com/cdn/kofi3.png?v=3' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a></p>
  
