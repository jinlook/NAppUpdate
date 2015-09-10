using System.Diagnostics;
using System.IO;
using AppUpdate.Utils;

namespace FeedBuilder
{
	public class FileInfoEx
	{
		private readonly FileInfo myFileInfo;
		private readonly string myFileVersion;
		private readonly string myHash;

		public FileInfo FileInfo
		{
			get { return myFileInfo; }
		}

		public string FileVersion
		{
			get { return myFileVersion; }
		}

		public string Hash
		{
			get { return myHash; }
		}

        public string RelativeName { get; private set; }

		public FileInfoEx(string fileName, int rootDirLength)
		{
			myFileInfo = new FileInfo(fileName);
            var verInfo = FileVersionInfo.GetVersionInfo(fileName);
            if (myFileVersion != null)
                myFileVersion = new System.Version(verInfo.FileMajorPart, verInfo.FileMinorPart, verInfo.FileBuildPart, verInfo.FilePrivatePart).ToString();
			myHash = AppUpdate.Utils.FileChecksum.GetSHA256Checksum(fileName);
            RelativeName = fileName.Substring(rootDirLength + 1);
		}
	}
}
