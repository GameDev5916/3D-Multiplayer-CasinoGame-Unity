#if !NETFX_CORE && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.IO;

namespace BestHTTP.PlatformSupport.FileSystem
{
    public sealed class DefaultIOService : IIOService
    {
        public Stream CreateFileStream(string path, FileStreamModes mode)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"CreateFileStream path: '{path}' mode: {mode}");

            switch (mode)
            {
                case FileStreamModes.Create:
                    return new FileStream(path, FileMode.Create);
                case FileStreamModes.Open:
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                case FileStreamModes.Append:
                    return new FileStream(path, FileMode.Append);
            }

            throw new NotImplementedException("DefaultIOService.CreateFileStream - mode not implemented: " + mode.ToString());
        }

        public void DirectoryCreate(string path)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"DirectoryCreate path: '{path}'");
            Directory.CreateDirectory(path);
        }

        public bool DirectoryExists(string path)
        {
            bool exists = Directory.Exists(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"DirectoryExists path: '{path}' exists: {exists}");

            return exists;
        }

        public string[] GetFiles(string path)
        {
            var files = Directory.GetFiles(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"GetFiles path: '{path}' files count: {files.Length}");

            return files;
        }

        public void FileDelete(string path)
        {
            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"FileDelete path: '{path}'");
            File.Delete(path);
        }

        public bool FileExists(string path)
        {
            bool exists = File.Exists(path);

            if (HTTPManager.Logger.Level == Logger.Loglevels.All)
                HTTPManager.Logger.Verbose("DefaultIOService", $"FileExists path: '{path}' exists: {exists}");

            return exists;
        }
    }
}

#endif