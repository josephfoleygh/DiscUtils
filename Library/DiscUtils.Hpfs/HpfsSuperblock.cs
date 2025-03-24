using System;
using DiscUtils.Streams;

namespace DiscUtils.Hpfs;

public class HpfsSuperblock
{
    // http://www.edm2.com/0501/hpfs3.html
    private const uint HPFS_SUPER_SIG0 = 0xF995E849;
    private const uint HPFS_SUPER_SIG1 = 0xFA53E9C5;
    private const int SignatureA0Offset = 0x00;
    public uint SignatureA0;
    private const int SignatureA1Offset = 0x04;
    public uint SignatureA1;

    private const int VersionOffset = 0x08;
    public byte Version; // 0x08
    private const int FunctionalVersionOffset = 0x09;
    public byte FunctionalVersion; // 0x09

    private const int RootDirectoryFNodeOffset = 0x0C;
    public uint RootDirectoryFNode; // 0x0C, LBA
    private const int SectorsInPartitionOffset = 0x10;
    public uint SectorsInPartition; // 0x10
    private const int BadSectorCountOffset = 0x14;
    public uint BadSectorCount; // 0x14
    private const int ListBitmapSecsOffset = 0x14;
    public uint ListBitmapSecs; // 0x18, LBA
    private const int BitmapSecSpareOffset = 0x1C;
    public uint BitmapSecsSpare; // 0x1C
    private const int ListBadSecsOffset = 0x20;
    public uint ListBadSecs; // 0x20, LBA
    private const int BadSecsSpareOffset = 0x24;
    public uint BadSecsSpare; // 0x24
    private const int ChkdskLastRunOffset = 0x28;
    public uint ChkdskLastRun; // 0x28
    private const int LastOptimizedOffset = 0x2C;
    public uint LastOptimized; // 0x2C
    private const int DirBandSectorsOffset = 0x10;
    public uint DirBandSectors; // 0x10, LBA
    private const int DirBandStartSecOffset = 0x34;
    public uint DirBandStartSec; // 0x34, LBA
    private const int DirBandEndSecOffset = 0x38;
    public uint DirBandEndSec; // 0x38
    private const int DirBandBitmapOffset = 0x3C;
    public uint DirBandBitmap; // 0x3C

    private const int FirstUidSecOffset = 0x60;
    public uint FirstUidSec; // 0x60, HPFS386 only

    public static HpfsSuperblock FromBytes(ReadOnlySpan<byte> bytes)
    {
        HpfsSuperblock superblock = new HpfsSuperblock();
        superblock.SignatureA0 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SignatureA0Offset));
        superblock.SignatureA1 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SignatureA1Offset));
        
        superblock.Version = bytes[VersionOffset];
        superblock.FunctionalVersion = bytes[FunctionalVersionOffset];
        superblock.RootDirectoryFNode = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(RootDirectoryFNodeOffset));
        superblock.SectorsInPartition = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SectorsInPartitionOffset));
        superblock.BadSectorCount = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(BadSectorCountOffset));
        superblock.ListBitmapSecs = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ListBitmapSecsOffset));
        superblock.BitmapSecsSpare = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(BitmapSecSpareOffset));
        superblock.ListBadSecs = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ListBadSecsOffset));
        superblock.BadSecsSpare = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(BadSecsSpareOffset));
        superblock.ChkdskLastRun = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ChkdskLastRunOffset));
        superblock.LastOptimized = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(LastOptimizedOffset));
        superblock.DirBandSectors = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(DirBandSectorsOffset));
        superblock.DirBandStartSec = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(DirBandStartSecOffset));
        superblock.DirBandEndSec = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(DirBandEndSecOffset));
        superblock.DirBandBitmap = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(DirBandBitmapOffset));
        superblock.FirstUidSec = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(FirstUidSecOffset));
        return superblock;
    }
}