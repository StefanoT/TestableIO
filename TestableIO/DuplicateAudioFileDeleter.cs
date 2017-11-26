/*
Project: TestableIO
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
using System.IO.Abstractions;

namespace TestableIO
{
    public class DuplicateAudioFileDeleter
    {
        private readonly IFileSystem fileSystem;
        private readonly HashSet<string> lossyFileExtension = new HashSet<string>() { ".MP3", ".MP4", ".AAC", ".MPC"};
        private readonly HashSet<string> losslessFileExtension = new HashSet<string>() { ".FLAC", ".APE", ".WAV"};

        public DuplicateAudioFileDeleter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public DuplicateAudioFileDeleter() : this(new FileSystem())
        {            
        }

        /// <summary>
        /// check the given path for files with the same name but different formats, and deletes lossy files if a lossless one exists
        /// </summary>
        /// <param name="path">the top level directory to start searching</param>
        /// <remarks>this method will search in all subfolders of the given directory</remarks>
        public void CleanupDirectory(string path)
        {
            HashSet<string> losslessFiles = new HashSet<string>();  // stores full file name without extension of lossless files
            List<string> lossyFiles = new List<string>();  // stores full file name of lossy files

            // get all files from the given path and all subfolders
            var allFiles = fileSystem.Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            // build list of lossy and lossless files
            foreach (var currentFile in allFiles)
            {
                var currentFileExtension = fileSystem.Path.GetExtension(currentFile).ToUpper();
                if (lossyFileExtension.Contains(currentFileExtension))
                {
                    // lossy file found
                    lossyFiles.Add(currentFile);                      
                } else if (losslessFileExtension.Contains(currentFileExtension))
                {
                    // lossless file found                    
                    var currentFileWithoutExtension = fileSystem.Path.Combine(fileSystem.Path.GetDirectoryName(currentFile),
                        fileSystem.Path.GetFileNameWithoutExtension(currentFile));                    
                    losslessFiles.Add(currentFileWithoutExtension);
                }                
                // not an audio file
            }
            // deleted lossy files if a lossless file with the same name exists
            foreach (var currentLossyFile in lossyFiles)
            {
                var currentLossyFileWithoutExtension = fileSystem.Path.Combine(fileSystem.Path.GetDirectoryName(currentLossyFile),
                    fileSystem.Path.GetFileNameWithoutExtension(currentLossyFile));
                if (losslessFiles.Contains(currentLossyFileWithoutExtension))
                {
                    // duplicate file found
                    fileSystem.File.Delete(currentLossyFile);                    
                }
            }
        }        
    }
}
