# [Kingdom Hearts II](../../index.md) - Area Data (ARD file format)

Describes how the game should behave before loading the map. This is responsible to spawn the player, NPC, enemies, run cutscenes, change music, invoke [MSN](msn.md) files and way more.

Internally it is a [BAR](bar.md) file, composed by micro files of different purpose:

- [Spawn](#spawn) to spawn objects and triggers.
- [Script](#script) to execute microcode
- Event responsible to play a cutscene
- [AI](ai.md) to execute exceptional microcode.

## Spawn Point

This is the easiest micro-format of an ARD file. This is mainly responsible to spawn objects inside a map, to describe how NPCs should roaming around, to describe how to interact with a specific NPC or even triggering an event like a cutscene or just by changing a map.

### File structure

#### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File type; always 2.
| 04     | int   | [Spawn descriptor count](#spawn-descriptor)

#### Spawn descriptor

When referring to _entrance_ it means that is the index where to spawn the character to when loading the next map (simplified: which part of the map you're coming from). When the entrance is 99, the playable character is spawn where the save point is located.

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | Type
| 01     | byte  | Flag
| 02     | short | Id
| 04     | short | [Entity count](#entity)
| 06     | short | [Event activator count](#event-activator)
| 08     | short | [Walk path count](#walk-path). Usually 1.
| 0a     | short | [Unknown tableA count](#unknown-table-a)
| 0c     | int   | [Signal count](#signal-table)
| 10     | int   | Always 0 [*¹](#notes)
| 14     | int   | Always 0 [*¹](#notes)
| 18     | int   | Always 0 [*¹](#notes)
| 1c     | byte  | [Area ID of a world](../../worlds.md)
| 1d     | byte  | Entrance
| 1e     | byte  | Approach Direction Trigger
| 1f     | byte  | Unknown
| 20     | int   | Unknown
| 24     | int   | Unknown
| 28     | int   | Always 0 [*¹](#notes)

Approach Direction Trigger determines if the event is triggered only when approaching from a specific direction or if it can be triggered by approaching it from any direction (even from above/below). An example of the former would be the room transition used in CoR skip where you land behind the trigger and have to move away from the exit for a bit in order to move to CoR: Depths. An example of the latter would be Cloud's mandatory cutscene in HB.

#### Entity

| Offset | Type  | Description
|--------|-------|------------
| 0x00     | int   | [Model ID](./00objentry.md)
| 04     | float | Position X  
| 08     | float | Position Y
| 12     | float | Position Z
| 16     | float | Rotation X
| 14     | float | Rotation Y
| 18     | float | Rotation Z
| 1c     | byte  | [Spawn type](#spawn-types)
| 1d     | byte  | Spawn argument
| 1e     | short | Serial
| 20     | int   | Argument 1
| 24     | int   | Argument 2
| 28     | int   | Talk message
| 2c     | int   | Reaction command
| 30     | int   | Unknown
| 34     | int   | Always 0 [*¹](#notes)
| 38     | int   | Always 0 [*¹](#notes)
| 3c     | int   | Always 0 [*¹](#notes)

##### Spawn types

| ID | Description | Argument purpose
|----|-------------|-----------------
| 0  | Do nothing  |
| 1  | Spawn at entrance | Filter by entrance index
| 2  | Used by enemies | Unknown
| 3  | Used by enemies | Unknown

#### Event activator

This is an invisible wall that is responsibe to activate an event. Which event is it, it is described in the [spawn point descriptor](#spawn-descriptor). One common usage is changing the map when the player touch the map's "border".

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Collision box shape](#event-shape)
| 02     | short | [Option](#event-option)
| 04     | float | Position X  
| 08     | float | Position Y
| 0c     | float | Position Z
| 10     | float | Scale X
| 14     | float | Scale Y
| 18     | float | Scale Z
| 1c     | float | Rotation X
| 20     | float | Rotation Y
| 24     | float | Rotation Z
| 28     | int   | Flags
| 2c     | short | Type
| 2e     | short | BG group on
| 30     | short | BG group off
| 32     | short | Always 0 [*¹](#notes)
| 34     | int   | Always 0 [*¹](#notes)
| 38     | int   | Always 0 [*¹](#notes)
| 3c     | int   | Always 0 [*¹](#notes)

##### Event shape

| ID | Description
|----|-------------
| 0  | Parallelepiped?
| 1  | Sphere?

##### Event option

| ID | Description
|----|-------------
| 0  | Change map?
| 1  | Show next map name?

#### Walk path

Instructs all the [entities](#entity) to follow a walk path, defined by multiple points in the map.

From some early tests, the entities would pick a random point in the walk path and walk there from they were started from. There is no known way to make it follow in order. Also when the map is loaded, the entity can be randomly teleported in any of those walk path points.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Serial
| 02     | short | [Walk point count](#walk-point)
| 04     | byte  | Flag
| 04     | byte  | Id
| 06     | short | Always 0 [*¹](#notes)

#### Walk point

Just a 12-byte structure, read as Vector3f.

### Unknown table A

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | Unknown
| 01     | byte  | Unknown
| 02     | byte  | Unknown
| 03     | byte  | Unknown
| 04     | byte  | Unknown
| 05     | byte  | Unknown
| 06     | short | Unknown
| 08     | int   | Unknown
| 0c     | int   | Unknown

### Signal table

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Signal ID
| 02     | short | Argument
| 04     | byte  | Action
| 05     | byte  | Always 0 [*¹](#notes)
| 06     | byte  | Always 0 [*¹](#notes)
| 07     | byte  | Always 0 [*¹](#notes)

## Script

This is a micro-code that is responsible to change the behaviour of a specific map.

Every map have three script files: `map`, `btl` and `evt`, loaded in this very speicfic order. Each file have a list of programs, where every program is represented by an unique ID. When a map loads, the game instructs the map loader which program to load ([sub_181cc0](#notes)). A map can load only a single program per script file by their ID. This is done by the Local Set or by reading those information in the save data area. The IDs varies from 0 to 50 and it is unique per area. While all the IDs starting from 51 are unique per world. For instance, you can find the Program ID 6 in both TT02 and TT04. But you can only find the Program ID 55 in for the area 4 for Twilight Town world. This is helpful be coherent with the Local Set based on the story progress.

### Structure

The file begins with a list of programs, defined as followed:

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Program](#program) ID
| 02     | short | [Program](#program) length in bytes
| 04     | byte[] | Program data

### Program

Each program is a list of functions, defined as followed:

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Operation code](#operation-code)
| 02     | short | Parameter count for the operation
| 04     | int[] | Parameters

### Operation code

There is a total of 30 operation codes for the spawn script. The parser can be found at [sub_181d30](#notes).

- 00: [Spawn](#spawn)
- 01: [MapOcclusion](#mapocclusion)
- 02: [RandomSpawn](#randomspawn)
- 03: [CasualSpawn](#casualspawn)
- 04: [Capacity](#capacity)
- 05: [AllocEnemy](#allocenemy)
- 06: [unknown](#unknown06)
- 07: [unknown](#unknown07)
- 09: [unknown](#unknown09)
- 0a: [unknown](#unknown0a)
- 0b: [BarrierFlag](#barrierflag)
- 0c: [AreaSettings](#areasettings)
- 0e: [unknown](#unknown0e)
- 0f: [Party](#party)
- 10: [Bgm](#bgm)
- 11: [MsgWall](#msgwall)
- 12: [AllocPacket](#allocpacket)
- 13: [Camera](#camera)
- 14: [StatusFlag3](#statusflag3)
- 15: [Mission](#mission)
- 16: [Bar](#bar)
- 17: [StatusFlag5](#statusflag5)
- 18: [AllocEffect](#alloceffect)
- 19: [Progress](#progress)
- 1a: [VisibilityOn](#visibilityon)
- 1b: [VisibilityOff](#visibilityoff)
- 1c: [If](#if)
- 1d: [unknown](#unknown1d)
- 1e: [BattleLevel](#battlelevel)
- 1f: [unknown](#unknown1f)

#### Spawn

Loads a [Spawn](#spawn) file. The parameter is a 4-byte string with the name of the spawn of the ARD's BAR file. This is the most used opcode, with a usage count of 11949.

#### MapOcclusion

Specify a 64-bit mask that is responsible to enable or disable specific map meshes and collisions. This is very common and used 2186 times.

#### RandomSpawn

This is used to randomly spawn a group of enemies. It is very similar to [Spawn](#spawn) but it defines multiple spawns and the game will [randomly](../../rng.md) choose one of them.

Not common (25 times), used in maps like `bb00`, `bb06`, `ca14`, etc. .

#### CasualSpawn

Casually spawn or not a specific enemy group. Defines an integer and, if the [random](../../rng.md) number is inferior to the one defined, then the enemy group will spawn. The smaller the number is, the less are the chances to trigger that specific spawn. The enemy group is specified as a 4-byte string, exactly as [Spawn](#spawn).
Very uncommon, used 17 times and only in the worlds BB, CA, LK and MU.

#### Capacity

Set the memory area `01c6053c`, which represents an integer that holds the capacity of the current map. The bigger is the capacity the better is the amount of enemies that can be loaded at once. It's found 465 times and only in `btl`.

#### AllocEnemy

Set the memory area `0034ecd0`, which represents the amount of memory reserved for enemies. It is not exactly clear why and how this is used. Found 34 times almost every time in `btl`.

#### Unknown06

Set the memory area `0034ecd8` with the 4-byte parameter. The purpose of that memory area is unknown. Found 64 times and only in `btl`.

#### Unknown07

Set the memory area `0034ecdc` with the 4-byte parameter. The purpose of that memory area is unknown. Very uncommon as it's ony found 7 times in the maps `ca12` and `nm02` in `btl`.

#### Unknown09

Looks like similar to [Spawn](#spawn), but it's way less used. Found 210 times, mostly in `map` and just once in `wi00` as `evt`.

#### Unknown0a

Found 73 times. Purpose unknown.

#### Barrier

Set the memory area `0034ecc8`, which seems to define which parts of the map are blocked by an invisible barrier. It might be related on enabling or disabling certain collisions of a map file. Found 203 times and only in `map`.

#### AreaSettings

Enqueue a message to perform a series of actions. Here it is possible to play an event, jump into another map, set the story flags, set the menu flags, set specific party members, obtain items or invoke the party menu. Refer to [Area settings script](#area-settings-script) to know more. This is commonly used, as the usage count tops 2408 times. It is only found in `evt` scripts.

#### Unknown0e

Set the memory area `0034ece0` with the first 4-byte parameter. The purpose of that memory area is unknown. Used 323 times and only in `map`.

#### Party

Set how the party member needs to be structured. According to `dbg/member.bin`, those are the only allowed values as a parameter:

| Value | Name          | Description
|-------|---------------|-------------
| 0     | NO_FRIEND     | Allow only Sora
| 1     | DEFAULT       | Allow Donald and Goofy
| 2     | W_FRIEND      | Allow Donald, Goofy and world guest.
| 3     | W_FRIEND_IN   | Same as `W_FRIEND` but forces the world guest to be in at the beginning.
| 4     | W_FRIEND_FIX  | Same as `W_FRIEND_IN`, but you can not swap the world guest.
| 5     | W_FRIEND_ONLY | Allow only with Sora and the world guest.
| 6     | DONALD_ONLY   | Only allow Sora and Donald.

#### Bgm

Set the map's background musics. The single 4-byte parameter can be read as two 2-byte integers, which represents the field and battle music ID that can be found in `bgm/music_xxx`. They overrides the default field and battle music used in the current map.

#### Unknown11

Set the memory area `0034ece4`, which represents the message displayed below when the player touches an invisible wall. Found 80 times and only in `map`.

#### AllocPacket

Set the memory area `0034ecd4`, which is related to the amount of memory reserved for the cache buffer. This opcode is unused.

#### Unknown13

Set the memory area `0034ece8`, which sets the camera mode for the map with the first 4-byte parameter. It does have a parameter of `1` or `2`. Found 39 times and only in `map` for `hb17`, `lk13`, `mu07` and `po00`.

#### StatusFlag3

Purpose unknown. Set the 3rd bit of the memory area `0034f240`. Used 89 times.

#### Mission

Loads a [mission](msn.md) file by specifying its MSN file name without path and extension.

#### Bar

Loads a [BAR](bar.md) file by specifying the entire path. The `%s` symbol can be specified in the path to not hard-code the script to work just for a single language. The BAR file can be a [layout](2ld.md) or a `minigame` file.

#### StatusFlag5

Set the 5th bit of the memory area `0034f240`. This is only used twice in `mu07` and might be related to the mechanic of Sora entering in the tornado.

#### AllocEffect

Set the memory area `0034ecec` and `0034ecf0` with the first two parameters, which seems to be used by the particle system. It is only used by `hb33`, `hb34`, `hb38` and `he15` in `btl`.

#### Progress

Set story flag.

#### VisibilityOn

It seems to do something with `01c60548`. It is only used 3 times in `hb13` by `evt`.

#### VisibilityOff

It seems to do something with `01c60550`. It is only used 3 times in `hb13` by `evt`.

#### Unknown1c

Seems to recursively call the spawn script parser, but it is only used in `he09` for 130 times. Purpose unknown.

#### Unknown1d

Auto revert Sora when the room is entered if the argument is 4 and disables drive if the argument is 5. Used 196 times.

#### BattleLevel

Override the battle level of the playing map. Usually used for special boss battle only.

#### Unknown1f

Purpose unknown. Very similar to [Unknown03](#unknown03)

### Area settings script

The scene script contains functions with a variable amount of parameters.

| Opcode | Name     | Description
|--------|----------|-------------
| 00     | Event    | Play an event. Parameters: type, event name.
| 01     | Jump     | Change maps. Parameters: padding, type, world, area, entrance, localset, fade type.
| 02     | ProgressFlag | Update story progress.
| 03     | MenuFlag | Unlock options in the menu.
| 04     | Member   | Change party member.
| 05     |          | Heals Sora's stats depending on the argument (auto revert, full heal, HP&MP only)
| 06     | Inventory | Obtain one or more items. It is possible to obtain up to 7 items in a row.
| 07     | PartyMenu | Shows the party menu if Member is also defined.
| 08     |          | Sets a flag. Purpose unknown.

## Notes

*¹ When referred as _always 0_ means that there are no game files that set it as a value different than that. Still it is unknown if the field is actually used in-game, which would potentially lead to unused functionalities.

*² When `sub_`_`xxxxxx`_ or a memory address is read, they refer to the values found in the retail version of Kingdom Hearts II: Final Mix, executable `SLPM_66675.elf`.
