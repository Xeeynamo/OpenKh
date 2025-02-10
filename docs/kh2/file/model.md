# [Kingdom Hearts II](../index.md) - Models

Found inside [MDLX](type/mdlx.md) or MAP files as a [BAR](type/bar.md) type 4, it contains 3d meshes and texture pointers to render a full 3D model.

There are two type of models, one for [objects](#object-model) and another one for [maps](#map-model). They comes with minor differences and they can be identified by their first byte in the header.

Every model data is optimized to use with VPU1 micro programs written inside Kingdom Hearts II executable.
It seems that at least 2 micro programs exist (each for map and objects).

VPU1 acts like vertex shader component equipped in modern PC's GPU.

Input:

- Vertex position
- Vertex color
- Vertex UV component
- Vertex indices
- Matrix (for objects)

Processing:

- Matrix transform (for objects)
- Shape composition: triangle strips

Output directly to GS:

- Vertex position
- Vertex color
- Vertex UV component

## Map model

The simplest of the two models. Its type identifier is `2`. Every file contains a reserved area of `0x90` bytes, used to store pointers at run-time.

### Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint32 | Model identifier. Always `2` for map files.
| 0x04   | uint32 | Unknown. They are often 0.
| 0x08   | uint32 | Unknown. They are often 0.
| 0x0c   | uint32 | Next offset, but it is always 0.
| 0x10   | uint32 | Number of [DMA Chain Maps](#dma-chain-map)
| 0x14   | uint16 | ??? va4
| 0x16   | uint16 | Number of [vifPacketRenderingGroup](#vifPacketRenderingGroup)
| 0x18   | uint32 | Offset of [vifPacketRenderingGroup](#vifPacketRenderingGroup)
| 0x1c   | uint32 | Offset of [dmaChainIndexRemapTable](#dmaChainIndexRemapTable)

### DMA Chain Map

Stored straight after the model [header](#header).

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint32 | First [Source chain DMA tag](#source-chain-dma-tag) offset
| 0x04   | uint32 | Texture Index
| 0x08   | uint16 | Unknown
| 0x0a   | uint16 | Transparent flag. `1`: Enable, `0`: Disable.
| 0x0c   | uint8 | UVSC option
| 0x0d   | uint8 | Unknown
| 0x0e   | uint8 | Unknown
| 0x0f   | uint8 | Unknown

[UVSC option](raw-texture.md#uvsc-uv-scroll):

| Bit | Description |
|-----|-------------|
| 0   | Unknown     |
| 1   | `1`: Enable UVSC |
| 2, 3, 4, 5 | UVSC source index (0 to 15) |
| 6   | Unknown     |
| 7   | Unknown     |

### vifPacketRenderingGroup

vifPacketRenderingGroup is tightly coupled with Table2 of [DOCT](type/doct.md).
It is considered to perform mesh occlusion technique using this group info.

The first 4 bytes stores the offset of `vifPacketRenderingGroup` table. Then the amount of `vifPacketRenderingGroup` specified in the [header](#header) is stored as `uint32`. Each of them represents the absolute offset to a single `vifPacketRenderingGroup` entry.

An `vifPacketRenderingGroup` entry is a list of `uint16`, terminating with a `0xffff`.

A map can be successfully rendered without using this information.

### dmaChainIndexRemapTable

Another structure of unknown purpose. Its content is an array of `uint16` with a `0xffff` indicating the array termination.

Most of the times, each element is stored sequencially from 0 to n-1, and the number never surpasses the [Dma Chain Map}(#dma-chain-map) count.

## Object model

Type identifier `3`.

## Source chain DMA tag

Generic form:

```
Offset(h) 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F

000005800 <------- DMAtag ------> <------ VIFtag ------->
000000010 <--
                   Raw data uploaded to VPU1
0000059A0                                             -->
0000059B0 <------- DMAtag ------> <------ VIFtag ------->
```

The raw data format depends on selection of VPU1 micro program used.
The selection mechanism is unknown for now.

### Map model sample

Sample data:

```
Offset(h) 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F

00005800  1A 00 00 10 00 00 00 00 01 01 00 01 00 80 04 6C  .............€.l
00005810  00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
00005820  0E 00 00 00 04 00 00 00 00 00 00 00 00 00 00 00  ................
00005830  0E 00 00 00 12 00 00 00 00 00 00 00 00 00 00 00  ................
00005840  09 00 00 00 20 00 00 00 00 00 00 00 00 00 00 00  .... ...........
00005850  01 01 00 01 04 80 0E 65 00 0C 44 05 00 0C C9 05  .....€.e..D...É.
00005860  00 04 72 05 00 04 EE 05 00 00 95 06 00 00 93 07  ..r...î...•...“.
00005870  00 00 3F 05 00 04 C5 04 00 04 72 05 00 0C B2 04  ..?...Å...r...².
00005880  00 0C 44 05 00 00 3F 05 00 00 95 06 00 04 72 05  ..D...?...•...r.
00005890  00 00 00 20 CF CF CF CF 01 01 00 01 04 C0 0E 72  ... ÏÏÏÏ.....À.r
000058A0  00 01 02 03 04 05 06 07 02 08 00 06 04 02 00 00  ................
000058B0  00 00 00 20 3F 3F 3F 3F 01 01 00 01 04 C0 0E 72  ... ????.....À.r
000058C0  10 10 30 20 30 20 10 10 30 20 30 10 10 20 00 00  ..0 0 ..0 0.. ..
000058D0  01 01 00 01 12 C0 0E 6E 00 00 00 26 00 00 00 26  .....À.n...&...&
000058E0  00 00 00 26 00 00 00 26 00 00 00 26 00 00 00 26  ...&...&...&...&
000058F0  00 00 00 26 00 00 00 26 00 00 00 26 00 00 00 26  ...&...&...&...&
00005900  00 00 00 26 00 00 00 26 00 00 00 26 00 00 00 26  ...&...&...&...&
00005910  00 00 00 31 00 00 80 3F 00 00 80 3F 00 00 80 3F  ...1..€?..€?..€?
00005920  00 00 80 3F 00 00 00 20 80 80 80 80 01 01 00 01  ..€?... €€€€....
00005930  20 80 09 78 9A 99 75 C2 CD CC 62 42 33 33 EC C3   €.xš™uÂÍÌbB33ìÃ
00005940  00 00 88 C2 9A 99 99 3E 66 26 EC C3 00 80 4B C3  ..ˆÂš™™>f&ìÃ.€KÃ
00005950  9A 99 6B 42 33 33 EC C3 33 B3 52 C3 9A 99 99 3E  š™kB33ìÃ3³RÃš™™>
00005960  66 26 EC C3 CD CC 5A C3 33 33 6B 42 33 33 EC C3  f&ìÃÍÌZÃ33kB33ìÃ
00005970  00 80 61 C3 9A 99 99 3E 66 26 EC C3 CD CC 5A C3  .€aÃš™™>f&ìÃÍÌZÃ
00005980  33 33 6B 42 CD 4C F7 C3 00 80 4B C3 9A 99 6B 42  33kBÍL÷Ã.€KÃš™kB
00005990  CD 4C F7 C3 9A 99 75 C2 CD CC 62 42 00 40 F7 C3  ÍL÷Ãš™uÂÍÌbB.@÷Ã
000059A0  00 00 00 17 00 00 00 00 00 00 00 00 00 00 00 00  ................
000059B0  00 00 00 60 00 00 00 00 00 00 00 00 00 00 00 00  ...`............
```

Sample interpretation:

```
0000   stcycl cl 01 wl 01
0004   unpack V4-32 c 4 a 000 usn 0 flg 1 m 0
    # header
    00000000 00000000 00000000 00000000 
    0000000e 00000004 00000000 00000000 
    0000000e 00000012 00000000 00000000 
    00000009 00000020 00000000 00000000 
    min(0), max(32)
0048   stcycl cl 01 wl 01
004C   unpack V2-16 c 14 a 004 usn 0 flg 1 m 0
    # UV data. 0 to 4096
    0c00 0544 
    0c00 05c9 
    0400 0572 
    0400 05ee 
    0000 0695 
    0000 0793 
    0000 053f 
    0400 04c5 
    0400 0572 
    0c00 04b2 
    0c00 0544 
    0000 053f 
    0000 0695 
    0400 0572 
    min(0), max(3072)
0088   stmask  3 3 0 3  3 3 0 3  3 3 0 3  3 3 0 3 
0090   stcycl cl 01 wl 01
0094   unpack S-8 c 14 a 004 usn 1 flg 1 m 1
    # Vertex position index mapping
    00 
    01 
    02 
    03 
    04 
    05 
    06 
    07 
    02 
    08 
    00 
    06 
    04 
    02 
    min(0), max(8)
00A8   stmask  3 3 3 0  3 3 3 0  3 3 3 0  3 3 3 0 
00B0   stcycl cl 01 wl 01
00B4   unpack S-8 c 14 a 004 usn 1 flg 1 m 1
    # Vertex flag:
    #   10 store
    #   20 store and render triangle
    #   30 store and render flipped triangle
    10 
    10 
    30 
    20 
    30 
    20 
    10 
    10 
    30 
    20 
    30 
    10 
    10 
    20 
    min(16), max(48)
00C8   stcycl cl 01 wl 01
00CC   unpack V4-8 c 14 a 012 usn 1 flg 1 m 0
    # vertex color rgba
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    00 00 00 26 
    min(0), max(38)
0108   stcol 3f800000 3f800000 3f800000 3f800000
011C   stmask  0 0 0 2  0 0 0 2  0 0 0 2  0 0 0 2 
0124   stcycl cl 01 wl 01
0128   unpack V3-32 c 9 a 020 usn 0 flg 1 m 1
    # vertex position xyz in float (IEEE 754)
    c275999a 4262cccd c3ec3333 
    c2880000 3e99999a c3ec2666 
    c34b8000 426b999a c3ec3333 
    c352b333 3e99999a c3ec2666 
    c35acccd 426b3333 c3ec3333 
    c3618000 3e99999a c3ec2666 
    c35acccd 426b3333 c3f74ccd 
    c34b8000 426b999a c3f74ccd 
    c275999a 4262cccd c3f74000 
    min(1050253722), max(3287764173)
0198   mscnt
019C   nop
01A0   nop
01A4   nop
01A8   nop
01AC   nop
```
