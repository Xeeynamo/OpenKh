## NetGameVSMode

### VSMODE_FLAG_DATA

Union

| Length | Type            | Name
|--------|-----------------|-----
| 4      | uint32          | uiFlagInt
| 4      | VSMODE_FLAG_BIT | FlagBit

### VSMODE_FLAG_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0x0       | 1    | UiError         |
| 0x1       | 1    | UiOnlineWait    |
| 0x2       | 1    | UiExterminate   |
| 0x3       | 6    | __dummy__       |
| 0x9       | 1    | uiDataSetupAll  |
| 0xA       | 1    | uiDataSetup     |
| 0xB       | 1    | uiStartCheck    |
| 0xC       | 1    | uiReadyAll      |
| 0xD       | 1    | uiReady         |
| 0xE       | 1    | uiExitStart     |
| 0xF       | 1    | uiPCamAdvertise |
| 0x10      | 1    | uiRoundStart    |
| 0x11      | 1    | uiLeaderLive    |
| 0x12      | 1    | uiTeamError     |
| 0x13      | 1    | uiResultWindow  |
| 0x14      | 1    | uiMVP           |
| 0x15      | 1    | uiLose          |
| 0x16      | 1    | uiWin           |
| 0x17      | 1    | uiDecideAll     |
| 0x18      | 1    | uiDecide        |
| 0x19      | 1    | uiExit          |
| 0x1A      | 1    | uiSelectWindow  |
| 0x1B      | 1    | uiTotalScore    |
| 0x1C      | 1    | uiDataFree      |
| 0x1D      | 1    | uiTeamBattle    |
| 0x1E      | 1    | uiLeader        |
| 0x1F      | 1    | uiOnline        |

### PLAYER_INFO

| Offset | Length | Type     | Name
|--------|--------|----------|-----
| 0x0    | 38     | char[38] | m_szName
| 0x26   | 1      | byte     | m_nDeck
| 0x27   | 1      | byte     | m_nColor
| 0x28   | 1      | byte     | m_nTeam
| 0x29   | 1      | byte     | m_nDecide
| 0x2A   | 1      | byte     | m_nRank
| 0x2B   | 1      | byte     | m_nWin
| 0x2C   | 1      | byte     | m_nLose
| 0x2D   | 1      | byte     | m_nMVP
| 0x2E   | 1      | byte     | m_nLiveInfo
| 0x2F   | 1      | byte     | m_dum0
| 0x30   | 4      | uint32   | m_nScore
| 0x34   | 4      | uint32   | m_nScoreR
| 0x38   | 4      | uint32   | m_nTotal
| 0x3C   | 2      | uint16   | m_nMedal
| 0x3E   | 2      | uint16   | m_nBonusP
| 0x40   | 2      | uint16   | m_nBonusM
| 0x42   | 1      | byte     | m_dum1
| 0x43   | 1      | byte     | m_dum2


### TEAM_INFO

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 4      | uint32    | m_nScoreR
| 0x8    | 2      | uint16    | m_nBonusP
| 0xA    | 2      | uint16    | m_nBonusM
| 0xC    | 1      | byte      | m_nWin
| 0xD    | 1      | byte      | m_nLose
| 0xE    | 1      | byte      | m_nRank
| 0xF    | 1      | byte      | m_nMemberCount
| 0x10   | 6      | byte[6]   | m_nMember
| 0x16   | 1      | undefined |
| 0x17   | 1      | undefined |

### CNetGameVSMode

| Offset | Length | Type             | Name
|--------|--------|------------------|-----
| 0x0    | 52     | CTreeTask        | super_CTreeTask
| 0x34   | 4      | IAllocator*      | m_pAllocator
| 0x38   | 4      | VSMODE_FLAG_DATA | m_FlagData
| 0x3C   | 4      | float            | m_fTimer
| 0x40   | 4      | float            | m_fLocalTimer
| 0x44   | 4      | float            | m_fGamerTimer
| 0x48   | 1      | byte             | m_nState
| 0x49   | 1      | byte             | m_nPreState
| 0x4A   | 1      | byte             | m_nStateEntry
| 0x4B   | 1      | byte             | m_nStateSelect
| 0x4C   | 1      | byte             | m_nStateLoad
| 0x4D   | 1      | byte             | m_nStateGame
| 0x4E   | 1      | byte             | m_nStateResult
| 0x4F   | 1      | byte             | m_nStateAlert
| 0x50   | 10     | byte[10]         | m_nCheck
| 0x5A   | 2      | uint16           | m_nCheckFlag
| 0x5C   | 1      | byte             | m_nCheckCurrent
| 0x5D   | 1      | byte             | m_dum0
| 0x5E   | 1      | byte             | m_dum1
| 0x5F   | 1      | byte             | m_dum2
| 0x60   | 4      | CGameCockpit*    | m_pGameCockpit
| 0x64   | 4      | CGOnlineWait*    | m_pOnlineWait
| 0x68   | 6      | byte[6]          | m_nLiveInfo
| 0x6E   | 1      | byte             | m_nRecvMax
| 0x6F   | 1      | byte             | m_nRecvOnline
| 0x70   | 1      | byte             | m_nRecvLost
| 0x71   | 1      | byte             | m_nLeaderId
| 0x72   | 1      | byte             | m_nDataSetupOK
| 0x73   | 1      | byte             | m_nStartMember
| 0x74   | 1      | byte             | m_nPacketCode
| 0x75   | 1      | byte             | m_nDropCount
| 0x76   | 1      | byte             | m_nMemberReady
| 0x77   | 1      | byte             | m_inGameMode
| 0x78   | 1      | byte             | m_inEventRule
| 0x79   | 1      | byte             | m_inDataMode
| 0x7A   | 1      | byte             | m_inTimeLimit
| 0x7B   | 1      | byte             | m_inMemberMax
| 0x7C   | 1      | byte             | m_inField
| 0x7D   | 1      | byte             | m_outGameMode
| 0x7E   | 1      | byte             | m_outEventRule
| 0x7F   | 1      | byte             | m_outDataMode
| 0x80   | 1      | byte             | m_outTimeLimit
| 0x81   | 1      | byte             | m_outMemberMax
| 0x82   | 1      | byte             | m_outField
| 0x83   | 1      | byte             | m_nMVP
| 0x84   | 1      | byte             | m_nTeam
| 0x85   | 1      | byte             | m_nDeckType
| 0x86   | 1      | byte             | m_nPlayerNum
| 0x87   | 1      | byte             | m_nMember
| 0x88   | 1      | byte             | m_nMemberLost
| 0x89   | 1      | byte             | m_nMemberOK
| 0x8A   | 1      | byte             | m_nResult
| 0x8B   | 1      | undefined        | 
| 0x8C   | 72     | PLAYER_SCORE[6]  | m_PlayerScore
| 0xD4   | 16     | TEAM_SCORE[2]    | m_TeamScore
| 0xE4   | 408    | PLAYER_INFO[6]   | m_PlayerInfo
| 0x27C  | 48     | TEAM_INFO[6]     | m_TeamInfo

### Split up next enums in there own categorie

### Check

| Name         | Value
|--------------|------
| CHECK_ENTRY  | 0x0
| CHECK_READY  | 0x1
| CHECK_SELECT | 0x2
| CHECK_SETUP  | 0x3
| CHECK_LOAD   | 0x4
| CHECK_FIELD  | 0x5
| CHECK_START  | 0x6
| CHECK_PLAY   | 0x7
| CHECK_RESULT | 0x8
| CHECK_NEXT   | 0x9
| CHECK_MAX    | 0xA

### State Entry

| Name                   | Value
|------------------------|------
| STATE_ENTRY_NONE       | 0x0
| STATE_ENTRY_START      | 0x1
| STATE_ENTRY_LEADERWAIT | 0x2
| STATE_ENTRY_MEMBERWAIT | 0x3
| STATE_ENTRY_OK         | 0x4
| STATE_ENTRY_ERROR      | 0x5

### State Error

| Name        | Value
|-------------|------
| STATE_ERROR | 0x63

### State Game

| Name             | Value
|------------------|------
| STATE_GAME_NONE  | 0x0
| STATE_GAME_CHECK | 0x1
| STATE_GAME_INIT  | 0x2
| STATE_GAME_DEMO  | 0x3
| STATE_GAME_READY | 0x4
| STATE_GAME_START | 0x5
| STATE_GAME_PLAY  | 0x6
| STATE_GAME_WAIT  | 0x7
| STATE_GAME_END   | 0x8

### State Load

| Name             | Value
|------------------|------
| STATE_LOAD_NONE  | 0x0
| STATE_LOAD_START | 0x1
| STATE_LOAD_EXEC  | 0x2
| STATE_LOAD_WAIT  | 0x3
| STATE_LOAD_END   | 0x4

### State Result

| Name               | Value
|--------------------|------
| STATE_RESULT_NONE  | 0x0
| STATE_RESULT_CHECK | 0x1
| STATE_RESULT_DEMO  | 0x2
| STATE_RESULT_DISP  | 0x3
| STATE_RESULT_WAIT  | 0x4
| STATE_RESULT_END   | 0x5

### State Select

| Name                | Value
|---------------------|------
| STATE_SELECT_NONE   | 0x0
| STATE_SELECT_FIRST  | 0x1
| STATE_SELECT_START  | 0x2
| STATE_SELECT_SETUP  | 0x3
| STATE_SELECT_READY  | 0x4
| STATE_SELECT_EXEC   | 0x5
| STATE_SELECT_DECIDE | 0x6
| STATE_SELECT_WAIT   | 0x7
| STATE_SELECT_END    | 0x8

### VSMode

| Name                | Value
|---------------------|------
| VSMODE_STATE_NONE   | 0x0
| VSMODE_STATE_ENTRY  | 0x1
| VSMODE_STATE_SELECT | 0x2
| VSMODE_STATE_LOAD   | 0x3
| VSMODE_STATE_GAME   | 0x4
| VSMODE_STATE_RESULT | 0x5
| VSMODE_STATE_EXIT   | 0x6
| VSMODE_STATE_ERROR  | 0x7

___