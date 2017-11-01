/*
Project: TestableIOTest
Author: Stefano Tommesani
Website: http://www.tommesani.com
Microsoft Public License (MS-PL) [OSI Approved License]
This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.
2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
*/

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestableIO;

namespace TestableIOTest
{
    [TestClass]
    public class DuplicateAudioFileDeleterTest
    {
        private readonly string testPath = @"c:\";

        [TestMethod]
        public void TestDeleteOfSingleLossyFile()
        {
            var lossyFileName = Path.Combine(testPath, "myfile.mp3");
            var losslessFileName = Path.Combine(testPath, "myfile.flac");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { lossyFileName, new MockFileData("Lossy file") },
                { losslessFileName, new MockFileData("Lossless file") }                 
            });
            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(lossyFileName));

            var audioFileDeleter = new DuplicateAudioFileDeleter(fileSystem);
            audioFileDeleter.CleanupDirectory(testPath);

            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsFalse(fileSystem.FileExists(lossyFileName));
        }

        [TestMethod]
        public void TestDeleteOfMultipleLossyFiles()
        {
            var lossyFileName = Path.Combine(testPath, "myfile.mp3");
            var otherLossyFileName = Path.Combine(testPath, "myfile.aac");
            var losslessFileName = Path.Combine(testPath, "myfile.flac");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { lossyFileName, new MockFileData("Lossy file") },
                { losslessFileName, new MockFileData("Lossless file") },
                { otherLossyFileName, new MockFileData("Lossy file") }
            });
            Assert.IsTrue(fileSystem.FileExists(losslessFileName));            
            Assert.IsTrue(fileSystem.FileExists(lossyFileName));
            Assert.IsTrue(fileSystem.FileExists(otherLossyFileName));

            var audioFileDeleter = new DuplicateAudioFileDeleter(fileSystem);
            audioFileDeleter.CleanupDirectory(testPath);

            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsFalse(fileSystem.FileExists(lossyFileName));
            Assert.IsFalse(fileSystem.FileExists(otherLossyFileName));
        }

        [TestMethod]
        public void TestDeleteWithMultipleLosslessFiles()
        {
            var lossyFileName = Path.Combine(testPath, "myfile.mp3");
            var otherLosslessFileName = Path.Combine(testPath, "myfile.wav");
            var losslessFileName = Path.Combine(testPath, "myfile.flac");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { lossyFileName, new MockFileData("Lossy file") },
                { losslessFileName, new MockFileData("Lossless file") },
                { otherLosslessFileName, new MockFileData("Lossless file") }
            });
            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(lossyFileName));
            Assert.IsTrue(fileSystem.FileExists(otherLosslessFileName));

            var audioFileDeleter = new DuplicateAudioFileDeleter(fileSystem);
            audioFileDeleter.CleanupDirectory(testPath);

            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsFalse(fileSystem.FileExists(lossyFileName));
            Assert.IsTrue(fileSystem.FileExists(otherLosslessFileName));
        }

        [TestMethod]
        public void TestNoDeleteOfFileInDifferentPath()
        {
            var lossyFileName = Path.Combine(testPath, @"lossy\myfile.mp3");
            var losslessFileName = Path.Combine(testPath, "myfile.flac");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { lossyFileName, new MockFileData("Lossy file") },
                { losslessFileName, new MockFileData("Lossless file") }
            });
            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(lossyFileName));

            var audioFileDeleter = new DuplicateAudioFileDeleter(fileSystem);
            audioFileDeleter.CleanupDirectory(testPath);

            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(lossyFileName));
        }

        [TestMethod]
        public void TestNoDeleteOfNonAudioFile()
        {
            var textFileName = Path.Combine(testPath, @"myfile.txt");
            var losslessFileName = Path.Combine(testPath, "myfile.flac");
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { textFileName, new MockFileData("Text file") },
                { losslessFileName, new MockFileData("Lossless file") }
            });
            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(textFileName));

            var audioFileDeleter = new DuplicateAudioFileDeleter(fileSystem);
            audioFileDeleter.CleanupDirectory(testPath);

            Assert.IsTrue(fileSystem.FileExists(losslessFileName));
            Assert.IsTrue(fileSystem.FileExists(textFileName));
        }
    }
}
