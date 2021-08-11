# [Kingdom Hearts Birth By Sleep](index.md) - [Network Packages](../network-packages.md) - NetGameDef

## NetGameDef

### PKT_DLINK_DATA

| Name             | Value
|------------------|------
| LINK_STATUS_NONE | 0x0
| LINK_STATUS_REQ  | 0x1
| LINK_STATUS_OK   | 0x2
| LINK_STATUS_NG   | 0x3
| LINK_STATUS_BUSY | 0x4

### PKT_BATTLE_GAME

| Offset | Length | Type    | Name
|--------|--------|---------|-----
| 0x0    | 1      | byte    | m_nState
| 0x1    | 1      | byte    | m_nSubState
| 0x2    | 1      | byte    | m_nMVP
| 0x3    | 1      | byte    | m_nStartMember
| 0x4    | 1      | byte    | m_nGameMode
| 0x5    | 1      | byte    | m_nEventRule
| 0x6    | 1      | byte    | m_nDataMode
| 0x7    | 1      | byte    | m_nTimeLimit
| 0x8    | 1      | byte    | m_nMemberMax
| 0x9    | 1      | byte    | m_nField
| 0xA    | 1      | byte    | m_nTarget
| 0xB    | 1      | byte    | m_nRank
| 0xC    | 4      | uint32  | m_BattleFlag
| 0x10   | 4      | float   | m_fTimer
| 0x14   | 4      | uint32  | m_nPlayTime
| 0x18   | 3      | byte[3] | m_nReqStyle
| 0x1B   | 1      | byte    | m_ndum0
| 0x1C   | 2      | int16   | m_nLmtX
| 0x1E   | 2      | int16   | m_nLmtY
| 0x20   | 2      | int16   | m_nLmtZ
| 0x22   | 2      | int16   | m_nLmtR
| 0x24   | 2      | uint16  | m_nLmtKind
| 0x26   | 1      | byte    | m_nLmtCreator
| 0x27   | 1      | byte    | m_nLmtState
| 0x28   | 1      | byte    | m_nLmtResult
| 0x29   | 3      | byte[3] | m_nLmtMember

### PKT_BATTLE_PLAY

| Offset | Length | Type             | Name
|--------|--------|------------------|-----
| 0x0    | 212    | PKT_PLAYER_DATA  | m_datPlayer
| 0xD4   | 164    | PKT_GIMMICK_DATA | m_datGimmick
| 0x178  | 480    | PKT_ENEMY_DATA   | m_datEnemy

### PKT_BATTLE_VS_PLAY

| Offset | Length | Type                   | Name
|--------|--------|------------------------|-----
| 0x0    | 212    | PKT_PLAYER_DATA        | m_datPlayer
| 0xD4   | 164    | PKT_GIMMICK_DATA       | m_datGimmick
| 0x178  | 380    | PKT_COMMAND_PRIZE_DATA | m_datCmdPrize
| 0x2F4  | 60     | PKT_WOOLGIMMICK_DATA   | m_datWoolGimmick

### PKT_COMMAND_PRIZE_CREATE

| Offset | Length | Type                              | Name
|--------|--------|-----------------------------------|-----
| 0x0    | 4      | uint32                            | uiCount
| 0x4    | 4      | int32                             | iNum
| 0x8    | 8      | uint8[8]                          | uiReady
| 0x10   | 8      | uint8[8]                          | uiCompleteCreate
| 0x18   | 160    | PKT_COMMAND_PRIZE_CREATE_INFO[10] | Info

### PKT_COMMAND_PRIZE_CREATE_INFO

| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | uiID
| 0x2    | 2      | uint16 | uiCmdKind
| 0x4    | 2      | int16  | iPosX
| 0x6    | 2      | int16  | iPosY
| 0x8    | 2      | int16  | iPosZ
| 0xA    | 2      | int16  | iVelX
| 0xC    | 2      | int16  | iVelY
| 0xE    | 2      | int16  | iVelZ

### PKT_COMMAND_PRIZE_DATA

| Offset | Length | Type                           | Name
|--------|--------|--------------------------------|-----
| 0x0    | 4      | int32                          | iCmdPrizeNum
| 0x4    | 192    | PKT_ONE_COMMAND_PRIZE_DATA[48] | cmdPrizeData
| 0xC4   | 184    | PKT_COMMAND_PRIZE_CREATE       | cmdPrizeCreate

### PKT_CROWD_MUSH_JERRY

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | _HANDLE_  | hNetGameHandle (is a int32)
| 0x4    | 4      | int32     | iMushJerryNum

### PKT_DLINK_DATA

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0      | 224    | DL_STATUS | m_DLinkInfo

### PKT_ENEMY_DATA

| Offset | Length | Type                   | Name
|--------|--------|------------------------|-----
| 0x0    | 4      | uint32                 | iNum
| 0x4    | 432    | PKT_ONE_ENEMY_DATA[12] | enemyData
| 0x1B4  | 8      | PKT_CROWD_MUSH_JERRY   | crowdMushJerry
| 0x1BC  | 4      | PKT_MMCMASTER_DATA     | mmcMasterPkt
| 0x1C0  | 4      | float                  | fTimeCounterFrame
| 0x1C4  | 28     | int8[28]               | iWork

### PKT_GIMMICK_DATA

| Offset | Length | Type                     | Name
|--------|--------|--------------------------|-----
| 0x0    | 4      | int32                    | iNum
| 0x4    | 160    | PKT_ONE_GIMMICK_DATA[20] | gimmickData

### PKT_LOBBY_PLAY

| Offset | Length | Type            | Name
|--------|--------|-----------------|-----
| 0x0    | 212    | PKT_PLAYER_DATA | m_datPlayer
| 0xD4   | 224    | PKT_DLINK_DATA  | m_datDLink

### PKT_MMCMASTER_DATA

union

| Length | Type        | Name
|--------|-------------|-----
| 1      | uint8       | uiFlag
| 1      | anon struct |

anon struct

| Postition | Size | Name
|-----------|------|-----
| 0         | 6    | __dummy__
| 6         | 1    | uiSummons2
| 7         | 1    | uiSummons1


| Offset | Length | Type  | Name
|--------|--------|-------|-----
| 0x0    | 2      | int16 | iTimer
| 0x2    | 1      | union |
| 0x3    | 1      | int8  | iCount

### PKT_ONE_COMMAND_PRIZE_DATA

union

| Length | Type        | Name
|--------|-------------|-----
| 2      | uint16      | uiFlag
| 2      | anon struct |

anon struct

| Postition | Size | Name
|-----------|------|-----
| 0         | 6    | __dummy__
| 6         | 1    | uiReady
| 7         | 1    | uiMaster
| 8         | 8    | uiNetPlayerNum


| Offset | Length | Type   | Name
|--------|--------|--------|-----
| 0x0    | 2      | uint16 | uiID
| 0x2    | 2      | union  |

### PKT_ONE_ENEMY_DATA

union 1

| Length | Type          | Name
|--------|---------------|-----
| 2      | int16         | iGroup
| 2      | anon struct 1 |

anon struct 1

| Postition | Size | Name
|-----------|------|-----
| 0         | 6    | iAtkGroup
| 6         | 10   | iEffGroup

union 2

| Length | Type          | Name
|--------|---------------|-----
| 1      | uint8         | uiFlag
| 1      | anon struct 2 |

anon struct 2

| Postition | Size | Name
|-----------|------|-----
| 0         | 1    | __dummy__
| 1         | 4    | uiNetPlayerNum
| 5         | 1    | uiDamageReaction
| 6         | 1    | uiRef
| 7         | 1    | uiMaster


| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0x0    | 4      | __HANDLE__ | hNetGameHandle
| 0x4    | 4      | __HANDLE__ | hNetTargetHandle
| 0x8    | 4      | int32      | iState
| 0xC    | 2      | int16      | iVelX
| 0xE    | 2      | int16      | iVelY
| 0x10   | 2      | int16      | iVelZ
| 0x12   | 2      | int16      | iPosX
| 0x14   | 2      | int16      | iPosY
| 0x16   | 2      | int16      | iPosZ
| 0x18   | 2      | int16      | iRotX
| 0x1A   | 2      | int16      | iRotY
| 0x1C   | 2      | int16      | iHP
| 0x1E   | 1      | int8       | iMotionNum
| 0x1F   | 1      | int8       | iState
| 0x20   | 2      | union 1    |
| 0x22   | 1      | union 2    |
| 0x23   | 1      | int8       | dummy

### PKT_ONE_GIMMICK_DATA

union

| Length | Type        | Name
|--------|-------------|-----
| 1      | uint8       | uiFlag
| 1      | anon struct |

anon struct

| Postition | Size | Name
|-----------|------|-----
| 0         | 6    | __dummy__
| 6         | 1    | uiRef
| 7         | 1    | uiMaster


| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0x0    | 4      | __HANDLE__ | hNetGameHandle
| 0x4    | 2      | int16      | iStateCount
| 0x6    | 1      | int8       | iState
| 0x7    | 1      | union      |

### PKT_ONE_WOOLGIMMICK_DATA

union

| Length | Type        | Name
|--------|-------------|-----
| 1      | uint8       | uiFlag
| 1      | anon struct |

anon struct

| Postition | Size | Name
|-----------|------|-----
| 0         | 5    | __dummy__
| 5         | 1    | uiAttack
| 6         | 1    | uiRef
| 7         | 1    | uiMaster


| Offset | Length | Type       | Name
|--------|--------|------------|-----
| 0x0    | 4      | __HANDLE__ | hNetGameHandle
| 0x4    | 4      | __HANDLE__ | hNetTargetHandle
| 0x8    | 4      | uint32     | iState
| 0xC    | 2      | int16      | iVelX
| 0xE    | 2      | int16      | iVelY
| 0x10   | 2      | int16      | iVelZ
| 0x12   | 2      | int16      | iPosX
| 0x14   | 2      | int16      | iPosY
| 0x16   | 2      | int16      | iPosZ
| 0x18   | 2      | int16      | iRotY
| 0x1A   | 1      | union      |
| 0x1B   | 1      | uint8      | iPlayerID

### PKT_PLAYER_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | __HANDLE__   | m_hNetGameHandle
| 0x4    | 1      | byte         | m_nState
| 0x5    | 1      | byte         | m_nSubState
| 0x6    | 1      | byte         | m_nPlayerNum
| 0x7    | 1      | byte         | m_nChara
| 0x8    | 1      | byte         | m_nWeaponID
| 0x9    | 1      | byte         | m_nTeam
| 0xA    | 1      | byte         | m_nDeckType
| 0xB    | 1      | byte         | m_nField
| 0xC    | 12     | PLAYER_SCORE | m_nScore
| 0x18   | 1      | byte         | m_nResult
| 0x19   | 1      | byte         | m_nArenaLv
| 0x1A   | 1      | byte         | m_nLv
| 0x1B   | 1      | byte         | m_nRound
| 0x1C   | 1      | byte         | m_nWorkState
| 0x1D   | 1      | byte         | m_nWorkFlag
| 0x1E   | 1      | byte         | m_nLostFlag
| 0x1F   | 1      | byte:4       | m_nCheck9
| 0x1F   | 1      | byte:4       | m_nCheck8
| 0x20   | 1      | byte:4       | m_nCheck7
| 0x20   | 1      | byte:4       | m_nCheck6
| 0x21   | 1      | byte:4       | m_nCheck5
| 0x21   | 1      | byte:4       | m_nCheck4
| 0x22   | 1      | byte:4       | m_nCheck3
| 0x22   | 1      | byte:4       | m_nCheck2
| 0x23   | 1      | byte:4       | m_nCheck1
| 0x23   | 1      | byte:4       | m_nCheck0
| 0x24   | 1      | byte         | m_dum1
| 0x25   | 1      | byte         | m_dum0
| 0x26   | 2      | int16        | m_nHp
| 0x28   | 2      | int16        | m_nHpMax
| 0x2A   | 1      | byte         | m_nAp
| 0x2B   | 1      | byte         | m_nMp
| 0x2C   | 1      | byte         | m_nDp
| 0x2D   | 1      | byte         | m_nWork0
| 0x2E   | 2      | int16        | m_nStateTime
| 0x30   | 1      | byte         | m_nPlayerState
| 0x31   | 1      | byte         | m_nSubPlayerState
| 0x32   | 1      | undefined    | 
| 0x33   | 1      | undefined    | 
| 0x34   | 4      | uint32       | m_nPlayerFlag
| 0x38   | 4      | uint32       | m_nTrgFlag
| 0x3C   | 4      | uint32       | m_nAttackFlag
| 0x40   | 4      | uint32       | m_nDamageFlag
| 0x44   | 4      | uint32       | m_nColor
| 0x48   | 2      | uint16       | m_nCommandKind
| 0x4A   | 1      | byte         | m_nCommandLv
| 0x4B   | 1      | byte         | m_nStyleID
| 0x4C   | 2      | uint16       | m_nReplyKind
| 0x4E   | 1      | byte         | m_nHpHealLight
| 0x4F   | 1      | byte         | m_nShootLockNum
| 0x50   | 2      | int16        | m_nAnim
| 0x52   | 2      | int16        | m_nAnimTime
| 0x54   | 4      | byte[4]      | m_nAtkGrp
| 0x58   | 2      | int16        | m_nEffGroup
| 0x5A   | 2      | int16        | m_nSEChannel
| 0x5C   | 2      | int16        | m_nPosX
| 0x5E   | 2      | int16        | m_nPosY
| 0x60   | 2      | int16        | m_nPosZ
| 0x62   | 2      | int16        | m_nRotX
| 0x64   | 2      | int16        | m_nRotY
| 0x66   | 2      | int16        | m_nSclXYZ
| 0x68   | 2      | int16        | m_nOldX
| 0x6A   | 2      | int16        | m_nOldY
| 0x6C   | 2      | int16        | m_nOldZ
| 0x6E   | 2      | int16        | m_nVelX
| 0x70   | 2      | int16        | m_nVelY
| 0x72   | 2      | int16        | m_nVelZ
| 0x74   | 2      | uint16       | m_nPlayerStateCounter
| 0x76   | 2      | int16        | m_nAtkPower
| 0x78   | 2      | int16        | m_nAtkForce
| 0x7A   | 1      | byte         | m_nCryticalRate
| 0x7B   | 1      | byte         | m_AtkAttr
| 0x7C   | 4      | _HANDLE_     | m_hLockonTarget
| 0x80   | 4      | _HANDLE_     | m_hActionTarget
| 0x84   | 2      | uint16       | m_nAtkDataId
| 0x86   | 1      | byte         | m_nBltCount
| 0x87   | 1      | byte         | m_nBltId
| 0x88   | 3      | byte[3]      | m_nLoadStyle
| 0x8B   | 1      | byte         | m_nReqStyle
| 0x8C   | 2      | int16        | m_nLmtX
| 0x8E   | 2      | int16        | m_nLmtY
| 0x90   | 2      | int16        | m_nLmtZ
| 0x92   | 2      | int16        | m_nLmtR
| 0x94   | 2      | uint16       | m_nLmtKind
| 0x96   | 1      | byte         | m_nLmtCreator
| 0x97   | 1      | byte         | m_nLmtState
| 0x98   | 1      | byte         | m_nLmtResult
| 0x99   | 3      | byte[3]      | m_nLmtMember
| 0x9C   | 2      | uint16       | m_nReactionKind
| 0x9E   | 1      | byte         | m_nDeckNum
| 0x9F   | 1      | byte         | m_nAtkGroup
| 0xA0   | 2      | int16        | m_nCntX
| 0xA2   | 2      | int16        | m_nCntY
| 0xA4   | 2      | int16        | m_nCntZ
| 0xA6   | 2      | int16        | m_nCntR
| 0xA8   | 2      | uint16       | m_nAnimSetCount
| 0xAA   | 1      | byte         | m_nTargetPC
| 0xAB   | 1      | byte         | m_nDFinishStep
| 0xAC   | 2      | int16        | m_nCommandStep
| 0xAE   | 1      | byte         | m_nEnemyAttackerPC
| 0xAF   | 1      | byte         | m_nEnemyMurdererPC
| 0xB0   | 2      | int16        | m_nAnimSpd
| 0xB2   | 2      | int16        | m_nIllAnimSpd
| 0xB4   | 2      | int16        | m_nIllHP
| 0xB6   | 1      | byte         | m_nIllGauge
| 0xB7   | 1      | byte         | m_nIllLuaState
| 0xB8   | 1      | byte         | m_nIllAnimId
| 0xB9   | 1      | byte         | m_nIllState
| 0xBA   | 1      | byte         | m_nIllGravity
| 0xBB   | 1      | byte         | m_nIllGroundIn
| 0xBC   | 1      | byte         | m_nIllDamage
| 0xBD   | 1      | undefined    |
| 0xBE   | 2      | uint16       | m_nAtkKind
| 0xC0   | 17     | byte[17]     | m_nPad
| 0xD1   | 1      | undefined    |
| 0xD2   | 1      | undefined    |
| 0xD3   | 1      | undefined    |

### PKT_SELECT_DATA

| Offset | Length | Type         | Name
|--------|--------|--------------|-----
| 0x0    | 4      | __HANDLE__   | m_hNetGameHandle
| 0x4    | 1      | byte         | m_nState
| 0x5    | 1      | byte         | m_nSubState
| 0x6    | 1      | byte         | m_nPlayerNum
| 0x7    | 1      | byte         | m_nChara
| 0x8    | 1      | byte         | m_nWeaponID
| 0x9    | 1      | byte         | m_nTeam
| 0xA    | 1      | byte         | m_nDeckType
| 0xB    | 1      | byte         | m_nField
| 0xC    | 12     | PLAYER_SCORE | m_nScore
| 0x18   | 1      | byte         | m_nResult
| 0x19   | 1      | byte         | m_nArenaLv
| 0x1A   | 1      | byte         | m_nLv
| 0x1B   | 1      | byte         | m_nRound
| 0x1C   | 1      | byte         | m_nWorkState
| 0x1D   | 1      | byte         | m_nWorkFlag
| 0x1E   | 1      | byte         | m_nLostFlag
| 0x1F   | 1      | byte:4       | m_nCheck9
| 0x1F   | 1      | byte:4       | m_nCheck8
| 0x20   | 1      | byte:4       | m_nCheck7
| 0x20   | 1      | byte:4       | m_nCheck6
| 0x21   | 1      | byte:4       | m_nCheck5
| 0x21   | 1      | byte:4       | m_nCheck4
| 0x22   | 1      | byte:4       | m_nCheck3
| 0x22   | 1      | byte:4       | m_nCheck2
| 0x23   | 1      | byte:4       | m_nCheck1
| 0x23   | 1      | byte:4       | m_nCheck0
| 0x24   | 1      | byte         | m_dum1
| 0x25   | 1      | byte         | m_dum0
| 0x26   | 64     | COMMAND[8]   | m_cmdDeck
| 0x66   | 8      | COMMAND      | m_cmdFinish
| 0x6E   | 8      | COMMAND      | m_cmdShootLock
| 0x76   | 8      | COMMAND      | m_cmdJump
| 0x7E   | 8      | COMMAND      | m_cmdGlide
| 0x86   | 8      | COMMAND      | m_cmdAerialDash
| 0x8E   | 8      | COMMAND      | m_cmdGroundDash
| 0x96   | 8      | COMMAND      | m_cmdDashAbi
| 0x9E   | 8      | COMMAND      | m_cmdAvoidSlide
| 0xA6   | 8      | COMMAND      | m_cmdComboSlide
| 0xAE   | 8      | COMMAND      | m_cmdTurnAbi
| 0xB6   | 8      | COMMAND      | m_cmdGuard
| 0xBE   | 8      | COMMAND      | m_cmdGuardAbi
| 0xC6   | 8      | COMMAND      | m_cmdBlowAbi
| 0xCE   | 1      | undefined    |
| 0xCF   | 1      | undefined    |

### PKT_WOOLGIMMICK_DATA

| Offset | Length | Type                        | Name
|--------|--------|-----------------------------|-----
| 0x0    | 4      | int32                       | iNum
| 0x4    | 56     | PKT_ONE_WOOLGIMMICK_DATA[2] | woolGimmickData

### PLAYER_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 2      | uint16    | m_nMedal
| 0x6    | 2      | uint16    | m_nBonusP
| 0x8    | 2      | uint16    | m_nBonusM
| 0xA    | 1      | undefined | 
| 0xB    | 1      | undefined | 

### TEAM_SCORE

| Offset | Length | Type      | Name
|--------|--------|-----------|-----
| 0x0    | 4      | uint32    | m_nScore
| 0x4    | 1      | byte      | m_nWin
| 0x5    | 1      | byte      | m_nLose
| 0x6    | 1      | undefined | 
| 0x7    | 1      | undefined |