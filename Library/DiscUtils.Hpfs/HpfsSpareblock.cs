using System;
using DiscUtils.Streams;

namespace DiscUtils.Hpfs;

public enum HpfsSpareblockFlags : byte
{
    HPFS_STATUS_FASTFORMAT = (1 << 5), // Fast format flag
    HPFS_STATUS_OLDFS = (1 << 7), // Written by old ifs
    HPFS_STATUS_BAD_BITMAP = (1 << 4),
    HPFS_STATUS_BAD_SECTOR = (1 << 3),
    HPFS_STATUS_HOTFIX_SECS_USED = (1 << 2),
    HPFS_STATUS_SPARE_DIRBLKS_USED = (1 << 1),
    HPFS_STATUS_DIRTY = (1 << 0) // "Dirty flag" -- this is what causes chkdsk to be run if you pull the plug on the machine
}

public class HpfsSpareblock
{
    private const uint HPFS_SPARE_SIG0 = 0xF9911849;
    private const uint HPFS_SPARE_SIG1 = 0xFA5229C5;

    private const int Signature0Offset = 0x00;
    public uint Signature0;
    private const int Signature1Offset = 0x04;
    public uint Signature1;

    private const int PartitionStatusOffset = 0x08;
    public byte PartitionStatus; // 0x08

    private const int HotfixListOffset = 0x0C;
    public uint HotfixList; // 0x0C, LBA

    private const int HotfixEntriesUsedOffset = 0x10;
    public uint HotfixEntriesUsed; // 0x10, LBA

    private const int TotalHotfixEntriesOffset = 0x14;
    public uint TotalHotfixEntries; // 0x14

    private const int SpareDirblksCountOffset = 0x18;
    public uint SpareDirblksCount; // 0x18

    private const int FreeSpareDirblksOffset = 0x1C;
    public uint FreeSpareDirblks; // 0x1C

    private const int CodePageDirSecOffset = 0x20;
    public uint CodePageDirSec; // 0x20

    private const int TotalCodePagesOffset = 0x24;
    public uint TotalCodePages; // 0x24

    private const int SuperblockCrc32Offset = 0x28;
    public uint SuperblockCrc32; // 0x28, unused except for HPFS386

    private const int SpareblockCrc32Offset = 0x2C;
    public uint SpareblockCrc32; // 0x2C

    private const int ExtraOffset = 0x30;
    public uint[] Extra = new uint[15];

    private const int SpareDirblksOffset = 0x6C;
    public uint[] SpareDirblks; // length is determined by spare_dirblks_count

    public static HpfsSpareblock FromBytes(ReadOnlySpan<byte> bytes)
    {
        HpfsSpareblock spareblock = new HpfsSpareblock();
        spareblock.Signature0 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(Signature0Offset));
        spareblock.Signature1 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(Signature1Offset));

        spareblock.PartitionStatus = bytes[PartitionStatusOffset];
        spareblock.HotfixList = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(HotfixListOffset));
        spareblock.HotfixEntriesUsed = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(HotfixEntriesUsedOffset));
        spareblock.TotalHotfixEntries = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(TotalHotfixEntriesOffset));
        spareblock.SpareDirblksCount = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SpareDirblksCountOffset));
        spareblock.FreeSpareDirblks = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(FreeSpareDirblksOffset));
        spareblock.CodePageDirSec = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(CodePageDirSecOffset));
        spareblock.TotalCodePages = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(TotalCodePagesOffset));
        spareblock.SuperblockCrc32 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SuperblockCrc32Offset));
        spareblock.SpareblockCrc32 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SpareblockCrc32Offset));
        for (int i = 0; i < 15; i++)
        {
            spareblock.Extra[i] = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ExtraOffset + (i * 4)));
        }

        spareblock.SpareDirblks = new uint[spareblock.SpareDirblksCount];
        for (int i = 0; i < spareblock.SpareDirblksCount; i++)
        {
            spareblock.SpareDirblks[i] = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SpareDirblksOffset + (i * 4)));
        }

        return spareblock;
    }
}