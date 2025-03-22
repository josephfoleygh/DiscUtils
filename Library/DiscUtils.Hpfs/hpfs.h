struct hpfs_boot_block
{
    u8 jmp[3];
    u8 oem_id[8];
    u8 bytes_per_sector[2];	/* 512 */
    u8 sectors_per_cluster;
    u8 n_reserved_sectors[2];
    u8 n_fats;
    u8 n_rootdir_entries[2];
    u8 n_sectors_s[2];
    u8 media_byte;
    __le16 sectors_per_fat;
    __le16 sectors_per_track;
    __le16 heads_per_cyl;
    __le32 n_hidden_sectors;
    __le32 n_sectors_l;		/* size of partition */
    u8 drive_number;
    u8 mbz;
    u8 sig_28h;			/* 28h */
    u8 vol_serno[4];
    u8 vol_label[11];
    u8 sig_hpfs[8];		/* "HPFS    " */
    u8 pad[448];
    __le16 magic;			/* aa55 */
};