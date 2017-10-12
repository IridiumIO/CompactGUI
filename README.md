# CompactGUI
This tool is a standalone visual interface to make using the Windows Compact function more availble to more people. It intentionally only exposes the ability to compress *folders*. Whole drives and entire Windows installations cannot be modified from this tool - users seeking that functionality should use the command-line version built into Windows.

## Installation
Download the standalone program from [Github Releases](https://github.com/ImminentFate/CompactGUI/releases)

## Uses
Use this tool to: 
- Compress program folders (e.g. Adobe Photoshop: 1.71 GB --> 886 MB)
- Compress game install folders (e.g. Portal 2: 11.8 GB --> 7.88 GB)
- Compress any other folder on your computer. 

For most modern computers with a reasonable CPU, there will be no (or very little) performance loss. Those with older HDDs may even see a performance gain in the form of reduced loading times as the smaller files means it takes less time to read programs and games into RAM. 

![Screenshot](https://raw.githubusercontent.com/ImminentFate/CompactGUI/master/WindowsApp1/Resources/SC.png)

## Background
Windows 10 includes a little-known but very useful tool called Compact that allows one to compress folders and files on disk, decompressing them at runtime. With any modern CPU, this added load is hardly noticed, and the space savings are of most use on those with smaller SSDs. 

As program folders and games can be shrunk by up to 60%, this has the added bonus of potentially reducing load times - especially on slower HDDs. 

More information on the inbuilt Windows function can be found [here](https://technet.microsoft.com/en-au/library/bb490884.aspx)


## Options
By default, the program runs Compact with the `/EXE:XPRESS8K` flag active. This provides a good balance between compression speed and size reduction. The default that Windows uses is `/EXE:XPRESS4K`.
The options available are: 
- XPRESS4K: Fastest, but doesn't compress as much
- XPRESS8K: Decent balance between speed and compression
- XPRESS16K: Slower, but compresses the most
- LZX: Okay technically this one compresses the most, BUT it is for compressing folders that are hardly used and are just being stored. Please don't use this on game or program folders.

### Additional Notes

In my testing, using any of the XPRESS modes has no discernible impact on CPU performance when the compressed program is run (Using an i7-6700HQ). However, if your processor is especially old, you may find that performance is worse when folders are compressed with 8K and 16K. Use 4K instead. 

Savings will vary depending on the program or game in question. Some will see massive reductions in size, while others will see little change. For example, trying to compress GTA V will only save a few hundred megabytes.
