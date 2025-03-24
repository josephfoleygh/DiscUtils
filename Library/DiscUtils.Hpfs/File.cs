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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Hpfs.Internals;
using DiscUtils.Internal;
using DiscUtils.Streams;
using DiscUtils.Streams.Compatibility;
using Buffer = DiscUtils.Streams.Buffer;

namespace DiscUtils.Hpfs;

internal class File
{
    protected IHpfsContext _context;
    
    public File(IHpfsContext context, FileRecord baseRecord)
    {
        _context = context;
    }
    
    internal IHpfsContext Context => _context;

    public HpfsDirectoryEntry? DirectoryEntry
    {
        get => throw new NotImplementedException();
    }
    
    public bool IsDirectory => throw new NotImplementedException();
    
    
    public void Modified()
    {
    }

    public void Accessed()
    {
    }
    
    public void Delete()
    {

        _context.ForgetFile(this);

    }
    
    public IEnumerable<Range<long, long>> GetClusters(ushort attributeid, AttributeType type)
    {
        var attr = GetAttribute(attributeid, type);

        foreach (var record in attr.Records)
        {
            foreach (var cluster in record.GetClusters())
            {
                yield return cluster;
            }
        }
    }
    
    public override string ToString()
    {
        throw new NullReferenceException();
    }
}