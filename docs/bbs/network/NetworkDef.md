# [Kingdom Hearts Birth By Sleep](index.md) - Network Packages

## Network Def

### PACKET_DATA

| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0x0    | 68     | PKT_HEADER | m_pktHeader
| 0x44   | 80     | PKT_GAME   | m_pktGame
| 0x94   | 880    | PKT_OBJECT | m_pktObject

### PKT_HEADER

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 1      | byte      | m_nUser
| 0x1    | 1      | byte      | m_nCode
| 0x2    | 1      | byte      | [m_nPhase](#PKT_HEADER-Phase) Needs testing
| 0x3    | 1      | byte      | m_nReady
| 0x4    | 1      | byte      | pad0
| 0x5    | 1      | byte      | m_nOpenLv
| 0x6    | 1      | undefined |
| 0x7    | 1      | undefined |
| 0x8    | 4      | uint32    | m_nSendCount
| 0xC    | 4      | uint32    | m_uiUserColor
| 0x10   | 38     | char[38]  | m_szNickName
| 0x36   | 8      | char[8]   | m_szRoomName
| 0x3E   | 1      | undefined |
| 0x3F   | 1      | undefined |
| 0x40   | 4      | float     | m_fUpdate

### PKT_GAME

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 80     | char[8] | m_nData

### PKT_OBJECT

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 880    | char[8] | m_nData

### Network State

| Name                       | Value
|----------------------------|------
| NETWORK_NONE               | 0x0
| NETWORK_INIT               | 0x1
| NETWORK_TOP                | 0x2
| NETWORK_CONNECT_IBSS       | 0x3
| NETWORK_CONNECTING_IBSS    | 0x4
| NETWORK_IBSS               | 0x5
| NETWORK_CONNECT_ROOM       | 0x6
| NETWORK_CONNECTING_ROOM    | 0x7
| NETWORK_DISCONNECTING_ROOM | 0x8
| NETWORK_CONNECTING_GAME    | 0x9
| NETWORK_DISCONNECTING      | 0xA
| NETWORK_ERROR_DIALOG       | 0xB
| NETWORK_LOBBY              | 0xC
| NETWORK_NEIGHBORS          | 0xD
| NETWORK_REQUESTING         | 0xE
| NETWORK_CANCELED           | 0xF
| NETWORK_DENIED             | 0x10
| NETWORK_PEER_BUSY          | 0x11
| NETWORK_PEER_DISAPPEARED   | 0x12
| NETWORK_START_GAME         | 0x13
| NETWORK_DISCONNECT         | 0x14
| NETWORK_CHILD_WAIT         | 0x15
| NETWORK_PARENT_START       | 0x16
| NETWORK_PARENT_SYNC        | 0x17
| NETWORK_START_READY        | 0x18
| NETWORK_CREATE_ROOM        | 0x19
| NETWORK_PARENT_WAIT        | 0x1A
| NETWORK_GAME               | 0x1B
| NETWORK_GAME_DISCONNECT    | 0x1C
| NETWORK_OUT                | 0x1D
| NETWORK_MAX                | 0x1E

### Matching Mode

| Name                 | Value
|----------------------|------
| MATCHING_MODE_CHILD  | 0x0
| MATCHING_MODE_PARENT | 0x1

### HELLO_OPT_GAME_PARAM

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 1      | byte | nGameMode
| 0x1    | 1      | byte | nEventRule
| 0x2    | 1      | byte | nDataMode
| 0x3    | 1      | byte | nTimeLimit
| 0x4    | 1      | byte | nMemberMax
| 0x5    | 1      | byte | nField
| 0x6    | 1      | byte | nTarget
| 0x7    | 1      | byte | nRank

### GAME_INFO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 4      | int32 | m_iServerFlag
| 0x4    | 4      | int32 | m_iClientReady
| 0x8    | 4      | int32 | m_iServerIndex

### PKT_HEADER Phase

| Name       | Value
|------------|------
| PHASE_OK   | 0x0
| PHASE_NG   | 0x1
| PHASE_LATE | 0x2
| PHASE_LOST | 0x3

### PACKET_DATA Code

| Name               | Value
|--------------------|------
| CODE_NONE          | 0x0
| CODE_LOBBY_PLAY    | 0x0
| CODE_VSMODE_SELECT | 0x1
| CODE_VSMODE_PLAY   | 0x2
| CODE_VSMODE_RESULT | 0x3
| CODE_ARENA_READY   | 0x4
| CODE_ARENA_PLAY    | 0x5
| CODE_DICE_         | 0x6
| CODE_RACE_         | 0x7

___