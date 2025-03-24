using System.IO;

namespace DiscUtils.Hpfs;

internal interface IHpfsContext
{
    AllocateFileFn AllocateFile { get; }

    AttributeDefinitions AttributeDefinitions { get; }

    BiosParameterBlock BiosParameterBlock { get; }

    ClusterBitmap ClusterBitmap { get; }

    ForgetFileFn ForgetFile { get; }

    GetDirectoryByIndexFn GetDirectoryByIndex { get; }

    GetDirectoryByRefFn GetDirectoryByRef { get; }

    GetFileByIndexFn GetFileByIndex { get; }

    GetFileByRefFn GetFileByRef { get; }

    HpfsOptions Options { get; }
    
    Stream RawStream { get; }

    bool ReadOnly { get; }
}