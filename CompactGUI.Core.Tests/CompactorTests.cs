using Xunit;
using CompactGUI.Core;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CompactGUI.Core.Tests
{
    public class CompactorTests
    {
        [Fact]
        public async Task BuildWorkingFilesList_WithExclusion_ShouldExcludeCorrectFiles()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            var file1 = Path.Combine(tempDir, "test.txt");
            var file2 = Path.Combine(tempDir, "test.log");
            File.WriteAllText(file1, "This is a test file.");
            File.WriteAllText(file2, "This is a log file.");

            var analyser = new Analyser(tempDir, null);
            var compactor = new Compactor(tempDir, WOFCompressionAlgorithm.XPRESS4K, new[] { ".log" }, analyser, null);

            // Act
            var fileDetails = (FileDetails)await compactor.BuildWorkingFilesList();
            var workingFiles = new List<FileDetails>(fileDetails);

            // Assert
            Assert.Single(workingFiles);
            Assert.Equal(file1, workingFiles[0].FileName);

            // Clean up
            Directory.Delete(tempDir, true);
        }
    }
}
