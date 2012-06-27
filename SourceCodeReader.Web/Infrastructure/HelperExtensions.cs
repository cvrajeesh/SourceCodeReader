using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

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
    }
}