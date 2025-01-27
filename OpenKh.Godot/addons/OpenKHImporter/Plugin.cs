/*
#if TOOLS
using System;
using Godot;
using Godot.Collections;

namespace OpenKh.Godot.addons.OpenKHImporter;

[Tool]
public partial class Plugin : EditorPlugin
{
	private static readonly Type[] ImporterTypes =
	[
		typeof(MdlsImporter),
		typeof(MdlxImporter),
		typeof(ScdImporter),
		typeof(CvblImporter),
		typeof(MsetImporter),
        typeof(MapImporter),
        typeof(InterfaceLayoutImporter),
        typeof(InterfaceSequenceImporter),
	];
	private Array<EditorImportPlugin> Importers = new();
	public override void _EnterTree()
    {
		foreach (var t in ImporterTypes)
		{
			var importer = Activator.CreateInstance(t) as EditorImportPlugin;
			Importers.Add(importer);
			AddImportPlugin(importer);
		}
	}

	public override void _ExitTree()
	{
		foreach (var importer in Importers) RemoveImportPlugin(importer);
		Importers.Clear();
	}
}
#endif
*/
//deprecated
