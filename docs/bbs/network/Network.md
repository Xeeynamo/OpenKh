## Network

Following ones were are split up as this make more sense then they're all together
These are 4 bytes in size

### State

| Name                 | Value
|----------------------|------
| STATE_0              | 0x0
| STATE_CONNECT_IBSS   | 0x1
| STATE_MATCHING_LOBBY | 0x2
| STATE_PLAYING_GAME   | 0x3
| STATE_MAX            | 0x4

### EVF

| Name           | Value
|----------------|------
| EVF_ERROR      | 0x1
| EVF_CONNECT    | 0x2
| EVF_DISCONNECT | 0x4
| EVF_SCAN       | 0x8
| EVF_GAMEMODE   | 0x10

### Flag

| Name                           | Value
|--------------------------------|------
| FLAG_INIT_MODULES              | 0x1
| FLAG_CONNECTED_IBSS            | 0x2
| FLAG_CONNECTED_LOBBY           | 0x4
| FLAG_CONNECTED_ROOM            | 0x8
| FLAG_ENTERED_ROOM              | 0x10
| FLAG_READY_GAME                | 0x20
| FLAG_START_READY               | 0x40
| FLAG_START_GAME                | 0x80
| FLAG_PLAYING_GAME              | 0x100
| FLAG_LOBBY_PLAY_SET            | 0x200
| FLAG_LOBBY_PLAY_ON             | 0x400
| FLAG_LOBBY_OVER                | 0x800
| FLAG_PEERLIST_OK               | 0x10000
| FLAG_MMEMBER_OK                | 0x20000
| FLAG_INIT_MODULE_PSPNETINIT    | 0x100000
| FLAG_INIT_MODULE_ADHOCINIT     | 0x200000
| FLAG_INIT_MODULE_ADHOCCTRLINIT | 0x400000

### Frame

| Name                  | Value
|-----------------------|------
| FRAME_UTTILITY_UPDATE | 0x2

### Game

| Name               | Value
|--------------------|------
| GAME_NICKNAME_SIZE | 0x26

### JOB

| Name      | Value
|-----------|------
| _JOB_PRI_ | 0x9C40

___