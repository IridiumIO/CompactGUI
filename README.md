<p align="center"><img src="https://user-images.githubusercontent.com/1491536/171987806-e7f290b4-91ed-451c-a7ef-d30f77a24931.svg" width="500"></p>

&nbsp;

<p align="center"><b>CompactGUI is a standalone user interface that makes the Windows 10 compact.exe function easier to use.</b></p> 

------

**Note - v3.0 Complete rewrite is underway as of May 2022 with the following features:**
 - Rebuilt from scratch in .NET 6 using WPF
 - Smoother, simplified UI
 - Parallelised and asynchronous programming resulting in _at least_ an order of magnitude speed improvement. Using ARK as a worst case scenario (160GB with 105,000 files)
     - Analysing: 6.3 seconds (down from 104 seconds)
     - Compressing: 8 minutes (down from 20+ minutes if it didn't just crash first)
 - Automatic skipping of files that are smaller than the disk's cluster size, 4kb by default
 - Saving of poorly compressed filetypes per directory to skip on next run

**Features not implemented yet:**
 - Skipping global poorly compressed filetypes (Settings menu not built yet; folder results can still be submitted)
 - Skipping online-sourced poorly compressed filetypes per game/folder based on previous users (soon)
 - Force action on files / compress sytem files (these radiobuttons are implemented but don't do anything)
 - Compress an already compressed folder to patch files that have changed in the meantime
 - Automatically checking for updates
 - Integration into Explorer context menus
------
&nbsp;

**What is the Windows 10 compact.exe function?**
It's a commandlet with a collection of new algorithms introduced in Windows 10 that allow you to transparently compress games, programs and other folders with virtually no performance loss.

**Transparently? What does that mean?**
Transparent compression means that files can still be used normally on the computer as if nothing had happened - they don't get repackaged like Zip and Rar files do. 

**How is this different from the built-in compression in older versions of Windows?**
This is similar to the NTFS-LZNT1 compression built-in to Windows (Right click > Properties > Compress to save space) however the newer algorithms introduced in Windows 10 are far superior, resulting in greater compression ratios with almost no performance impact.Those with older HDDs may even see a decent performance gain in the form of reduced loading times as the smaller files means it takes less time to read programs and games into RAM.[More information can be found here](https://msdn.microsoft.com/en-us/library/windows/desktop/hh920921(v=vs.85).aspx) 



<h2>Installation  </h> <a href="https://github.com/ImminentFate/CompactGUI/releases"><img src="https://img.shields.io/github/release/ImminentFate/compactgui/all.svg""></a>  <a href="https://chocolatey.org/packages/compactgui/"><img src="https://img.shields.io/chocolatey/v/compactgui.svg""></a>  <a href="https://github.com/ImminentFate/CompactGUI/releases"><img src="https://img.shields.io/github/downloads/ImminentFate/CompactGUI/total.svg""></a>

####
 
<p>Download from <a href="https://github.com/ImminentFate/CompactGUI/releases"><b>GitHub Releases</b></a></p>

Coming soon: Download from Windows 10/11 Store
  
## Uses
Use this tool to: 
- Reduce the size of games (e.g. ARK-Survival Evolved: 169 GB > 91.2 GB)
- Reduce the size of programs (e.g. Adobe Photoshop: 1.71 GB > 886 MB)
- Compress any other folder on your computer
  
## Extra Features
 - Visual feedback on compression progress and statistics
 - Configurable list of poorly-compressed filetypes that can be skipped.
 - Online integration with community-sourced [database](https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results) to get compression estimates
 - Integration into Windows Explorer context menus for easier use.
 - Analyze the status of existing folders
 

<h4 align="center"><b>See the <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results">Wiki</a> for a list of <a href="https://github.com/ImminentFate/CompactGUI/wiki/Community-Compression-Results"><img src="https://img.shields.io/badge/Games-5085-blue.svg"></a> that have been tested</b></h3>
<p>&nbsp;</p>



## Screenshots (v3.0a)

<img src="https://user-images.githubusercontent.com/1491536/171412294-4be4f920-decf-4dc4-94d7-e9f65f9a299b.png" width="300"/><img src="https://user-images.githubusercontent.com/1491536/171412299-9b17884a-1046-485b-8f32-4b018856bfd1.png" width="300"/><img src="https://user-images.githubusercontent.com/1491536/171412306-12ba42a1-fcd4-40cf-a11c-64ac6f139c5e.png" width="300"/>

 
## Background
Windows 10 includes a little-known but very useful tool called Compact.exe that allows one to compress folders and files on disk, decompressing them at runtime. With any modern CPU (I have tested as old as an i3-370M from 2010 with negligible impact), this added load is hardly noticed, and the space savings are of most use on those with smaller SSDs. 

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

  
