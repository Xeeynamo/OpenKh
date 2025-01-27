namespace OpenKh.Command.AnbMaker.Commands.Interfaces
{
    internal interface IFbxSourceItemSelector
    {
        string? RootName { get; }
        string? MeshName { get; }
        string? AnimationName { get; }
    }
}
