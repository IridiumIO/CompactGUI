# CompactGUI
This tool is an open-source standalone visual interface to make using the Windows 10 compact.exe function more available to more people. 

The compression used by compact.exe is similar to the built-in NTFS compression in that it is transparent. Compressed files and programs can still be accessed as if nothing has changed and show up in Explorer as they normally would — they'll just be decompressed on the fly at runtime. However, the newer algorithms used by Compact are much more efficient. 

## Installation
Download the standalone program from [Github Releases](https://github.com/ImminentFate/CompactGUI/releases)

## Uses
Use this tool to: 
- Compress program folders (e.g. Adobe Photoshop: 1.71 GB --> 886 MB)
- Compress game install folders (e.g. Portal 2: 11.8 GB --> 7.88 GB)
- Compress any other folder on your computer. 

For most modern computers there will be no (or very little) performance loss. Those with older HDDs may even see a decent performance gain in the form of reduced loading times as the smaller files means it takes less time to read programs and games into RAM.

![Screenshot](https://i.imgur.com/BkDnVa4.png)![Screenshot2](https://i.imgur.com/4fThTKX.png)

## Background
Windows 10 includes a little-known but very useful tool called Compact that allows one to compress folders and files on disk, decompressing them at runtime. With any modern CPU, this added load is hardly noticed, and the space savings are of most use on those with smaller SSDs. 

As program folders and games can be shrunk by up to 60%, this has the added bonus of potentially reducing load times - especially on slower HDDs. 

More information on the inbuilt Windows function can be found [here](https://technet.microsoft.com/en-au/library/bb490884.aspx)

This tool is intentionally designed to only compress folders and files. Whole drives and entire Windows installations cannot be modified from within CompactGUI - users seeking that functionality should use the command-line version built into Windows (this is intentional).

The compression used by compact.exe is similar to the built-in NTFS compression in that it is transparent. Compressed files and programs can still be accessed as if nothing has changed and show up in Explorer as they normally would — they'll just be decompressed on the fly at runtime. However, the newer algorithms are much more efficient than NTFS (LZNT1).

## Options
By default, the program runs Compact with the `/EXE:XPRESS8K` flag active. This provides a good balance between compression speed and size reduction. The default that Windows uses is `/EXE:XPRESS4K`.
The options available are: 
- XPRESS4K: Fastest, but doesn't compress as much
- XPRESS8K: Decent balance between speed and compression
- XPRESS16K: Slower, but compresses the most
- LZX: Okay technically this one compresses the most, BUT it is for compressing folders that are hardly used and are just being stored. Please don't use this on game or program folders.

### Additional Notes

In my testing, using any of the XPRESS modes has no discernible impact on CPU performance when the compressed program is run (Using an i7-6700HQ). Here's the output tests for Adobe Photoshop:
![Photoshop compression results](https://i.imgur.com/ou0D0B1.png)

However, if your processor is especially old, you may find that performance is worse when folders are compressed with 8K and 16K. Use 4K instead. Despite this, I've successfully tested it on an i3-370M from 2010, and it had no issues with performance on any of the compression modes. 
