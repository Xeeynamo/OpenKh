# [Kingdom Hearts II](../../index.md) - Image

The game engine uses a custom texture format for all the 2D images to render.

## IMGD

Contains a single image. The [file structure](#file-structure) is defined below. Files of this type have `.imd` as extension. They are very similar to [TIM2](../../../common/tm2.md) files as the game collects the information in the header and send them straight to the PlayStation 2's GS exactly as TIM2 does.

## FAC

Contains multiple [IMGDs](#imgd) aligned by `0x800` and appended one after another. Files of this type have `.fac` as extension.

The only known FAC files are used by Jiminy's journal menu.

## IMGZ

Contains multiple [IMGDs](#imgd) prepended by a [specific header](#imz-header). They are appended one after another without any padding. Files of this type have `.imz` as extension.

## File structure

### IMGD Header

| Offset | Type | Description |
|--------|------|-------------|
| 00 | uint32_t | Magic code, always `0x44474D49`
| 04 | uint32_t | Version. Always `0x100`
| 08 | uint32_t | Bitmap offset
| 0c | uint32_t | Bitmap size in bytes
| 10 | uint32_t | Palette offset
| 14 | uint32_t | Palette size in bytes
| 18 | uint32_t | Always `-1`
| 1c | uint16_t | Image width
| 1e | uint16_t | Image height
| 20 | uint16_t | log2 of image width
| 22 | uint16_t | log2 of image height
| 24 | uint16_t | Image width divided by `64`
| 26 | uint16_t | [Pixel storage format](../../../common/tm2.md#psm-register-pixel-storage-mode)
| 28 | uint32_t | Always `-1`
| 2c | uint16_t | Clut width. `8` when format is 4bpp, else `16`
| 2e | uint16_t | Clut height. `2` when format is 4bpp, else `16`
| 30 | uint32_t | Always `1`
| 32 | uint16_t | [Clut storage format](../../../common/tm2.md#cpsm-register-color-look-up-pixel-storage-mode). `19` when format is 32bpp, else `PSMCT32`
| 34 | uint16_t | Image type. `3` if 32bpp, `5` if 8bpp or `4` if 4bpp
| 36 | uint16_t | Clut Type. `0` when format is 32bpp, else `3`
| 38 | uint32_t | Reserved. Always `0`
| 3c | uint32_t | Flags. Usually `3` or `7`, where the mask `4` tells if it's swizzled.

### IMZ header

| Offset | Type | Description |
|--------|------|-------------|
| 00 | uint32_t | Magic code, always `0x5A474D49`
| 04 | uint32_t | Always `256`
| 08 | uint32_t | Header length. Always `16`
| 0c | uint32_t | [IMGD](#imgd) count
