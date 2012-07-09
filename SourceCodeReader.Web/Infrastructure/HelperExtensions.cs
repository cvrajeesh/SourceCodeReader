using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;

namespace SourceCodeReader.Web.Infrastructure
{
    public static class HelperExtensions
    {
        public static void EnsureDirectoryExists(this string path)
        {
            var directoryPath = path;
          
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static bool IsFilePath(this string path)
        {
            // get the file attributes for file or directory
            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return false;

            return true;
        }

        public static string Cookie(this HttpRequestMessage request, string name)
        {
            string selectedCookieValue = null;
            var result = new Collection<CookieHeaderValue>();
            IEnumerable<string> cookies;
            if (request.Headers.TryGetValues("Cookie", out cookies))
            {
                foreach (string cookie in cookies)
                {
                    CookieHeaderValue cookieHeaderValue;
                    if (CookieHeaderValue.TryParse(cookie, out cookieHeaderValue))
                    {
                        selectedCookieValue = cookieHeaderValue[name].Value;
                        if (!string.IsNullOrEmpty(selectedCookieValue))
                        {
                            break;
                        }
                    }
                }
            }

            return selectedCookieValue;
        }
    }
}