using Godot;
using OpenKh.Godot.Resources;

namespace OpenKh.Godot.Nodes
{
    [Tool]
    public partial class KH2InterfaceSprite : Node2D
    {
        [Export] public KH2InterfaceSequenceAnimation Animation;
        [Export] public MeshInstance2D Mesh;
        [Export] public Node2D RotationNode1;
        [Export] public Node2D RotationNode2;
        [Export] public Node2D PositionNode;
        [Export] public Node2D ScaleNode;
        [Export] public Node2D PivotNode;

        public static KH2InterfaceSprite Create(Mesh mesh, Texture2D texture)
        {
            var node = new KH2InterfaceSprite();
            
            var rotationNode1 = new Node2D();
            var rotationNode2 = new Node2D();
            var positionNode = new Node2D();
            var scaleNode = new Node2D();
            var pivotNode = new Node2D();

            node.PivotNode = pivotNode;
            node.PositionNode = positionNode;
            node.ScaleNode = scaleNode;
            node.RotationNode1 = rotationNode1;
            node.RotationNode2 = rotationNode2;
                    
            node.AddChild(positionNode);
            positionNode.AddChild(scaleNode);
            scaleNode.AddChild(rotationNode1);
            rotationNode1.AddChild(rotationNode2);
            rotationNode2.AddChild(pivotNode);
            
            var meshInstance = new MeshInstance2D();
            pivotNode.AddChild(meshInstance);
            node.Mesh = meshInstance;
            
            meshInstance.Mesh = mesh;
            meshInstance.Texture = texture;

            return node;
        }
    }
}
