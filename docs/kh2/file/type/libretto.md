# [Kingdom Hearts II](../../index.md) - Libretto

This is the file that connects the Text ID in MSG files with Talk Message in ARD files. The world-specific libretto files is a BAR file with only 1 subfile while 13libretto.bin is the subfile directly. The structure below assumes that you have extracted said subfile.

## Header

| Offset | Type   | Description
|--------|--------|------------
| 0 	 | uint32 | File type (3)
| 4 	 | uint32 | Entry Count

## Talk Message Definition

| Offset | Type   | Description
|--------|--------|------------
| 0 	 | uint16 | Talk Message ID
| 2 	 | uint16 | Unknown
| 4 	 | uint32 | Content pointer within subfile

## Talk Message Content

The content starts from the address pointed in the definition until the bytes 00 00 00 00 are found. The structure of each textbox is as follows:

| Offset | Type   | Description
|--------|--------|------------
| 0 	 | uint32 | Unknown (usually 01 00 01 00)
| 4 	 | uint32 | Text ID
