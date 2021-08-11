## NetGameHistory

### HISTORY_CONTENT

| Name                   | Value
|------------------------|------
| HISTORY_CONTENT_VS     | 0x0
| HISTORY_CONTENT_ARENA  | 0x1
| HISTORY_CONTENT_DICE   | 0x2
| HISTORY_CONTENT_RACE   | 0x3
| HISTORY_CONTENT_D_LINK | 0x4

### HISTORY_CONTENT_INFO

Union

| Length | Type            | Name
|--------|-----------------|-----
| 4      | conflict 1      | ??
| 4      | conflict 2      | ??
| 4      | conflict 3      | ??

conflict 1

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | detailsMettsAgainNum
| 0x2    | 2      | uint16 | detailsCmdKind

conflict 2

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 1      | byte  | fillerb
| 0x1    | 1      | byte  | m_Lap
| 0x2    | 2      | int16 | fillers

conflict 3

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 2      | int16 | detailsParam0
| 0x2    | 2      | int16 | detailsParam1


| Offset | Length | Type            | Name
|--------|--------|-----------------|-----
| 0x0    | 16     | SPspDataTime    | AddHistoryTime
| 0x10   | 4      | uint32          | resultTime
| 0x14   | 4      | HISTORY_CONTENT | content
| 0x18   | 4      | HISTORY_MODE    | mode
| 0x1C   | 4      | HISTORY_RESULT  | result
| 0x20   | 4      | HISTORY_DETAILS | details
| 0x24   | 4      | union           | HISTORY_CONTENT_INFO union (the needed struct probably depends on the mode)
| 0x28   | 2      | uint16          | medal
| 0x2A   | 1      | ??              | undefined
| 0x2B   | 1      | ??              | undefined

### HISTORY_DETAILS

| Name                   | Value
|------------------------|------
| HISTORY_DETAILS_NONE   | 0x0
| HISTORY_DETAILS_D_LINK | 0x1
| HISTORY_RIDE_RACE      | 0x2

### HISTORY_MODE

| Name                  | Value
|-----------------------|------
| HISTORY_MODE_VS_BR    | 0x0
| HISTORY_MODE_VS_TEAM  | 0x1
| HISTORY_MODE_ARENA_0  | 0x2
| HISTORY_MODE_ARENA_1  | 0x3
| HISTORY_MODE_ARENA_2  | 0x4
| HISTORY_MODE_ARENA_3  | 0x5
| HISTORY_MODE_ARENA_4  | 0x6
| HISTORY_MODE_ARENA_5  | 0x7
| HISTORY_MODE_ARENA_6  | 0x8
| HISTORY_MODE_ARENA_7  | 0x9
| HISTORY_MODE_ARENA_8  | 0xA
| HISTORY_MODE_ARENA_9  | 0xB
| HISTORY_MODE_ARENA_10 | 0xC
| HISTORY_MODE_ARENA_11 | 0xD
| HISTORY_MODE_ARENA_12 | 0xE
| HISTORY_MODE_ARENA_13 | 0xF
| HISTORY_MODE_ARENA_14 | 0x10
| HISTORY_MODE_ARENA_15 | 0x11
| HISTORY_MODE_DICE_0   | 0x12
| HISTORY_MODE_DICE_1   | 0x13
| HISTORY_MODE_DICE_2   | 0x14
| HISTORY_MODE_DICE_3   | 0x15
| HISTORY_MODE_DICE_4   | 0x16
| HISTORY_MODE_DICE_5   | 0x17
| HISTORY_MODE_DICE_6   | 0x18
| HISTORY_MODE_RACE_0   | 0x19
| HISTORY_MODE_RACE_1   | 0x1A
| HISTORY_MODE_RACE_2   | 0x1B
| HISTORY_MODE_RACE_3   | 0x1C

### HISTORY_NICKNAME

Union conflict 1

| Length | Type       | Name
|--------|------------|-----
| 20     | conflict 1 | ??
| 20     | conflict 2 | ??

Union conflict 2

| Length | Type       | Name
|--------|------------|-----
| 20     | conflict 3 | ??
| 20     | conflict 4 | ??

conflict 1

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 20     | char *[5] | pNickName

conflict 2

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 4      | char * | pNickName0
| 0x4    | 4      | char * | pNickName1
| 0x8    | 4      | char * | pNickName2
| 0xC    | 4      | char * | pNickName3
| 0x10   | 4      | char * | pNickName4

conflict 3

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 20     | bool [5]  | isEnemyNickName

conflict 4

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 4      | bool | isEnemyNickName0
| 0x4    | 4      | bool | isEnemyNickName1
| 0x8    | 4      | bool | isEnemyNickName2
| 0xC    | 4      | bool | isEnemyNickName3
| 0x10   | 4      | bool | isEnemyNickName4


Names are guess from types.

| Offset | Length | Type             | Name
|--------|--------|------------------|-----
| 0x0    | 20     | union conflict 1 | pNickName
| 0x14   | 20     | union conflict 2 | isEnemyNickName

### HISTORY_RESULT

| Name                  | Value
|-----------------------|------
| HISTORY_RESULT_WIN    | 0x0
| HISTORY_RESULT_LOSE   | 0x1
| HISTORY_RESULT_RANK_1 | 0x2
| HISTORY_RESULT_RANK_2 | 0x3
| HISTORY_RESULT_RANK_3 | 0x4
| HISTORY_RESULT_RANK_4 | 0x5
| HISTORY_RESULT_RANK_5 | 0x6
| HISTORY_RESULT_RANK_6 | 0x7
| HISTORY_RESULT_TIE    | 0x8
