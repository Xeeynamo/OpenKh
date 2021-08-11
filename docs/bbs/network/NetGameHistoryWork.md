## NetGameHistoryWork

### NetGameHistoryData

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 16     | SPspDataTime | AddHistoryTime
| 0x10   | 4      | uint32       | resultTime
| 0x14   | 1      | byte         | content
| 0x15   | 1      | byte         | result
| 0x16   | 1      | byte         | mode
| 0x17   | 1      | byte         | details
| 0x18   | 2      | int16        | detailsParam0
| 0x1A   | 2      | int16        | detailsParam1
| 0x1C   | 2      | uint16       | medal
| 0x1E   | 1      | byte         | neckNameID0
| 0x1F   | 1      | byte         | neckNameID1
| 0x20   | 1      | byte         | neckNameID2
| 0x21   | 1      | byte         | neckNameID3
| 0x22   | 1      | byte         | neckNameID4
| 0x23   | 1      | char         | dummy

### NetGameHistoryWork

| Offset | Length | Type                   | Name
|--------|--------|------------------------|-----
| 0x0    | 1900   | char[50][38]           | nickName
| 0x76C  | 50     | char[50]               | nickNameRef
| 0x79E  | 1      | char                   | historyNum
| 0x79F  | 1      | char                   | historyNowIndex
| 0x7A0  | 360    | NetGamehistoryData[10] | historyData

### SPspDataTime
| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | year
| 0x2    | 2      | uint16 | month
| 0x4    | 2      | uint16 | day
| 0x6    | 2      | uint16 | hour
| 0x8    | 2      | uint16 | minute
| 0xA    | 2      | uint16 | second
| 0xC    | 4      | uint32 | microsecond

___