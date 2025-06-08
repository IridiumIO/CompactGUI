using System.Runtime.InteropServices;
using Windows.Win32;

namespace CompactGUI.Core;

public static class WOFHelper
{

    public const UInt64 WOF_PROVIDER_FILE = 2;
    public const UInt32 FSCTL_DELETE_EXTERNAL_BACKING = 0x90314;


    public static WOFCompressionAlgorithm WOFConvertCompressionLevel(int level)
    {
        return level switch
        {
            0 => WOFCompressionAlgorithm.XPRESS4K,
            1 => WOFCompressionAlgorithm.XPRESS8K,
            2 => WOFCompressionAlgorithm.XPRESS16K,
            3 => WOFCompressionAlgorithm.LZX,
            _ => WOFCompressionAlgorithm.XPRESS4K
        };
    }

    public static WOFCompressionAlgorithm WOFConvertCompressionLevel(CompressionMode mode)
    {
        return mode switch
        {
            CompressionMode.XPRESS4K => WOFCompressionAlgorithm.XPRESS4K,
            CompressionMode.XPRESS8K => WOFCompressionAlgorithm.XPRESS8K,
            CompressionMode.XPRESS16K => WOFCompressionAlgorithm.XPRESS16K,
            CompressionMode.LZX => WOFCompressionAlgorithm.LZX,
            _ => WOFCompressionAlgorithm.XPRESS4K
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WOF_FILE_COMPRESSION_INFO_V1
    {
        public UInt32 Algorithm;
        public UInt32 Flags;
    }


    public static unsafe WOFCompressionAlgorithm DetectCompression(FileInfo fileInfo)
    {
        Windows.Win32.Foundation.BOOL isExternalFile;
        UInt32 provider;
        WOFHelper.WOF_FILE_COMPRESSION_INFO_V1 info;
        uint buffer = 8;

        var ret = PInvoke.WofIsExternalFile(fileInfo.FullName, &isExternalFile, &provider, &info, &buffer);

        WOFCompressionAlgorithm algorithm = (WOFCompressionAlgorithm)info.Algorithm;

        if (!isExternalFile) algorithm = WOFCompressionAlgorithm.NO_COMPRESSION;
        if ((fileInfo.Attributes & FileAttributes.Compressed) != 0) algorithm = WOFCompressionAlgorithm.LZNT1;

        return algorithm;

    }
}
