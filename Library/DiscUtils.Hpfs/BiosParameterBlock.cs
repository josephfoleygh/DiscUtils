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
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using DiscUtils.Streams;

namespace DiscUtils.Hpfs;

// Dos 4 bpb - https://en.wikipedia.org/wiki/BIOS_parameter_block
internal class BiosParameterBlock
{
    const string HPFS_OEM_ID = "HPFS    ";
    
    // BR Offset 0x03 - oem id 
    private const int OemIdOffset = 0x03;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 8)]
    public byte[] OemIdAscii;
    public string OemId;

    /*
     BS Offset, BPB Offset, Item Size, Description
    0x00B	0x00	WORD	Bytes per logical sector
    0x00D	0x02	BYTE	Logical sectors per cluster
    0x00E	0x03	WORD	Reserved logical sectors
    0x010	0x05	BYTE	Number of FATs
    0x011	0x06	WORD	Root directory entries
    0x013	0x08	WORD	Total logical sectors
    0x015	0x0A	BYTE	Media descriptor
    0x016	0x0B	WORD	Logical sectors per FAT
    */

    private const int BytesPerSectorOffset = 0x0B;
    public ushort BytesPerSector;
    private const int SectorsPerClusterOffset = 0x0D;
    public int SectorsPerCluster;
    private const int ReservedSectorsOffset = 0x0E;
    private ushort ReservedSectors; // Must be 0
    private const int NumFatsOffset = 0x10;
    public byte NumFats; // Must be 0
    private const int NumRootDirectoryEntriesOffset = 0x11;
    public ushort NumRootDirectoryEntries;
    private const int TotalSectorsOffset = 0x13;
    public ushort TotalSectors16; // Must be 0
    private const int MediaOffset = 0x15;
    public byte Media; // Must be 0xF8
    private const int SectorsPerFatOffset = 0x16;
    public ushort SectorsPerFat;
    
    /*
     * DOS 3.31 BPB
       Main article: DOS 3.31 BPB
       Format of standard DOS 3.31 BPB for FAT12, FAT16 and FAT16B (25 bytes):
       
       Sector offset	BPB offset	Field length	Description
       0x00B	0x00	13 BYTEs	DOS 2.0 BPB
       0x018	0x0D	WORD	Physical sectors per track (identical to DOS 3.0 BPB)
       0x01A	0x0F	WORD	Number of heads (identical to DOS 3.0 BPB)
       0x01C	0x11	DWORD	Hidden sectors (incompatible with DOS 3.0 BPB)
       0x020	0x15	DWORD	Large total logical sectors
     */
    private const int SectorsPerTrackOffset = 0x18;
    public ushort SectorsPerTrack; // Value: 0x3F 0x00
    private const int NumHeadsPerCylinderOffset = 0x1A;
    public ushort NumHeadsPerCylinder; // Value: 0xFF 0x00
    private const int HiddenSectorsOffset = 0x1C;
    public uint HiddenSectors; // Value: 0x3F 0x00 0x00 0x00
    private const int TotalSectors32Offset = 0x20;
    public uint TotalSectors32; // Size of partition
    
    /*
     * BS Offset 0x24
     */
    private const int BiosDriveNumberOffset = 0x24;
    public byte BiosDriveNumber; // Value: 0x80 (first hard disk)
    // BS Offset 0x25
    private const int PaddingByteOffset = 0x25;
    public byte PaddingByte; // Value: 0x00
    // BS Offset 0x26
    private const int SignatureByte28hOffset = 0x26;
    public byte SignatureByte28h; // 0x28h    
    // BS Offset 0x27
    private const int VolumeSerialNumberOffset = 0x27;
    public ulong VolumeSerialNumber;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 11)]
    public byte[] VolumeLabelAscii; // 11 bytes
    public string VolumeLabel;
    
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 8)]
    public byte[] SignatureHpfsAscii; // 8 bytes, /* "HPFS    " */
    public string SignatureHpfs;
    
    // Calculated
    public int BytesPerCluster => BytesPerSector * SectorsPerCluster;
    
    public void Dump(TextWriter writer, string linePrefix)
    {
        writer.WriteLine($"{linePrefix}BIOS PARAMETER BLOCK (BPB)");
        writer.WriteLine($"{linePrefix}                OEM ID: {OemId}");
        writer.WriteLine($"{linePrefix}      Bytes per Sector: {BytesPerSector}");
        writer.WriteLine($"{linePrefix}   Sectors per Cluster: {SectorsPerCluster}");
        writer.WriteLine($"{linePrefix}      Reserved Sectors: {ReservedSectors}");
        writer.WriteLine($"{linePrefix}                # FATs: {NumFats}");
        writer.WriteLine($"{linePrefix}    # FAT Root Entries: {NumRootDirectoryEntriesOffset}");
        writer.WriteLine($"{linePrefix}   Total Sectors (16b): {TotalSectors16}");
        writer.WriteLine($"{linePrefix}                 Media: {Media:X}h");
        writer.WriteLine($"{linePrefix}     Sectors per Track: {SectorsPerTrack}");
        writer.WriteLine($"{linePrefix}               # Heads: {NumHeadsPerCylinder}");
        writer.WriteLine($"{linePrefix}        Hidden Sectors: {HiddenSectors}");
        writer.WriteLine($"{linePrefix}   Total Sectors (32b): {TotalSectors32}");
        writer.WriteLine($"{linePrefix}     BIOS Drive Number: {BiosDriveNumber}");
        writer.WriteLine($"{linePrefix}        Signature Byte: {SignatureByte28h}");
        writer.WriteLine($"{linePrefix}        HPFS Signature: {SignatureHpfs}");
        writer.WriteLine($"{linePrefix}  Volume Serial Number: {VolumeSerialNumber}");
    }

    internal static BiosParameterBlock Initialized(Geometry diskGeometry, int clusterSize, uint partitionStartLba,
                                                   long partitionSizeLba, int mftRecordSize, int indexBufferSize)
    {
        var bpb = new BiosParameterBlock
        {
            OemId = HPFS_OEM_ID,
            BytesPerSector = Sizes.Sector
        };
        bpb.SectorsPerCluster = Convert.ToByte(clusterSize / bpb.BytesPerSector);
        bpb.ReservedSectors = 0;
        bpb.NumFats = 0;
        bpb.NumRootDirectoryEntries = 0;
        bpb.TotalSectors16 = 0;
        bpb.Media = 0xF8;
        bpb.SectorsPerTrack = (ushort)diskGeometry.SectorsPerTrack;
        bpb.NumHeadsPerCylinder = (ushort)diskGeometry.HeadsPerCylinder;
        bpb.HiddenSectors = partitionStartLba;
        bpb.TotalSectors32 = 0;
        bpb.BiosDriveNumber = 0x80;
        bpb.SignatureByte28h = 0x28;
        bpb.PaddingByte = 0;
        //bpb.VolumeSerialNumber;

        return bpb;
    }

    // Takes bootsector as a span
    internal static BiosParameterBlock FromBytes(ReadOnlySpan<byte> bytes)
    {
        var latin1Encoding = EncodingUtilities.GetLatin1Encoding();

        var bpb = new BiosParameterBlock
        {
            OemId = latin1Encoding.GetString(bytes.Slice(OemIdOffset, 8)),
            BytesPerSector = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(BytesPerSectorOffset, 2)),
            TotalSectors16 = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(TotalSectorsOffset)),
            TotalSectors32 = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(TotalSectors32Offset)),
            SignatureByte28h = bytes[SignatureByte28hOffset],
            SectorsPerCluster = DecodeSingleByteSize(bytes[SectorsPerClusterOffset])
        };
        if (!bpb.IsValid(long.MaxValue))
        {
            return bpb;
        }

        bpb.ReservedSectors = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(ReservedSectorsOffset));
        bpb.NumFats = bytes[NumFatsOffset];
        bpb.NumRootDirectoryEntries = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(NumRootDirectoryEntriesOffset));
        bpb.Media = bytes[MediaOffset];
        bpb.SectorsPerTrack = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(SectorsPerTrackOffset));
        bpb.NumHeadsPerCylinder = EndianUtilities.ToUInt16LittleEndian(bytes.Slice(NumHeadsPerCylinderOffset));
        bpb.HiddenSectors = EndianUtilities.ToUInt32LittleEndian(bytes.Slice(HiddenSectorsOffset));
        bpb.BiosDriveNumber = bytes[BiosDriveNumberOffset];
        bpb.PaddingByte = bytes[PaddingByteOffset];
        bpb.VolumeSerialNumber = EndianUtilities.ToUInt64LittleEndian(bytes.Slice(VolumeSerialNumberOffset));

        return bpb;
    }

    internal void ToBytes(Span<byte> buffer)
    {
        var latin1Encoding = EncodingUtilities.GetLatin1Encoding();

        latin1Encoding.GetBytes(OemId, buffer.Slice(OemIdOffset, 8));
        EndianUtilities.WriteBytesLittleEndian(BytesPerSector, buffer.Slice(BytesPerSectorOffset, 8));
        buffer[SectorsPerClusterOffset] = EncodeSingleByteSize(SectorsPerCluster);
        EndianUtilities.WriteBytesLittleEndian(ReservedSectors, buffer.Slice(ReservedSectorsOffset, 8));
        buffer[NumFatsOffset] = NumFats;
        EndianUtilities.WriteBytesLittleEndian(NumRootDirectoryEntries, buffer.Slice(NumRootDirectoryEntriesOffset, 8));
        EndianUtilities.WriteBytesLittleEndian(TotalSectors16, buffer.Slice(TotalSectors16));
        buffer[MediaOffset] = Media;
        EndianUtilities.WriteBytesLittleEndian(SectorsPerTrack, buffer.Slice(SectorsPerTrackOffset, 8));
        EndianUtilities.WriteBytesLittleEndian(NumHeadsPerCylinder, buffer.Slice(NumHeadsPerCylinderOffset, 8));
        EndianUtilities.WriteBytesLittleEndian(HiddenSectors, buffer.Slice(HiddenSectorsOffset));
        EndianUtilities.WriteBytesLittleEndian(TotalSectors32, buffer.Slice(TotalSectors32Offset));
        buffer[BiosDriveNumberOffset] = BiosDriveNumber;
        buffer[SignatureByte28hOffset] = SignatureByte28h;
        buffer[PaddingByteOffset] = PaddingByte;
        EndianUtilities.WriteBytesLittleEndian(VolumeSerialNumber, buffer.Slice(VolumeSerialNumberOffset));
    }

    internal static int DecodeSingleByteSize(byte rawSize)
    {
        if (rawSize > 0x80)
        {
            return 1 << -(sbyte)rawSize;
        }
        
        return rawSize;
    }

    internal static byte EncodeSingleByteSize(int size)
    {
        if (size <= 0x80)
        {
            return (byte)size;
        }

        var count = 1;

        for (; size != 0; size >>= 1)
        {
            count--;
        }

        return (byte)(sbyte)count;
    }

    internal int CalcRecordSize(byte rawSize)
    {
        if (rawSize > 0x80)
        {
            return 1 << -(sbyte)rawSize;
        }
        
        return rawSize * SectorsPerCluster * BytesPerSector;
    }

    internal bool IsValidOemId()
    {
        return (!string.IsNullOrEmpty(OemId) && OemId.Length == HPFS_OEM_ID.Length
                && string.Compare(OemId, 0, HPFS_OEM_ID, 0, HPFS_OEM_ID.Length) == 0);
    }

    internal bool IsValidHpfsSignature()
    {
        return (!string.IsNullOrEmpty(SignatureHpfs) && SignatureHpfs.Length == HPFS_OEM_ID.Length
                                             && string.Compare(SignatureHpfs, 0, HPFS_OEM_ID, 0, HPFS_OEM_ID.Length) == 0);
    }

    internal bool IsValid(long volumeSize)
    {
        /*
         * Some filesystem creation tools are not very strict and DO NOT
         * set the Signature byte to 0x80 (Version "8.0" NTFS BPB).
         *
         * Let's rather check OemId here, so we don't fail hard.
         */
        if (!IsValidOemId() || IsValidHpfsSignature() || TotalSectors16 != 0 || TotalSectors32 != 0)
        {
            return false;
        }

        // TODO: need validation of location of super, spare and rootdir
        return true;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    private static ulong GenSerialNumber()
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(buffer);
        return MemoryMarshal.Read<ulong>(buffer);
    }
#else
    [ThreadStatic]
    private static RandomNumberGenerator rng;

    private static ulong GenSerialNumber()
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));
        try
        {
            rng ??= RandomNumberGenerator.Create();
            rng.GetBytes(buffer, 0, sizeof(ulong));
            return BitConverter.ToUInt64(buffer, 0);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
#endif

    private byte CodeRecordSize(int size)
    {
        if (size >= BytesPerCluster)
        {
            return (byte)(size / BytesPerCluster);
        }

        sbyte val = 0;
        while (size != 1)
        {
            size = (size >> 1) & 0x7FFFFFFF;
            val++;
        }

        return (byte)-val;
    }
}
