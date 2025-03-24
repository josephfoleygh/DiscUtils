using System;
using DiscUtils.Streams;

namespace DiscUtils.Hpfs;

[Flags]
public enum DirectoryFlag : byte
{
    IsDirectory = 0x01
}

[Flags]
public enum BTreeTypeFlag : byte
{
    FNode = 0x20,
    Alnode = 0x80
}

public class HpfsFNodeHeader
{
    /*
    Offset          Data               Size    Comment
    hex     (dec)                         bytes
        Header
            */

    //    00h     (1)     Signature               4     0xF7E40AAE
    private const uint SignatureValue = 0xF7E40AAE;
    private const int SignatureOffset = 0x00;
    public uint Signature;

    //    04h     (5)     Seq. Read History       4     Not implemented.
    private const int SequentialReadHistoryOffset = 0x04;
    public uint SequentialReadHistory;

    //    08h     (9)     Fast Read History       4     Not Implemented.
    private const int FastReadHistoryOffset = 0x08;
    public uint FastReadHistory;

    //    0Ch     (13)    Name Length             1     0-254.
    private const int NameLengthOffset = 0x0C;
    public byte NameLength;

    //    0Dh     (14)    Name                    15    Last 15 chars. (Full name in DIRBLK.)
    private const int NameOffset = 0x0D;
    private const int LocalNameLength = 15;
    public string Name;

    //    1Ch     (29)    Container Dir LSN       4     FNODE of Dir that contains this one.
    private const int ParentDirectoryLogicalSectorNumberOffset = 0x1C;
    public uint ParentDirectoryLogicalSectorNumber;

    //    20h     (33)    ACL Ext. Run Size       4     Secs in external ACL, if present.
    private const int AccessControlListExternalRunLengthOffset = 0x20;
    public uint AccessControlListExternalRunLength;

    //    24h     (37)    ACL LSN                 4     Location of external ACL run.
    private const int AccessControlListExternalRunLogicalSectorNumberOffset = 0x24;
    public uint AccessControlListExternalRunLogicalSectorNumber;

    //    28h     (41)    ACL Int. Size           2     Bytes in internal (inside FNODE) ACL.
    private const int AccessControlListInternalLengthOffset = 0x28;
    public uint AccessControlListInternalLength;

    //    2Ah     (43)    ACL ALSEC Flag          1     >0 if ACL LSN points to an ALSEC.
    private const int AccessControlListAlsecOffset = 0x2A;
    private byte AccessControlListAlsec;
    public bool IsAccessControlListAlsec => AccessControlListAlsec > 0;

    //    2Bh     (44)    History Bits Count      1     Not implemented.
    private const int HistoryBitsCountOffset = 0x2B;
    private byte HistoryBitsCount;

    //    2Ch     (45)    EA Ext. Run Size        4
    private const int ExtendedAttributeExternalRunLengthOffset = 0x2C;
    private uint ExtendedAttributeExternalRunLength;

    //    30h     (49)    EA LSN                  4
    private const int ExtendedAttributeLogicalSectorNumberOffset = 0x30;
    private uint ExtendedAttributeLogicalSectorNumber;

    //    34h     (53)    EA Int. Size            2
    private const int ExtendedAttributeInternalLengthOffset = 0x34;
    private uint ExtendedAttributeInternalLength;

    //    36h     (55)    EA ALSEC Flag           1     >0 if EA LSN points to an ALSEC.
    private const int ExtendedAttributeAlsecOffset = 0x36;
    private byte ExtendedAttributeAlsec;
    public bool IsExtendedAttributeAlsec => ExtendedAttributeAlsec > 0;

    //    37h     (56)    Dir Flag                1     Bit0 = 1 if dir FNODE, else file FNODE.
    private const int DirectoryFlagOffset = 0x37;
    private DirectoryFlag DirectoryFlag;
    public bool IsDirectory => (DirectoryFlag & DirectoryFlag.IsDirectory) > 0;

    //    38h     (57)    B+Tree Info Flag        1     0x20 (5) Parent is an FNODE, else ALSEC.
    //                                                  0x80 (7) ALNODEs follow, else ALLEAFs.
    private const int BTreeTypeFlagOffset = 0x38;
    private BTreeTypeFlag BTreeTypeFlag;
    public bool IsFNode => (BTreeTypeFlag & BTreeTypeFlag.FNode) > 0;
    public bool IsAlnode => (BTreeTypeFlag & BTreeTypeFlag.Alnode) > 0;

    //    39h     (58)    Padding                 3     Reestablish 32-bit alignment.
    // Not modeled

    //    3Ch     (61)    Free Entries            1     Number of free array entries.
    private const int FreeEntriesOffset = 0x3C;
    public byte FreeEntries;

    //    3Dh     (62)    Used Entries            1     Number of used array entries.
    private const int UsedEntriesOffset = 0x3D;
    public byte UsedEntries;

    //    3Eh     (63)    Free Ent. Offset        2     Offset to next free entry in array.
    private const int FreeEntryOffset = 0x3E;
    public byte FreeEntry;

    public static HpfsFNodeHeader FromBytes(ReadOnlySpan<byte> bytes)
    {
        HpfsFNodeHeader fnode = new HpfsFNodeHeader();

        fnode.Signature = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SignatureOffset));
        fnode.SequentialReadHistory = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(SequentialReadHistoryOffset));
        fnode.FastReadHistory = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(FastReadHistoryOffset));
        fnode.NameLength = bytes[NameLengthOffset];

        var latin1Encoding = EncodingUtilities.GetLatin1Encoding();

        fnode.Name = EncodingExtensions.GetString(latin1Encoding, bytes.Slice(NameOffset, LocalNameLength));
        fnode.ParentDirectoryLogicalSectorNumber = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ParentDirectoryLogicalSectorNumberOffset));
        fnode.AccessControlListExternalRunLength = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(AccessControlListExternalRunLengthOffset));
        fnode.AccessControlListExternalRunLogicalSectorNumber = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(AccessControlListExternalRunLogicalSectorNumberOffset));
        fnode.AccessControlListInternalLength = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(AccessControlListInternalLengthOffset));
        fnode.AccessControlListAlsec = bytes[AccessControlListAlsecOffset];
        fnode.HistoryBitsCount = bytes[HistoryBitsCountOffset];
        fnode.ExtendedAttributeExternalRunLength = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ExtendedAttributeExternalRunLengthOffset));
        fnode.ExtendedAttributeLogicalSectorNumber = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ExtendedAttributeLogicalSectorNumberOffset));
        fnode.ExtendedAttributeInternalLength = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(ExtendedAttributeInternalLengthOffset));
        fnode.ExtendedAttributeAlsec = bytes[ExtendedAttributeAlsecOffset];
        fnode.DirectoryFlag = (DirectoryFlag)bytes[DirectoryFlagOffset];
        fnode.BTreeTypeFlag = (BTreeTypeFlag)bytes[BTreeTypeFlagOffset];

        //    39h     (58)    Padding                 3     Reestablish 32-bit alignment.
        // Not modeled

        fnode.FreeEntries = bytes[FreeEntriesOffset];
        fnode.UsedEntries = bytes[UsedEntriesOffset];
        fnode.FreeEntry = bytes[FreeEntryOffset];
        return fnode;
    }
}