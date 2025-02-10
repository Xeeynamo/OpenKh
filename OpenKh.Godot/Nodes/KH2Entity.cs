using System;
using System.Collections.Generic;
using Godot;
using OpenKh.Godot.Helpers;
using OpenKh.Godot.Resources;
using OpenKh.Godot.Storage;
using OpenKh.Kh2;

namespace OpenKh.Godot.Nodes;

[Tool]
public partial class KH2Entity : CharacterBody3D
{
    public struct Input
    {
        public Vector2 Movement;
    }
    public enum EntityStatus
    {
        Default,
    }
    [Export]
    public int ObjectEntry
    {
        get => _entry;
        set
        {
            if (_entry == value) return;
            if (!KH2ObjectEntryTable.Entries.TryGetValue(value, out var objEntry)) return;
            
            _entry = value;

            if (Model is not null)
            {
                RemoveChild(Model);
                Model.QueueFree();
                Model = null;
            }
            Model = PackAssetLoader.GetMdlx(objEntry);
            Moveset = PackAssetLoader.GetMoveset(objEntry);
            
            
        }
    }
    private int _entry = -1;

    public Objentry ObjEntry => KH2ObjectEntryTable.Entries.GetValueOrDefault(_entry);

    [Export] public KH2Mdlx Model;
    [Export] public KH2Moveset Moveset;
    [Export] public CollisionShape3D CollisionShape;
    [Export] public CapsuleShape3D CapsuleShape;
    [Export] public float FacingRotation;
    [Export] public EntityStatus Status;
    public bool Grounded { get; private set; }

    public override void _Ready()
    {
        base._Ready();

        if (CollisionShape is null)
        {
            CollisionShape = new CollisionShape3D();
            AddChild(CollisionShape);
        }
        if (CapsuleShape is null) CapsuleShape = new CapsuleShape3D();
        CollisionShape.Shape = CapsuleShape;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var deltaf = (float)delta;

        if (Engine.IsEditorHint()) return;

        var obj = ObjEntry;
        UpdateGrounded();
        MotionMode = Grounded ? MotionModeEnum.Grounded : MotionModeEnum.Floating;

        var input = CurrentInput;

        switch (Status)
        {
            case EntityStatus.Default:
            {
                
                break;
            }
        }
        
        var horizontalVel = Velocity.XZ();
        
        MoveAndSlide();
    }

    protected virtual Input CurrentInput => default;

    private void UpdateGrounded()
    {
        Grounded = false;
        var collisions = GetSlideCollisionCount();
        for (var i = 0; i < collisions; i++)
        {
            var collision = GetSlideCollision(i);
            var multiCollisions = collision.GetCollisionCount();
            for (var j = 0; j < multiCollisions; j++)
            {
                var other = collision.GetCollider(j);
                if (other is not CollisionObject3D col) continue;
                var layer = (CollisionHelpers.CoctFlags)col.CollisionLayer;
                if ((layer & (CollisionHelpers.CoctFlags.PartyStand | CollisionHelpers.CoctFlags.HitPlayer)) > 0)
                {
                    Grounded = true;
                    return;
                }
            }
        }
    }
}
