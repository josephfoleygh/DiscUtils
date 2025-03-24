//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using DiscUtils.Compression;

namespace DiscUtils.Hpfs;

/// <summary>
/// Class whose instances hold options controlling how <see cref="HpfsFileSystem"/> works.
/// </summary>
public sealed class HpfsOptions : DiscFileSystemOptions
{
    internal HpfsOptions()
    {
        HideHiddenFiles = true;
        HideSystemFiles = true;
        ReadCacheEnabled = true;
    }
    
    /// <summary>
    /// Gets or sets a value indicating whether to include hidden files when enumerating directories.
    /// </summary>
    public bool HideHiddenFiles { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to include system files when enumerating directories.
    /// </summary>
    public bool HideSystemFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether NTFS-level read caching is used.
    /// </summary>
    public bool ReadCacheEnabled { get; set; }

    /// <summary>
    /// Returns a string representation of the file system options.
    /// </summary>
    /// <returns>A string of the form Show: XX XX XX.</returns>
    public override string ToString()
        => $"Show: Normal {(HideHiddenFiles ? string.Empty : "Hidden ")}{(HideSystemFiles ? string.Empty : "System ")}";
}