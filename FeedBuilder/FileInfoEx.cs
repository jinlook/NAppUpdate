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
			myFileVersion = FileVersionInfo.GetVersionInfo(fileName).FileVersion;
			if (myFileVersion != null) myFileVersion = myFileVersion.Replace(", ", ".");
			myHash = AppUpdate.Utils.FileChecksum.GetSHA256Checksum(fileName);
            RelativeName = fileName.Substring(rootDirLength + 1);
		}
	}
}
