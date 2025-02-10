using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenKh.Kh2;

namespace OpenKh.Godot.Resources;

[Tool, GlobalClass]
public partial class KH2Moveset : Resource
{
    public enum MovesetType
    {
        Default,
        Player,
        Raw
    }
    public enum PlayerAnimationStatus
    {
        InBattleHasWeapon,
        InBattleNoWeapon,
        OutBattleNoWeapon,
        OutBattleHasWeapon,
    }

    public static readonly IReadOnlyDictionary<PlayerAnimationStatus, int[]> AnimationTryOrder =
        new System.Collections.Generic.Dictionary<PlayerAnimationStatus, int[]>
        {
            { PlayerAnimationStatus.InBattleHasWeapon, [0, 1, 3, 2] },
            { PlayerAnimationStatus.InBattleNoWeapon, [1, 0, 2, 3] },
            { PlayerAnimationStatus.OutBattleNoWeapon, [2, 3, 1, 0] },
            { PlayerAnimationStatus.OutBattleHasWeapon, [3, 2, 0, 1] },
        }.AsReadOnly();

    [Export] public MovesetType Type;
    [Export] public Array<KH2MovesetEntry> Entries = new();

    public KH2MovesetEntry GetEntry(int index, PlayerAnimationStatus status = PlayerAnimationStatus.InBattleHasWeapon) =>
        Type == MovesetType.Player ? AnimationTryOrder[status].Select(o => Entries[(index * 4) + o]).FirstOrDefault(get => get is not null) : Entries[index];
}
