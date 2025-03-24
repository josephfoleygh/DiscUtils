using System;
using System.Text;
using DiscUtils.Streams;
using LTRData.Extensions.Buffers;

namespace DiscUtils.Hpfs;

public enum HpfsDirEntFlags : byte
{
/*
16h     (23)    Flags             1       0x01 (0) Special ".." entry
0x02 (1) Has an ACL
0x04 (2) Has a B-tree down-pointer
0x08 (3) Entry is a dummy end record
0x10 (4) Has an EA list
0x20 (5) Has an extended permission list
0x40 (6) Has an explicit ACL
0x80 (7) Has "needed" EAs
*/
    ParentDirectorySpecialEntry = 0x01,
    HasAcl = 0x02,
    HasBtreeDownPointer = 0x04,
    IsDummyEndRecord = 0x08,
    HasExtendedAttributeList = 0x10,
    HasExtendedPermissionList = 0x20,
    HasExplicitAccessControlList = 0x40,
    HasNeededExtendedAttributes = 0x80
}

public enum HpfsDirEntAttributeFlags : byte
{
/*17h     (24)    Attributes        1       0x01 (0) Read-Only
0x02 (1) Hidden
0x04 (2) System
0x08 (3) Not used. Would be Vol attr.
0x10 (4) Directory
0x20 (5) Archive
0x40 (6) Long Name  (bigger than 8.3)
0x80 (7) Reserved*/
    ReadOnly = 0x01,
    Hidden = 0x02,
    System = 0x04,
    NotUsed1 = 0x08,
    Directory = 0x10,
    Archive = 0x20,
    LongName = 0x40,
    Reserved = 0x80
}

public class HpfsDirectoryEntry
{
    /*
    DIRENT #0
    14h     (21)    Entry's Size      2       Always a multiple of 4.
    */
    private const int DirEntSizeOffset = 0x00;
    public ushort DirEntSize;
    
    // 16h     (23)    Flags             1
    private const int FlagsOffset = 0x02;
    public HpfsDirEntFlags Flags;
    
    // 17h     (24)    Attributes        1 
    private const int AttributeFlagsOffset = 0x03;
    public HpfsDirEntAttributeFlags AttributeFlags;

    // 18h     (25)    FNODE LSN         4       FNODE of this entry.
    private const int FNodeLogicalSectorNumberOffset = 0x04;
    public uint FNodeLogicalSectorNumber;

    // 1Ch     (29)    Time Last Mod.    4       Secs since 00:00 1-1-70
    private const int LastModifiedTimeOffset = 0x08;
    public uint LastModifiedTime;

    // 20h     (33)    File Size         4
    private const int FileSizeOffset = 0x0C;
    public uint FileSize;

    // 24h     (37)    Time Last Access  4       Secs since 00:00 1-1-70
    private const int LastAccessTimeOffset = 0x10;
    public uint LastAccessTime;

    // 28h     (41)    Time When Created 4       Secs since 00:00 1-1-70
    private const int CreateTimeOffset = 0x14;
    public uint CreateTime;

    // 2Ch     (45)    EA Size           4       Size of internal or external EAs.
    private const int ExtendedAttributesSizeOffset = 0x18;
    public uint ExtendedAttributesSize;

    // 30h     (49)    Flex              1       3 LSBits: # ACLs. 5 MSBits: reserved
    private const int FlexOffset = 0x1C;
    public byte Flex;

    // 31h     (50)    Code Page Index   1       7 LSBits: CP index. MSBit: DCBS present
    private const int CodePageIndexOffset = 0x1D;
    public byte CodePageIndex;

    // 32h     (51)    Name Size         1       Length of following name
    private const int NameLengthOffset = 0x1E;
    public byte NameLength;
    
    // 33h     (52)    Name              n       Variable length: 0-254
    private const int NameOffset = 0x1F;
    public string Name;

    // 33h+n           ACL               ?       Access Control List info, if present.
    private int AclOffset => NameOffset + NameLength;

    private int AclLength => LogicalSectorNumberDownPointerOffset - AclOffset;
    
    //                Padding                   0-3 bytes to reestablish dword align.
    //x               Down Ptr LSN      4       If present, down ptr to next B-tree node
    private int LogicalSectorNumberDownPointerOffset => DirEntSize - 4;
    private uint LogicalSectorNumberDownPointer;

    public static HpfsDirectoryEntry FromBytes(ReadOnlySpan<byte> bytes)
    {
        HpfsDirectoryEntry hpfsDirectoryEntry = new HpfsDirectoryEntry();

        hpfsDirectoryEntry.DirEntSize = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(DirEntSizeOffset));
        hpfsDirectoryEntry.Flags = (HpfsDirEntFlags)bytes[FlagsOffset];
        hpfsDirectoryEntry.AttributeFlags = (HpfsDirEntAttributeFlags)bytes[AttributeFlagsOffset];
        hpfsDirectoryEntry.FNodeLogicalSectorNumber = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(FNodeLogicalSectorNumberOffset));
        hpfsDirectoryEntry.LastModifiedTime = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(LastModifiedTimeOffset));
        hpfsDirectoryEntry.FileSize = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(FileSizeOffset));
        hpfsDirectoryEntry.LastAccessTime = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(LastAccessTimeOffset));
        hpfsDirectoryEntry.CreateTime = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(CreateTimeOffset));
        hpfsDirectoryEntry.ExtendedAttributesSize = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ExtendedAttributesSizeOffset));
        hpfsDirectoryEntry.Flex = bytes[FlexOffset];
        hpfsDirectoryEntry.CodePageIndex = bytes[CodePageIndexOffset];
        hpfsDirectoryEntry.NameLength = bytes[NameLengthOffset];
        
        var latin1Encoding = EncodingUtilities.GetLatin1Encoding();
        hpfsDirectoryEntry.Name = EncodingExtensions.GetString(latin1Encoding, bytes.Slice(NameOffset, hpfsDirectoryEntry.NameLength));
        
        // TODO: Acl Support

        hpfsDirectoryEntry.LogicalSectorNumberDownPointer = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(hpfsDirectoryEntry.LogicalSectorNumberDownPointerOffset));
        
        return hpfsDirectoryEntry;
    }
}