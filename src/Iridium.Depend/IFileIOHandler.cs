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
    public interface IFileIOHandler
    {
        string ReadAllText(string path);
        string[] ReadAllLines(string path);
        byte[] ReadAllBytes(string path);
        void WriteAllText(string path, string s);
        bool FileExists(string path);
        void Delete(string path);
        void CreateFolder(string path);
        void DeleteFolder(string path);
        bool FolderExists(string path);
        Stream OpenReadStream(string path, bool exclusive);
        Stream OpenWriteStream(string path, bool exclusive, bool create);
        void AppendAllText(string path, string s);
    }

    public class FileIOException : Exception
    {
        public FileIOException(string message) : base(message) { }
        public FileIOException(string message, Exception innException) : base(message, innException) { }
    }
}