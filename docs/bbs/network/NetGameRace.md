## NetGameRace

### Event

| Name                    | Value
|-------------------------|------
| EVENT_LEADER_PLAYER_NUM | 0x0

### Packet

| Name             | Value
|------------------|------
| PACKET_INDEX_MAX | 0x6

### NPC Player

| Name                 | Value
|----------------------|------
| NPC_PLAYER_NUM_START | 0xA

### Phase

| Name                    | Value
|-------------------------|------
| PHASE_CHECK_MEMBER      | 0x1
| PHASE_CHECK_PLAYER_NUM  | 0x2
| PHASE_CHECK_MEMBER_INFO | 0x3
| PHASE_CHECK_NPC_INFO    | 0x4
| PHASE_CHECK_COUNT_DOWN  | 0x5
| PHASE_RACE              | 0x6
| PHASE_RESULT            | 0x7
| PHASE_MAX               | 0x8

### PktObject Race

| Offset | Length | Type               | Name
|--------|--------|--------------------|-----
| 0x0    | 1      | sbyte              | m_nPlayerNum
| 0x1    | 1      | byte               | m_CurrentBestPlayerCarLap
| 0x2    | 1      | byte               | fillerb0
| 0x3    | 1      | byte               | fillerb1
| 0x4    | 4      | float              | m_GameTimer
| 0x8    | 104    | CAR                | m_Player
| 0x70   | 580    | NPC[5]             | m_Npc
| 0x2B4  | 64     | GIMMICK_TORNADO[4] | m_GimmickTornado
| 0x2F4  | 32     | GIMMICK_SHIELD[4]  | m_GimmickShield
| 0x314  | 6      | byte[6]            | m_isRequestCutNetwork
| 0x31A  | 1      | byte               | filler0
| 0x31B  | 1      | byte               | filler1
| 0x31C  | 8      | GIMMICK_PISTON     | m_GimmickPiston

### PktObject Object

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 8      | HEADER | m_Header
| 0x8    | 4      | GAME   | m_Game
| 0xC    | 804    | RACE   | m_RaceData

### PktObject Header

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | m_Phase
| 0x2    | 1      | byte   | m_isEnable
| 0x3    | 1      | byte   | m_CharaID
| 0x4    | 2      | uint16 | m_AllMemberPhaseRecive
| 0x6    | 1      | byte   | filler0
| 0x7    | 1      | byte   | filler1

### PktObject Game

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 1      | byte | fillerb0
| 0x1    | 1      | byte | fillerb1
| 0x2    | 1      | byte | fillerb2
| 0x3    | 1      | byte | fillerb3

### PktObject Rider

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 2      | int16 | m_nPosX
| 0x2    | 2      | int16 | m_nPosZ
| 0x4    | 2      | int16 | m_nPosY
| 0x6    | 2      | int16 | m_nRotX
| 0x8    | 2      | int16 | m_nRotY
| 0xA    | 2      | int16 | m_nRotZ

### PktObject GIMMICK

| Name                | Value
|---------------------|------
| GIMMICK_TORNADO_MAX | 0x4
| GIMMICK_SHIELD_MAX  | 0x4

### PktObject NPC

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 104    | CAR   | m_Car
| 0x68   | 12     | RIDER | m_Rider

### PktObject GIMMICK_TORNADO

| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 1      | byte  | m_UniqueID
| 0x1    | 1      | byte  | m_MoveDirection
| 0x2    | 2      | int16 | m_nPosY
| 0x4    | 4      | float | m_nPosX
| 0x8    | 4      | float | m_nPosZ
| 0xC    | 2      | int16 | m_nVelX
| 0xE    | 2      | int16 | m_nVelZ

### PktObject GIMMICK_SHIELD

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 1      | byte   | m_UniqueID
| 0x1    | 1      | byte   | filler0
| 0x2    | 1      | byte   | filler1
| 0x3    | 1      | byte   | filler2
| 0x4    | 4      | uint32 | m_DisappearCounter

### PktObject GIMMICK_PISTON

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | m_nAnimTime
| 0x2    | 1      | byte   | filler00
| 0x3    | 1      | byte   | filler01
| 0x4    | 4      | float  | m_MotSpeed

### PktObject CAR_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0         | 21   | __dummy__
| 21        | 1    | m_isFixedBranchCourseSelect
| 22        | 1    | m_isBranchChecking
| 23        | 1    | m_isUseBranchPath
| 24        | 1    | m_isWrongWay
| 25        | 1    | m_isCheckDangerAfterPlayer
| 26        | 1    | m_isLockOnEnable
| 27        | 2    | m_ShieldState (int)
| 29        | 2    | m_GuardState (int)
| 31        | 1    | m_isRiderAction

### PktObject CAR

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 48     | BULLET  | m_Bullet
| 0x30   | 1      | byte    | m_nAnimNo
| 0x31   | 1      | byte    | m_LockOnPlayerNum
| 0x32   | 1      | byte    | m_DangerPlayerNum
| 0x33   | 1      | byte    | m_State
| 0x34   | 4      | CAR_BIT | m_CarBit
| 0x38   | 1      | byte    | m_DarkStateCounter
| 0x39   | 1      | sbyte   | m_CurrentLap
| 0x3A   | 1      | byte    | filler1
| 0x3B   | 1      | byte    | filler2
| 0x3C   | 4      | float   | m_nPosX
| 0x40   | 4      | float   | m_nPosZ
| 0x44   | 2      | int16   | m_nPosY
| 0x46   | 2      | int16   | m_nRotX
| 0x48   | 2      | int16   | m_nRotY
| 0x4A   | 2      | int16   | m_nRotZ
| 0x4C   | 4      | float   | m_nOldX
| 0x50   | 4      | float   | m_nOldZ
| 0x54   | 2      | int16   | m_nOldY
| 0x56   | 2      | int16   | m_nVelX
| 0x58   | 2      | int16   | m_nVelY
| 0x5A   | 2      | int16   | m_nVelZ
| 0x5C   | 2      | uint16  | m_nAnimTime
| 0x5E   | 1      | ubyte   | m_CurrentPathIndex
| 0x5F   | 1      | ubyte   | m_CurrentPathCount
| 0x60   | 1      | ubyte   | m_CurrentPathDataNo
| 0x61   | 1      | ubyte   | m_CurrentBranchTargetPathIndex
| 0x62   | 1      | byte    | filler00
| 0x63   | 1      | byte    | filler01
| 0x64   | 4      | float   | m_CheckDangerAfterPlayerLength

### PktObject BULLET

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 1      | byte   | m_isExist
| 0x1    | 1      | byte   | m_DangerPlayerNum
| 0x2    | 2      | uint16 | m_BulletID
| 0x4    | 1      | byte   | m_State
| 0x5    | 1      | byte   | m_Mode
| 0x6    | 1      | byte   | m_fillerb0
| 0x7    | 1      | byte   | m_fillerb1
| 0x8    | 4      | float  | m_TimbeID
| 0xC    | 4      | float  | m_nPosX
| 0x10   | 4      | float  | m_nPosZ
| 0x14   | 2      | int16  | m_nPosY
| 0x16   | 2      | int16  | m_nRotX
| 0x18   | 2      | int16  | m_nRotY
| 0x1A   | 2      | int16  | m_nRotZ
| 0x1C   | 4      | float  | m_nOldX
| 0x20   | 4      | float  | m_nOldZ
| 0x24   | 2      | int16  | m_nOldY
| 0x26   | 2      | int16  | m_nVelX
| 0x28   | 2      | int16  | m_nVelY
| 0x2A   | 2      | int16  | m_nVelZ
| 0x2C   | 2      | int16  | m_Param0
| 0x2E   | 2      | int16  | m_Param1

### PktObject ShieldState

| Name                 | Value
|----------------------|------
| SHIELD_STATE_NONE    | 0x0
| SHIELD_STATE_MOVING  | 0x1
| SHIELD_STATE_SUCCESS | 0x2

### PktObject GuardState

| Name                | Value
|---------------------|------
| GUARD_STATE_NONE    | 0x0
| GUARD_STATE_MOVING  | 0x1
| GUARD_STATE_SUCCESS | 0x2

### PktObject BULLET

| Name         | Value
|--------------|------
| MODE_NETWORK | 0x0
| MODE_HOMING  | 0x1

### PktGame HEADER

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | m_Phase
| 0x2    | 2      | uint16 | m_phaseAkn

### PktGame COMMON_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | HEADER       | m_Header
| 0x4    | 1      | byte         | m_NpcNum
| 0x5    | 1      | byte         | m_MemberNum
| 0x6    | 1      | byte         | fillerb0
| 0x7    | 1      | byte         | fillerb1
| 0x8    | 10     | NPC_INFO[5]  | m_NpcInfo
| 0x12   | 1      | byte         | fillerb2
| 0x13   | 1      | byte         | fillerb3
| 0x14   | 48     | RACE_INFO[6] | m_RaceInfo

### PktGame RACE_INFO_BIT

| Postition | Size | Name            |
|-----------|------|-----------------|
| 0         | 17   | __dummy__
| 17        | 1    | m_isRequestCutNetwork
| 18        | 1    | m_isGoalFinished
| 19        | 4    | m_GoalRank
| 23        | 4    | m_Rank
| 27        | 5    | m_nPlayerNum

### PktGame RACE_INFO

| Offset | Length | Type          | Name
|--------|--------|---------------|-----
| 0x0    | 4      | RACE_INFO_BIT | m_InfoBit
| 0x4    | 4      | float         | m_GoalTime

### PktGame NPC_INFO

| Offset | Length | Type | Name
|--------|--------|------|-----
| 0x0    | 1      | byte | m_CarID
| 0x1    | 1      | byte | m_PlayerNum

### PktGame CNetGameRace

| Offset | Length | Type           | Name
|--------|--------|----------------|-----
| 0x0    | 52     | CTreeTask      | super_CTreeTask
| 0x34   | 4      | uint32         | m_Step
| 0x38   | 520    | NETGAME_STATUS | m_NetGameStatus
| 0x240  | 4      | float          | m_Timer
| 0x244  | 4      | int32          | m_CurrentCarNum
| 0x248  | 4      | int32          | m_CurrentMemberNum
| 0x24C  | 4      | int32          | m_TimeOutStep
| 0x250  | 4      | float          | m_StartUpOverTimer
| 0x254  | 4      | cGOnlineWait*  | m_pGOnlineWait
| 0x258  | 4      | bool           | m_isDispOnlineWait
| 0x25C  | 4      | int32          | m_EventReadyStep
| 0x260  | 4      | float          | m_EventReadyTimer

### PktGame SyncPhaseState

| Name                      | Value
|---------------------------|------
| SYNC_PHASE_STATE_WAITING  | 0x0
| SYNC_PHASE_STATE_DONE     | 0x1
| SYNC_PHASE_STATE_TIME_OUT | 0x1

___