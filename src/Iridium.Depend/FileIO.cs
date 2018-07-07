#region License
//=============================================================================
// Iridium-Core - Portable .NET Productivity Library 
//
// Copyright (c) 2008-2017 Philippe Leybaert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//=============================================================================
#endregion

using System;
using System.IO;

namespace Iridium.Depend
{
    public static class FileIO
    {
#if NETSTANDARD1_3 || NETSTANDARD2_0 || NET45

        private class FileIOHandler : IFileIOHandler
        {
            string IFileIOHandler.ReadAllText(string path) => File.ReadAllText(path);
            string[] IFileIOHandler.ReadAllLines(string path) => File.ReadAllLines(path);
            byte[] IFileIOHandler.ReadAllBytes(string path) => File.ReadAllBytes(path);
            void IFileIOHandler.WriteAllText(string path, string s) => File.WriteAllText(path, s);
            bool IFileIOHandler.FileExists(string path) => File.Exists(path);
            void IFileIOHandler.Delete(string path) => File.Delete(path);
            void IFileIOHandler.CreateFolder(string path) => Directory.CreateDirectory(path);
            void IFileIOHandler.DeleteFolder(string path) => Directory.Delete(path);
            bool IFileIOHandler.FolderExists(string path) => Directory.Exists(path);
            Stream IFileIOHandler.OpenReadStream(string path, bool exclusive) => File.OpenRead(path);
            Stream IFileIOHandler.OpenWriteStream(string path, bool exclusive, bool create) => File.OpenWrite(path);
            void IFileIOHandler.AppendAllText(string path, string s) => File.AppendAllText(path, s);
        }

        static FileIO()
        {
            ServiceLocator.Register(new FileIOHandler()).Replace<IFileIOHandler>();
        }

#endif

        private static IFileIOHandler Handler => ServiceLocator.Get<IFileIOHandler>();

        public static string ReadAllText(string path)
        {
            return Handler.ReadAllText(path);
        }

        public static string[] ReadAllLines(string path)
        {
            return Handler.ReadAllLines(path);
        }

        public static void WriteAllText(string path, string s)
        {
            Handler.WriteAllText(path, s);
        }

        public static bool FileExists(string path)
        {
            return Handler.FileExists(path);
        }

        public static void Delete(string path)
        {
            Handler.Delete(path);
        }

        public static Stream OpenReadStream(string path, bool exclusive)
        {
            return Handler.OpenReadStream(path, exclusive);
        }

        public static Stream OpenWriteStream(string path, bool exclusive, bool create)
        {
            return Handler.OpenWriteStream(path, exclusive, create);
        }

        public static void AppendAllText(string path, string s)
        {
            Handler.AppendAllText(path, s);            
        }
    }
}
