using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using AppUpdate.Sources;
using AppUpdate.Common;
using AppUpdate.Utils;

namespace AppUpdate.Sources
{
    public class BaiduPanSource : IUpdateSource
    {
        public string FeedBaiduPanPath { get; set; }
        public string AccessToken { get; set; }
        public IWebProxy Proxy { get; set; }
        const string _baiduDownloadUrlFormat = @"https://pcs.baidu.com/rest/2.0/pcs/file?method=download&access_token={0}&path={1}";

        public BaiduPanSource(string feedBaiduPanPath, string accessToken)
        {
            FeedBaiduPanPath = feedBaiduPanPath;
            AccessToken = accessToken;
            Proxy = null;
        }

        #region IUpdateSource Members

        public string GetUpdatesFeed()
        {
            string data = string.Empty;

            var request = WebRequest.Create(string.Format(_baiduDownloadUrlFormat, AccessToken, FeedBaiduPanPath));
            request.Method = "GET";
            request.Proxy = Proxy;
            using (var response = request.GetResponse())
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                    using (var reader = new StreamReader(stream, true))
                    {
                        data = reader.ReadToEnd();
                    }
            }

            return data;
        }

        public bool GetData(string fileName, string basePath, Action<UpdateProgressInfo> onProgress, ref string tempLocation)
        {
            // A baseUrl of http://testserver/somefolder with a file linklibrary.dll was resulting in a webrequest to http://testserver/linklibrary
            // The trailing slash is required for the Uri parser to resolve correctly.
            if (!basePath.EndsWith("/")) basePath += "/";
            var fullFilePath = basePath + fileName;
            fullFilePath = fullFilePath.Replace("\\", "/");

            Uri url = new Uri(string.Format(_baiduDownloadUrlFormat, AccessToken, fullFilePath));
            FileDownloader fd = new FileDownloader(url);
            fd.Proxy = Proxy;

            if (string.IsNullOrEmpty(tempLocation) || !Directory.Exists(Path.GetDirectoryName(tempLocation)))
                // WATCHOUT!!! Files downloaded to a path specified by GetTempFileName may be deleted on
                // application restart, and as such cannot be relied on for cold updates, only for hot-swaps or
                // files requiring pre-processing
                tempLocation = Path.GetTempFileName();

            return fd.DownloadToFile(tempLocation, onProgress);         
        }

        #endregion
    }
}
