using System.Runtime.InteropServices;

namespace CompactGUI.Core;

internal static class NTFSInterop
{

    internal const uint FSCTL_GET_RETRIEVAL_POINTERS = 0x90073;
    internal const int OPEN_EXISTING = 3;
    internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
    internal const int FILE_SHARE_READ = 1;
    internal const int FILE_SHARE_WRITE = 2;
    internal const uint GENERIC_READ = 0x80000000;

    [StructLayout(LayoutKind.Sequential)]
    internal struct STARTING_VCN_INPUT_BUFFER
    {
        public long StartingVcn;
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct RETRIEVAL_POINTERS_BUFFER
    {
        public int ExtentCount;
        public long StartingVcn;
        public LCN_EXTENT Extents; // This is actually an array, but we only need the first. ?Perhaps I should get the LCN of the middle cluster in future?
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct LCN_EXTENT
    {
        public long NextVcn;
        public long Lcn;
    }

}
