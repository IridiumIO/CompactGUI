using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Microsoft.Win32.SafeHandles;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using Windows.Win32;

namespace CompactGUI.Core;

public class Estimator
{

    private const int BlockSize = 8192;
    private const int SampleSize = 100;

    private int DiskClusterSize = 4096;
    private bool isHDD = false;

    public delegate Stream CompressionStreamFactory(Stream output);

    private record FileDetails(AnalysedFileDetails AnalysedFile, float CompressionRatio);

    public List<(AnalysedFileDetails AnalysedFile, float CompressionRatio)> EstimateCompression(
        List<AnalysedFileDetails> analysedFileDetails,
        bool isHdd,
        int maxParallelism = 1,
        int clusterSize = 4096,
        CancellationToken? cToken = null)
    {

        DiskClusterSize = clusterSize;
        if (maxParallelism <= 0) maxParallelism = Environment.ProcessorCount;
        isHDD = isHdd;

        ConcurrentBag<FileDetails> filesList = new();

        var filteredList = analysedFileDetails.Where(f => f.UncompressedSize > clusterSize);
        if (isHdd) filteredList = filteredList.OrderBy(f => GetFirstLcn(f.FileName));
        var finalList = filteredList.ToList();

        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = maxParallelism };

        Parallel.ForEach(finalList, parallelOptions, file =>
        {
            cToken?.ThrowIfCancellationRequested();
            float estimatedRatio = EstimateCompressabilityLZ4(file.FileName, file.UncompressedSize, cToken ?? CancellationToken.None);
            filesList.Add(new FileDetails(file, (float)estimatedRatio));

        });


        return filesList.Select(f => (f.AnalysedFile, f.CompressionRatio)).ToList();

    }

    private float EstimateCompressabilityLZ4(string fileName, long uncompressedSize, CancellationToken cancellationToken)
    {
        try
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                if (isHDD) return EstimateCompressabilityHDD(fs, uncompressedSize,
                    output => LZ4Stream.Encode(output, LZ4Level.L00_FAST, 0,true), cancellationToken);

                return EstimateCompressability(fs, uncompressedSize,
                    output => LZ4Stream.Encode(output, LZ4Level.L00_FAST, 0, true), cancellationToken);
            }


        }
        catch (Exception)
        {

            return 1.0f;
        }
    }

    private float EstimateCompressability(FileStream input, long fileSize, CompressionStreamFactory compressionStreamFactory, CancellationToken cancellationToken)
    {
        long totalBlocks = fileSize / BlockSize;
        int sampleSize = (int)Math.Ceiling((totalBlocks * SampleSize) / (SampleSize + totalBlocks -1.0f));
        long totalWritten = 0;

        MemoryStream compressed = new();

        using (Stream compressionStream = compressionStreamFactory(compressed))
        {
            if (sampleSize == 0 || fileSize < sampleSize * BlockSize * 2)
            {
                Byte[] buffer = new byte[BlockSize];
                input.Position = 0;

                int bytesRead = input.Read(buffer, 0, BlockSize);

                while (bytesRead > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    compressionStream.Write(buffer, 0, bytesRead);
                    totalWritten += bytesRead;
                    bytesRead = input.Read(buffer, 0, BlockSize);
                }

            }
            else
            {
                long stepSize = BlockSize * (totalBlocks / sampleSize);

                Byte[] buffer = new byte[BlockSize];
                for (int i = 0; i < sampleSize; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    input.Position = i * stepSize;
                    int bytesRead = input.Read(buffer, 0, BlockSize);
                    compressionStream.Write(buffer, 0, bytesRead);
                    totalWritten += bytesRead;
                }

            }
        }

        return Math.Min(((float)compressed.Length) / Math.Max(totalWritten, 1), 1.0f);


    }

    private float EstimateCompressabilityHDD(FileStream input, long fileSize, CompressionStreamFactory compressionStreamFactory, CancellationToken cancellationToken)
    {
        int numClusters = SampleSize; // or any small number to sample;
        int clusterSize = DiskClusterSize;

        long middleCluster = fileSize / (2 * clusterSize);
        long alignedStart = middleCluster * clusterSize;
        int chunkSize = (int)Math.Min(clusterSize * numClusters, fileSize - alignedStart);

        long totalWritten = 0;

        MemoryStream compressed = new();

        using (Stream compressionStream = compressionStreamFactory(compressed))
        {
            input.Position = alignedStart;
            Byte[] buffer = new byte[chunkSize];
            int bytesRead = input.Read(buffer, 0, chunkSize);
            
            cancellationToken.ThrowIfCancellationRequested();

            if (bytesRead > 0)
            {
                compressionStream.Write(buffer, 0, bytesRead);
                totalWritten += bytesRead;
            }

        }

        return Math.Min(((float)compressed.Length) / Math.Max(totalWritten, 1), 1.0f);

    }

    unsafe long GetFirstLcn(string fileName)
    {
        SafeFileHandle handle = File.OpenHandle(fileName);

        if (handle.IsInvalid) throw new IOException("Failed to open file handle for " + fileName);

        NTFSInterop.STARTING_VCN_INPUT_BUFFER inBuffer = new() { StartingVcn = 0 };
        uint inBufferSize = (uint)sizeof(NTFSInterop.STARTING_VCN_INPUT_BUFFER);

        int outBufferSize = 4096;
        byte* outBuffer = stackalloc byte[outBufferSize];

        uint bytesReturned = 0;

        var result = PInvoke.DeviceIoControl(
            handle,
            NTFSInterop.FSCTL_GET_RETRIEVAL_POINTERS,
            &inBuffer,
            inBufferSize,
            outBuffer,
            (uint)outBufferSize,
            &bytesReturned,
            null);

        if (!result) return long.MaxValue;

        int extentOffset = Marshal.OffsetOf<NTFSInterop.RETRIEVAL_POINTERS_BUFFER>("Extents").ToInt32();
        long lcn = Marshal.ReadInt64((IntPtr)outBuffer + extentOffset + 8);

        return lcn;

    }




}
