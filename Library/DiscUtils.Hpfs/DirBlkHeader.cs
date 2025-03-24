namespace DiscUtils.Hpfs;

public class DirBlkHeader
{
    /*
    Offset          Data                 Size               Comment
hex    (dec)                         bytes

Header
00h     (1)     Signature         4       0x77E40AAE
*/
    private const uint DIRBLK_SIGNATURE = 0x77E40AAE;
    private const uint SignatureOffset = 0;
    public uint Signature;

    // 04h     (5)     Off. 1st Free     4       Offset within DIRBLK to 1st free entry.
    private const uint OffsetOfFirstFreeEntryOffset = 4;
    public uint OffsetOfFirstFreeEntry;

    // 08h     (9)     Change            4       Low bit indicates whether this is
    //                                            topmost DIRBLK in B-tree.
    private const uint TopmostDirblkOffset = 8;
    public uint TopmostDirblk;

    // 0Ch     (13)    Parent LSN        4       If topmost, LSN of this dir's FNODE.
    //                                            Otherwise, LSN of parent DIRBLK.
    private const uint LogicalSectorNumberChainOffset = 0x0C;
    public uint LogicalSectorNumberChain;


    // 10h     (17)    DIRBLK's LSN      4       Self-pointer.
    private const uint LogicalSectorNumberSelfOffset = 0x10;
    public uint LogicalSectorNumberSelf;
}