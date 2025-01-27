using libcsv;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands
{
    [HelpOption]
    [Command(Description = "mdlx file: dump scale rotation translation to csv")]
    internal class DumpMdlxSrtCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "mdlx input")]
        public string InputModel { get; set; } = null!;

        [Argument(1, Description = "csv output")]
        public string? OutputCsv { get; set; }

        [Option(Description = "use radians instead of degrees", ShortName = "r", LongName = "use-radians")]
        public bool UseRadians { get; set; }

        protected int OnExecute(CommandLineApplication app)
        {
            OutputCsv = Path.GetFullPath(OutputCsv ?? Path.GetFileNameWithoutExtension(InputModel) + ".csv");

            Console.WriteLine($"Writing to: {OutputCsv}");

            using Stream mdlxFile = File.OpenRead(InputModel);

            var bar = Bar.Read(mdlxFile);

            var modelEntry = bar.SingleOrDefault(it => it.Type == Bar.EntryType.Model)
                ?? throw new Exception("Model entry not found");

            var mdlx = Mdlx.Read(modelEntry.Stream);
            if (mdlx.Model is ModelSkeleton modelSkeleton)
            {
                var subModel = mdlx.SubModels.SingleOrDefault(it => it.Type == 3)
                    ?? throw new Exception("SubModel entry not found");

                using var writer = new StreamWriter(OutputCsv, false, new UTF8Encoding(true));
                var csv = new Csvw(writer, ',', '"');

                string FormatScale(float value) => value.ToString();

                string FormatRotation(float value) =>
                    UseRadians
                        ? value.ToString()
                        : (value * 180 / MathF.PI).ToString();

                string FormatTranslation(float value) => value.ToString();

                {
                    csv.Write("BoneIndex");
                    csv.Write("BoneParent");
                    csv.Write("Scale X");
                    csv.Write("Scale Y");
                    csv.Write("Scale Z");
                    csv.Write("Rotation X");
                    csv.Write("Rotation Y");
                    csv.Write("Rotation Z");
                    csv.Write("Translation X");
                    csv.Write("Translation Y");
                    csv.Write("Translation Z");
                }

                foreach (var bone in subModel.Bones)
                {
                    csv.NextRow();
                    csv.Write(bone.Index.ToString());
                    csv.Write(bone.Parent.ToString());
                    csv.Write(FormatScale(bone.ScaleX));
                    csv.Write(FormatScale(bone.ScaleY));
                    csv.Write(FormatScale(bone.ScaleZ));
                    csv.Write(FormatRotation(bone.RotationX));
                    csv.Write(FormatRotation(bone.RotationY));
                    csv.Write(FormatRotation(bone.RotationZ));
                    csv.Write(FormatTranslation(bone.TranslationX));
                    csv.Write(FormatTranslation(bone.TranslationY));
                    csv.Write(FormatTranslation(bone.TranslationZ));
                }
            }
            else
            {
                throw new Exception($"Model type {mdlx.Model?.GetType()?.FullName} is not a skeleton");
            }

            return 0;
        }
    }
}
