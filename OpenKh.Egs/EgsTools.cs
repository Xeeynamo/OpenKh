using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Egs
{
    public class EgsTools
    {
        private const string RAW_FILES_FOLDER_NAME = "raw";
        private const string ORIGINAL_FILES_FOLDER_NAME = "original";
        private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

        #region MD5 names

        private static readonly IEnumerable<string> KH2Names = IdxName.Names
            .Concat(IdxName.Names.Where(x => x.Contains("anm/")).SelectMany(x => new string[]
            {
                    x.Replace("anm/", "anm/jp/"),
                    x.Replace("anm/", "anm/us/"),
                    x.Replace("anm/", "anm/fm/")
            }))
            .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                Kh2.Constants.WorldIds.SelectMany(world =>
                    Enumerable.Range(0, 64).Select(index => Path.Combine("ard", lang).Replace('\\', '/') + $"/{world}{index:D02}.ard"))))
            .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                Kh2.Constants.WorldIds.SelectMany(world =>
                    Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.map"))))
            .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                Kh2.Constants.WorldIds.SelectMany(world =>
                    Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.bar"))))
            .Concat(IdxName.Names.Where(x => x.StartsWith("bgm/")).Select(x => x.Replace(".bgm", ".win32.scd")))
            .Concat(IdxName.Names.Where(x => x.StartsWith("se/")).Select(x => x.Replace(".seb", ".win32.scd")))
            .Concat(IdxName.Names.Where(x => x.StartsWith("vagstream/")).Select(x => x.Replace(".vas", ".win32.scd")))
            .Concat(IdxName.Names.Where(x => x.StartsWith("gumibattle/se/")).Select(x => x.Replace(".seb", ".win32.scd")))
            .Concat(IdxName.Names.Where(x => x.StartsWith("voice/")).Select(x => x
                .Replace(".vag", ".win32.scd")
                .Replace(".vsb", ".win32.scd")))
            .Concat(new string[]
            {
                    "item-011.imd",
                    "KH2.IDX",
                    "ICON/ICON0.PNG",
                    "ICON/ICON0_EN.png",
            });

        public static readonly Dictionary<string, string> Names = KH2Names
            .Concat(Idx1Name.Names)
            .Concat(EgsHdAsset.DddNames)
            .Concat(EgsHdAsset.BbsNames)
            .Concat(EgsHdAsset.RecomNames)
            .Concat(EgsHdAsset.MareNames)
            .Concat(EgsHdAsset.SettingMenuNames)
            .Concat(EgsHdAsset.TheaterNames)
            .Concat(EgsHdAsset.Kh1AdditionalNames)
            .Concat(EgsHdAsset.Launcher28Names)
            .Concat(new string[] { "dummy.txt" })
            .Distinct()
            .ToDictionary(x => Helpers.ToString(MD5.HashData(Encoding.UTF8.GetBytes(x))), x => x);

        #endregion

        #region Extract

        public enum ExtractFileOutputMode
        {
            SeparateRoot,
            Adjacent,
        }

        public static void Extract(string inputHed, string output, bool doNotExtractAgain = false, ExtractFileOutputMode outputMode = ExtractFileOutputMode.SeparateRoot)
        {
            var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
            using var hedStream = File.OpenRead(inputHed);
            using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

            foreach (var entry in Hed.Read(hedStream))
            {
                var hash = Helpers.ToString(entry.MD5);
                if (!Names.TryGetValue(hash, out var fileName))
                    fileName = $"{hash}.dat";

                var outputFileName = outputMode switch
                {
                    ExtractFileOutputMode.Adjacent => Path.Combine(outputDir, fileName),
                    _ => Path.Combine(outputDir, ORIGINAL_FILES_FOLDER_NAME, fileName),
                };

                if (doNotExtractAgain && File.Exists(outputFileName))
                    continue;

                Console.WriteLine(outputFileName);
                CreateDirectoryForFile(outputFileName);

                var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));

                File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

                outputFileName = outputMode switch
                {
                    ExtractFileOutputMode.Adjacent => Path.Combine(outputDir, $"{fileName}.remastered.d"),
                    _ => Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName),
                };

                foreach (var asset in hdAsset.Assets)
                {
                    var outputFileNameRemastered = Path.Combine(GetHDAssetFolder(outputFileName), asset);

                    Console.WriteLine(outputFileNameRemastered);
                    CreateDirectoryForFile(outputFileNameRemastered);

                    var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                    File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                }
            }
        }

        public static void ExtractRaw(string inputHed, string output, bool doNotExtractAgain = false)
        {
            var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
            using var hedStream = File.OpenRead(inputHed);
            using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

            foreach (var entry in Hed.Read(hedStream))
            {
                var hash = Helpers.ToString(entry.MD5);
                if (!Names.TryGetValue(hash, out var fileName))
                    fileName = $"{hash}.dat";

                var outputFileName = Path.Combine(outputDir, RAW_FILES_FOLDER_NAME, fileName);

                if (doNotExtractAgain && File.Exists(outputFileName))
                    continue;

                Console.WriteLine(outputFileName);
                CreateDirectoryForFile(outputFileName);

                byte[] rawData = img.ReadBytes(entry.DataLength);
                File.Create(outputFileName).Using(stream => stream.Write(rawData));
            }
        }

        public static string GetHDAssetFolder(string assetFile)
        {
            var parentFolder = Directory.GetParent(assetFile).FullName;
            var assetFolderName = Path.Combine(parentFolder, $"{Path.GetFileName(assetFile)}");

            return assetFolderName;
        }

        public static void CreateDirectoryForFile(string fileName)
        {
            var directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
        }

        #endregion

        #region Patch

        public static void Patch(string pkgFile, string inputFolder, string outputFolder)
        {
            // Get files to inject in the PKG to detect if we want to include new files or not
            // We only get the original files as for me it doesn't make sense to include
            // new "remastered" asset since it must be linked to an original one
            var patchFiles = new List<string>();
            if (Directory.Exists(Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME)))
                patchFiles = Helpers.GetAllFiles(Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME)).ToList();
            if (Directory.Exists(Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME)))
                patchFiles.AddRange(Helpers.GetAllFiles(Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME)).ToList());

            var filenames = new List<string>();
            var outputDir = outputFolder ?? Path.GetFileNameWithoutExtension(pkgFile);
            var hedFile = Path.ChangeExtension(pkgFile, "hed");
            using var hedStream = File.OpenRead(hedFile);
            using var pkgStream = File.OpenRead(pkgFile);
            var hedHeaders = Hed.Read(hedStream).ToList();

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
            using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

            foreach (var hedHeader in hedHeaders)
            {
                var hash = Helpers.ToString(hedHeader.MD5);

                // We don't know this filename, we ignore it
                if (!Names.TryGetValue(hash, out var filename))
                {
                    Console.WriteLine($"Unknown filename (hash: {hash})");
                    continue;
                }

                if (patchFiles.Contains(filename))
                {
                    patchFiles.Remove(filename);
                }

                filenames.Add(filename);

                var asset = new EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

                if (hedHeader.DataLength > 0)
                {
                    ReplaceFile(inputFolder, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                }
                else
                {
                    Console.WriteLine($"Skipped: {filename}");
                }
            }

            // Add all files that are not in the original HED file and inject them in the PKG stream too
            foreach (var filename in patchFiles)
            {
                AddFile(inputFolder, filename, patchedHedStream, patchedPkgStream);
                Console.WriteLine($"Added a new file: {filename}");
            }
        }

        public static Hed.Entry AddFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, bool shouldCompressData = false, bool shouldEncryptData = false)
        {
            var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
            var completeRawFilePath = Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME, filename);
            var offset = pkgStream.Position;
            int actualLength = 0;

            #region Data
            if (File.Exists(completeFilePath))
            {
                using var newFileStream = File.OpenRead(completeFilePath);
                actualLength = (int)newFileStream.Length;

                bool RemasterExist = false;
                string RemasteredPath = completeFilePath.Replace("\\original\\", "\\remastered\\");
                if (Directory.Exists(RemasteredPath))
                    RemasterExist = true;

                var header = new EgsHdAsset.Header()
                {
                    // CompressedLenght => -2: no compression and encryption, -1: no compression 
                    CompressedLength = !shouldCompressData ? !shouldEncryptData ? -2 : -1 : 0,
                    DecompressedLength = (int)newFileStream.Length,
                    RemasteredAssetCount = 0,
                    CreationDate = -1
                };

                var decompressedData = newFileStream.ReadAllBytes();
                // Make sure to align asset data on 16 bytes
                if (decompressedData.Length % 0x10 != 0)
                {
                    int diff = 16 - (decompressedData.Length % 0x10);
                    byte[] paddedData = new byte[decompressedData.Length + diff];
                    decompressedData.CopyTo(paddedData, 0);
                    Enumerable.Repeat((byte)0xCD, diff).ToArray().CopyTo(paddedData, decompressedData.Length);
                    decompressedData = paddedData;
                }

                var compressedData = decompressedData.ToArray();

                if (shouldCompressData)
                {
                    compressedData = Helpers.CompressData(decompressedData);
                    header.CompressedLength = compressedData.Length;
                }

                SDasset sdasset = new SDasset(filename, decompressedData, RemasterExist, false);
                RemasterExist = false;

                if (sdasset != null && !sdasset.Invalid)
                    header.RemasteredAssetCount = sdasset.TextureCount;

                // Encrypt and write current file data in the PKG stream
                // The seed used for encryption is the original data header
                var seed = new MemoryStream();
                BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                var encryptionSeed = seed.ReadAllBytes();
                var encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;

                // Write original file header
                BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                if (header.RemasteredAssetCount > 0)
                {
                    // Create an "Asset" to pass to ReplaceRemasteredAssets
                    EgsHdAsset asset = new EgsHdAsset(header, decompressedData, encryptedData, encryptionSeed);
                    ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
                }
                else
                {
                    // Make sure to write the original file after remastered assets headers
                    pkgStream.Write(encryptedData);
                }
            }
            else if (File.Exists(completeRawFilePath))
            {
                var newFileStream = File.ReadAllBytes(completeRawFilePath);
                actualLength = BitConverter.ToInt32(newFileStream, 0);

                pkgStream.Write(newFileStream);
            }

            #endregion

            // Write a new entry in the HED stream
            var hedHeader = new Hed.Entry()
            {
                MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
                ActualLength = actualLength,
                DataLength = (int)(pkgStream.Position - offset),
                Offset = offset
            };

            //if (!Names.TryGetValue(Helpers.ToString(hedHeader.MD5), out var existingfilename))
            //{
            //    File.AppendAllText("resources/custom_filenames.txt", filename + "\n");
            //}

            BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

            return hedHeader;
        }

        public static Hed.Entry ReplaceFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, EgsHdAsset asset, Hed.Entry originalHedHeader = null)
        {
            var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
            var completeRawFilePath = Path.Combine(inputFolder, RAW_FILES_FOLDER_NAME, filename);

            var offset = pkgStream.Position;
            var originalHeader = asset.OriginalAssetHeader;

            // Clone the original asset header
            var header = new EgsHdAsset.Header()
            {
                CompressedLength = originalHeader.CompressedLength,
                DecompressedLength = originalHeader.DecompressedLength,
                RemasteredAssetCount = originalHeader.RemasteredAssetCount,
                CreationDate = originalHeader.CreationDate
            };

            // Use the base original asset data by default
            var decompressedData = asset.OriginalData;
            var encryptedData = asset.OriginalRawData;
            var encryptionSeed = asset.Seed;
            int actualLength = 0;

            SDasset sdasset = null;
            // We want to replace the original file

            if (File.Exists(completeFilePath))
            {
                bool RemasterExist = false;

                Console.WriteLine($"Replacing original: {filename}!");
                string RemasteredPath = completeFilePath.Replace("\\original\\","\\remastered\\");
                if (Directory.Exists(RemasteredPath))
                {
                    //Console.WriteLine($"Remastered Folder Exists! Path: {RemasteredPath}");
                    RemasterExist = true;
                }

                using var newFileStream = File.OpenRead(completeFilePath);
                decompressedData = newFileStream.ReadAllBytes();

                sdasset = new SDasset(filename, decompressedData, RemasterExist, false);

                if (sdasset != null && !sdasset.Invalid)
                    header.RemasteredAssetCount = sdasset.TextureCount;

                var compressedData = decompressedData.ToArray();
                var compressedDataLenght = originalHeader.CompressedLength;

                // CompressedLenght => -2: no compression and encryption, -1: no compression 
                if (originalHeader.CompressedLength > -1)
                {
                    compressedData = Helpers.CompressData(decompressedData);
                    compressedDataLenght = compressedData.Length;
                }

                header.CompressedLength = compressedDataLenght;
                header.DecompressedLength = decompressedData.Length;
                // Encrypt and write current file data in the PKG stream

                // The seed used for encryption is the original data header
                var seed = new MemoryStream();
                BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                encryptionSeed = seed.ReadAllBytes();
                encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;
            }

            if (File.Exists(completeRawFilePath))
            {
                var rawFileStream = File.ReadAllBytes(completeRawFilePath);
                actualLength = BitConverter.ToInt32(rawFileStream, 0);

                pkgStream.Write(rawFileStream);
            }
            else
            {
                // Write original file header
                BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                var remasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();

                // Is there remastered assets?
                if (header.RemasteredAssetCount > 0)
                {
                    remasteredHeaders = ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
                }
                else
                {
                    // Make sure to write the original file after remastered assets headers
                    pkgStream.Write(encryptedData);
                }
                actualLength = decompressedData.Length;
            }

            // Write a new entry in the HED stream
            var hedHeader = new Hed.Entry()
            {
                MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
                ActualLength = actualLength,
                DataLength = (int)(pkgStream.Position - offset),
                Offset = offset
            };

            // For unknown reason, some files have a data length of 0
            if (originalHedHeader.DataLength == 0)
            {
                Console.WriteLine($"{filename} => {originalHedHeader.ActualLength} ({originalHedHeader.DataLength})");

                hedHeader.ActualLength = originalHedHeader.ActualLength;
                hedHeader.DataLength = originalHedHeader.DataLength;
            }

            BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

            return hedHeader;
        }

        private static List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string inputFolder, string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData, SDasset sdasset)
        {
            var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
            var oldRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
            var relativePath = Helpers.GetRelativePath(originalFile, Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME));
            var remasteredAssetsFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);

            var allRemasteredAssetsData = new MemoryStream();

            foreach (var remasteredAssetHeader in asset.RemasteredAssetHeaders.Values)
            {
                oldRemasteredHeaders.Add(remasteredAssetHeader);
            }

            //At the moment this only applies on fresh PKGs (or ones that haven't been patched with this modded MDLX before, otherwise we'd neet to analyse ALL MDLX files)
            if (sdasset != null && !sdasset.Invalid)
            {
                //File.AppendAllText("custom_hd_assets.txt", "HD assets for: " + originalFile + "\n");
                while (oldRemasteredHeaders.Count > sdasset.TextureCount)
                {
                    //File.AppendAllText("custom_hd_assets.txt", "Removing: -" + (oldRemasteredHeaders.Count - 1) + ".dds\n");
                    oldRemasteredHeaders.RemoveAt(oldRemasteredHeaders.Count - 1);
                }
                while (oldRemasteredHeaders.Count < sdasset.TextureCount)
                {
                    var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                    {
                        CompressedLength = 0,
                        DecompressedLength = 0,
                        Name = "-" + oldRemasteredHeaders.Count + ".dds",
                        Offset = 0,
                        OriginalAssetOffset = 0
                    };
                    //File.AppendAllText("custom_hd_assets.txt", "Adding: -" + oldRemasteredHeaders.Count + ".dds\n");
                    oldRemasteredHeaders.Add(newRemasteredAssetHeader);
                }
                //File.AppendAllText("custom_hd_assets.txt", "\n");
            }

            // 0x30 is the size of this header
            var totalRemasteredAssetHeadersSize = oldRemasteredHeaders.Count() * 0x30;
            // This offset is relative to the original asset data
            var offset = totalRemasteredAssetHeadersSize + 0x10 + asset.OriginalAssetHeader.DecompressedLength;
            List<string> remasteredNames = new List<string>();

            if (asset.RemasteredAssetHeaders.Values.Count == 0 || offset != asset.RemasteredAssetHeaders.Values.First().Offset)
                remasteredNames.Clear();

            //grab list of full file paths from current remasteredAssetsFolder path and add them to a list.
            //we use this list later to correctly add the file names to the PKG.
            if (Directory.Exists(remasteredAssetsFolder) && Directory.GetFiles(remasteredAssetsFolder, "*", SearchOption.AllDirectories).Length > 0) //only do this if there are actually files in it.
            {
                remasteredNames.AddRange(Directory.GetFiles(remasteredAssetsFolder, "*", SearchOption.AllDirectories).ToList());

                for (int l = 0; l < remasteredNames.Count; l++) //fix names
                {
                    remasteredNames[l] = remasteredNames[l].Replace(remasteredAssetsFolder, "").Replace(@"\", "/");
                    //this check is needed else it adds a full stop for files without extensions.
                    if (Path.GetExtension(remasteredNames[l]) != "")
                        remasteredNames[l] = Path.ChangeExtension(remasteredNames[l], Path.GetExtension(remasteredNames[l]).ToLower());
                }

                if (remasteredNames.Contains("/-10.dds") || remasteredNames.Contains("/-10.png"))
                {
                    //Make a sorted list tempremasteredNames
                    List<string> tempremasteredNamesD = new List<string>();
                    List<string> tempremasteredNamesP = new List<string>();
                    List<string> tempremasteredNames = new List<string>(remasteredNames);
                    for (int i = 0; i < remasteredNames.Count; i++)
                    {
                        var filename = "/-"  + i.ToString();
                        if (remasteredNames.Contains(filename + ".dds"))
                        {
                            //Console.WriteLine(filename + ".dds" + "FOUND!");
                            tempremasteredNamesD.Add(filename + ".dds");
                            tempremasteredNames.Remove(filename + ".dds");
                        }
                        else if (remasteredNames.Contains(filename + ".png"))
                        {
                            //Console.WriteLine(filename + ".png" + "FOUND!");
                            tempremasteredNamesP.Add(filename + ".png");
                            tempremasteredNames.Remove(filename + ".png");
                        }
                    }
                    //Add the image files at the end
                    //DDS list first, PNG list 2nd, everything else after
                    tempremasteredNamesD.AddRange(tempremasteredNamesP);
                    tempremasteredNamesD.AddRange(tempremasteredNames);
                    //Add the sorted list back to remasteredNames
                    remasteredNames = tempremasteredNamesD;
                }
            }

            for (int i = 0; i < oldRemasteredHeaders.Count; i++)
            {
                var remasteredAssetHeader = oldRemasteredHeaders[i];
                var filename = remasteredAssetHeader.Name;
                var assetFilePath = Path.Combine(remasteredAssetsFolder, filename);

                //get actual file names ONLY if the remastered asset count is greater than 0 and ONLY if the number of files in the 
                //remastered folder for the SD asset is equal to or greater than what the total count is from what was gotten in SDasset.
                //if those criteria aren't met then do the old method.
                if (sdasset != null && !sdasset.Invalid && remasteredNames.Count >= oldRemasteredHeaders.Count && remasteredNames.Count > 0)
                {
                    //filename = remasteredNames[i].Replace((remasteredAssetsFolder), "").Remove(0, 1);
                    filename = remasteredNames[i].Remove(0, 1);
                    assetFilePath = Path.Combine(remasteredAssetsFolder, filename);
                }

                // Use base remastered asset data
                var assetData = asset.RemasteredAssetsDecompressedData.ContainsKey(filename) ? asset.RemasteredAssetsDecompressedData[filename] : new byte[] { };
                var decompressedLength = remasteredAssetHeader.DecompressedLength;
                var originalAssetOffset = remasteredAssetHeader.OriginalAssetOffset;
                if (File.Exists(assetFilePath))
                {
                    Console.WriteLine($"Replacing remastered file: {relativePath}/{filename}");

                    assetData = File.ReadAllBytes(assetFilePath);
                    decompressedLength = assetData.Length;
                    assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                    assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                    if (sdasset != null && !sdasset.Invalid)
                        originalAssetOffset = sdasset.Offsets[i];
                }
                else
                {
                    if (Directory.Exists(relativePath))
                        Console.WriteLine($"Keeping remastered file: {relativePath}/{filename}");
                    // The original file have been replaced, we need to encrypt all remastered asset with the new key
                    if (!seed.SequenceEqual(asset.Seed))
                    {
                        assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                        assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                        if (sdasset != null && !sdasset.Invalid && sdasset.TextureCount >= i)
                            originalAssetOffset = sdasset.Offsets[i];
                    }
                    else
                    {
                        assetData = asset.RemasteredAssetsCompressedData.ContainsKey(filename) ? asset.RemasteredAssetsCompressedData[filename] : new byte[] { };
                    }
                }
                var compressedLength = remasteredAssetHeader.CompressedLength > -1 ? assetData.Length : remasteredAssetHeader.CompressedLength;

                var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                {
                    CompressedLength = compressedLength,
                    DecompressedLength = decompressedLength,
                    Name = filename,
                    Offset = offset,
                    OriginalAssetOffset = originalAssetOffset
                };

                newRemasteredHeaders.Add(newRemasteredAssetHeader);

                // Write asset header in the PKG stream
                BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, newRemasteredAssetHeader);

                // Don't write into the PKG stream yet as we need to write
                // all HD assets header juste after original file's data
                allRemasteredAssetsData.Write(assetData);

                // Make sure to align remastered asset data on 16 bytes
                if (assetData.Length % 0x10 != 0)
                {
                    allRemasteredAssetsData.Write(Enumerable.Repeat((byte)0xCD, 16 - (assetData.Length % 0x10)).ToArray());
                }

                offset += decompressedLength;
            }

            pkgStream.Write(originalAssetData);
            pkgStream.Write(allRemasteredAssetsData.ReadAllBytes());

            return newRemasteredHeaders;
        }

        #endregion

        #region List

        public static void List(string inputHed)
        {
            using var hedStream = File.OpenRead(inputHed);
            var entries = Hed.Read(hedStream);

            foreach (var entry in entries)
            {
                var hash = Helpers.ToString(entry.MD5);
                if (!Names.TryGetValue(hash, out var fileName))
                    fileName = $"{hash}.dat";

                //Console.WriteLine(fileName);
            }
        }

        #endregion
    }

    public class SDasset
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = true;
        public static bool ScanMode = false;

        public SDasset(string name, byte[] originalAssetData, bool remasterpathtrue, bool scanmode)
        {
            dynamic asset = null;
            ScanMode = scanmode;

            switch (Path.GetExtension(name), remasterpathtrue)
            {
                case (".2dd", true):
                case (".2ld", true):
                case (".bar", true):
                case (".bin", true):
                case (".mag", true):
                case (".map", true):
                case (".mdlx", true):
                    asset = new BAR(originalAssetData);
                    break;
                case (".imd", true):
                    asset = new IMD(originalAssetData, 0);
                    break;
                case (".imz", true):
                    asset = new IMZ(originalAssetData, 0);
                    break;
                case (".pax", true):
                    asset = new PAX(originalAssetData, 0);
                    break;
                case (".tm2", true):
                    asset = new TM2(originalAssetData, 0);
                    break;
                //case (".dpd", true): //Special file, fix later
                    //asset = new DPD(originalAssetData);
                    //break;
            }
            switch (".a" + (Path.GetExtension(name)), remasterpathtrue)
            {
                case (".a.fm", true):
                case (".a.fr", true):
                case (".a.gr", true):
                case (".a.it", true):
                case (".a.sp", true):
                case (".a.us", true):
                case (".a.uk", true):
                case (".a.jp", true):
                    asset = new BAR(originalAssetData);
                    break;
            }

            if (asset != null && !asset.Invalid)
            {
                Offsets = asset.Offsets;
                TextureCount = asset.TextureCount;
                Invalid = false;
                Console.WriteLine("File: " + name + " | Asset Count: " + TextureCount + "\n");
            }
        }
    }

    class IMD
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public IMD(byte[] AssetData, int AssetOffset)
        {
            using MemoryStream ms = new MemoryStream(AssetData);

            int magic = ms.ReadInt32();
            if (magic != 1145523529 && AssetOffset == 0) //IMGD
            {
                Invalid = true;
                Helpers.ScanPrint("IMD texture could not be scanned! Wrong filetype?");
                return;
            }

            TextureCount = 1; //IMDs are always single images
            ms.ReadInt32(); //always 256(?)
            int Imageoffset = ms.ReadInt32(); //offset for image data
            Offsets.Add(AssetOffset + Imageoffset + 0x20000000);

            if (AssetOffset == 0)
                Helpers.ScanPrint($"IMD texture found! | Suggested HD Texture name: -0.dds");
        }
    }

    class IMZ
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public IMZ(byte[] AssetData, int AssetOffset)
        {
            using MemoryStream ms = new MemoryStream(AssetData);

            
            int magic = ms.ReadInt32();
            if (magic != 1514622281 && AssetOffset == 0)
            { //IMGZ
                Invalid = true;
                Helpers.ScanPrint("IMZ could not be scanned! Wrong filetype?");
                return;
            }
            ms.ReadInt64(); //unknown
            int TexCount = ms.ReadInt32(); //texture count

            for (int i = 0; i < TexCount; i++) 
            {
                ms.Seek(0x10 + (i * 0x8), SeekOrigin.Begin);
                int IMDoffset = ms.ReadInt32(); //Offset for IMGD data
                ms.Seek(IMDoffset, SeekOrigin.Begin);

                magic = ms.ReadInt32();
                //Console.WriteLine(magic);
                if (magic == 1145523529) //IMGD
                {
                    TextureCount += 1;
                    ms.ReadInt32(); //always 256
                    int Imageoffset = ms.ReadInt32(); //offset for image data
                    Offsets.Add(AssetOffset + IMDoffset + Imageoffset + 0x20000000);
                }

            }

            if (AssetOffset == 0)
            {
                for (int i = 0; i < Offsets.Count; i++)
                    Helpers.ScanPrint($"IMZ texture found! | Suggested HD Texture name: -{i}.dds");
            }
        }
    }

    class PAX
    {
        public List<int> Offsets = new List<int>();
        SortedDictionary<int, int> TempOffsets = new SortedDictionary<int, int>();
        SortedDictionary<int, Tuple<string, int>> TempScanPAX = new SortedDictionary<int, Tuple<string, int>>();
        public Dictionary<string, int> ScanPAX = new Dictionary<string, int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        //PAX Textures are a bit weird to link to their remastered counterparts.
        //Currently all offsets seem to be gotten correctly, but the order of them doesn't
        //always match how the devs seemed to have them ordered in the remastered folder.
        //If adding a new PAX or file with a PAX the user will usually have to manually re-order their remastered
        //textures to link up correctly by renaming them.

        public PAX(byte[] AssetData, int AssetOffset)
        {
            using MemoryStream ms = new MemoryStream(AssetData);
            //ScanPAX.Clear();

            var magic = ms.ReadInt32();
            if (magic != 1599619408 && AssetOffset == 0) //PAX_
            {
                Invalid = true;
                Helpers.ScanPrint("PAX could not be scanned! Wrong filetype?");
                return;
            }
            ms.ReadInt64(); //we just skip these 8 bytes. unsure what they are for.

            var Dpxoffset = ms.ReadInt32();
            ms.Seek(Dpxoffset + 0xC, SeekOrigin.Begin);

            var Unk1Count = ms.ReadInt32(); //unsure what this block of data is for. we seem to not need it though.
            ms.Seek(Unk1Count * 0x20, SeekOrigin.Current); //so skip it to get to the part we actually need.

            var DpdCount = ms.ReadInt32();
            var DpdOffsets = ((int)ms.Position); //the DPDs are what have our textures so save the position of this area.

            for (int d = 0; d < DpdCount; d++)
            {
                ms.Seek(DpdOffsets + (d * 0x4), SeekOrigin.Begin);

                var DpdOffset = ms.ReadInt32();
                ms.Seek(Dpxoffset + DpdOffset, SeekOrigin.Begin);

                ms.ReadInt32(); //unknown

                var Unk2Count = ms.ReadInt32(); //don't know this block of data, so skip it to get to what me need
                ms.Seek(Unk2Count * 0x4, SeekOrigin.Current);

                var DpdTexCount = ms.ReadInt32(); //finally found the texture offsets
                var DpdTexOffsets = ((int)ms.Position); //save this position

                for (int t = 0; t < DpdTexCount; t++)
                {
                    //Console.WriteLine("new PAX texture found");

                    ms.Seek(DpdTexOffsets + (t * 0x4), SeekOrigin.Begin);
                    var DpdTexOffset = ms.ReadInt32();
                    ms.Seek(Dpxoffset + DpdOffset + DpdTexOffset, SeekOrigin.Begin);
                    int value1 = ms.ReadInt32(); //use this as a key in  the dictionary
                    ms.ReadInt32();
                    int value2 = ms.ReadInt32(); //this value seems to define if a texture is new

                    if (value2 == 0)
                    {
                        //Console.WriteLine("new texture found");

                        TextureCount += 1;
                        int finaloffset = AssetOffset + Dpxoffset + DpdOffset + (DpdTexOffset + 0x20) + 0x20000000;
                        var ScanTuple = new Tuple<string, int>($"PAX texture was found in DPD {d} as image {t}!", value2);

                        //check to see if our key already exists
                        if (!TempOffsets.ContainsKey(value1))
                        {
                            //if it doesn't then add it as normal
                            TempOffsets.Add(value1, finaloffset);
                            TempScanPAX.Add(value1, ScanTuple);
                        }
                        else
                        {
                            //if it does then we need to increase the offset by 1 for the original value
                            TempOffsets[value1] += 1;
                            //then use that new value + 1 as our new offset for the duplicate key then add t to our key so that it can actually be added.
                            TempOffsets.Add(value1 + t, (TempOffsets[value1] + 1));
                            TempScanPAX.Add(value1 + t, ScanTuple);
                        }
                    }
                    else
                    {
                        var ScanTuple = new Tuple<string, int>($"PAX \"combo\" texture was found in DPD {d} as image {t}!", value2);
                        //int prevValue = TempOffsets[TempOffsets.Count - 1];
                        TempScanPAX.Add(value1, ScanTuple);
                        //ScanPAX.Add($"PAX \"combo\" texture was found in DPD {d} as image {t}!", value2);
                    }
                }
                //Add our current list of offsets from the dpd to our main ffsets list
                Offsets.AddRange(TempOffsets.Values);
                foreach (Tuple<string,int> PT in TempScanPAX.Values)
                    ScanPAX.Add(PT.Item1, PT.Item2);

                //then clear the temp list for the next dpd
                TempOffsets.Clear();
                TempScanPAX.Clear();
            }

            if (AssetOffset == 0)
            {
                int imageNum = 0;
                for (int a = 0; a < ScanPAX.Count; a++)
                {
                    if (ScanPAX.ElementAt(a).Value == 0)
                    {
                        Helpers.ScanPrint(ScanPAX.ElementAt(a).Key + $" | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                    else
                    {
                        Helpers.ScanPrint(ScanPAX.ElementAt(a).Key + $" Combine it with HD Texture -{imageNum - 1}.dds for proper HD linking.");
                    }

                }

            }
        }
    }

    class BAR
    {
        public List<int> Offsets = new List<int>();
        List<int> OffsetsTIM = new List<int>();
        List<int> OffsetsPAX = new List<int>();
        List<int> OffsetsTM2 = new List<int>();
        List<int> OffsetsIMD = new List<int>();
        List<int> OffsetsIMZ = new List<int>();
        List<int> OffsetsRAW = new List<int>();
        List<int> OffsetsAudio = new List<int>();
        public List<Tuple<string, int>> NamesAudio = new List<Tuple<string, int>>();
        Dictionary<string, int> ScanPAX = new Dictionary<string, int>();

        public int TextureCount = 0;
        public bool Invalid = false;
        
        public BAR(byte[] originalAssetData)
        {
            dynamic subasset;

            using MemoryStream ms = new MemoryStream(originalAssetData);

            int type;
            int offset;
            int subsize;
            string magic;
            byte[] subfile;

            magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
            if (magic != "BAR") //BAR
            {
                Invalid = true;
                Helpers.ScanPrint("BAR could not be scanned! Wrong filetype?");
                Helpers.ScanPrint("Valid BAR file types are: bar, bin, mag, map, mdlx, 2dd, and 2ld.");
                return;
            }
            ms.ReadBytes(1);

            int count = ms.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ms.Seek(0x10 + (i * 0x10), SeekOrigin.Begin);

                type = ms.ReadInt32(); //subasset type
                //int subname = ms.ReadInt32(); //subasset name
                ms.ReadInt32(); //subasset name
                offset = ms.ReadInt32(); //subasset offset
                subsize = ms.ReadInt32(); //subasset size

                ms.Seek(offset, SeekOrigin.Begin);

                //Console.WriteLine("Type is - " + type);
                switch (type)
                {
                    case (7): //RAW Image
                        int rawmagic = ms.ReadInt32();
                        if (rawmagic == 0)
                        {
                            //Console.WriteLine("RAW image!");
                            ms.Seek(offset, SeekOrigin.Begin);
                            subfile = ms.ReadBytes(subsize);
                            subasset = new RAW(subfile, offset);

                            TextureCount += subasset.TextureCount;
                            OffsetsTIM.AddRange(subasset.Offsets);
                        }
                        else
                            Helpers.ScanPrint("RAW subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                    case (10): //TIM2
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                        if (magic == "TIM2")
                        {
                            //Console.WriteLine("TIM2 Image!");
                            ms.Seek(offset, SeekOrigin.Begin);
                            subfile = ms.ReadBytes(subsize);
                            subasset = new TM2(subfile, offset);

                            TextureCount += subasset.TextureCount;
                            OffsetsTM2.AddRange(subasset.Offsets);
                        }
                        else
                            Helpers.ScanPrint("TM2 subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                    case (18): //PAX
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
                        if (magic == "PAX") //PAX
                        {
                            //Console.WriteLine("PAX archive!");
                            ms.Seek(offset, SeekOrigin.Begin);
                            subfile = ms.ReadBytes(subsize);
                            subasset = new PAX(subfile, offset);

                            TextureCount += subasset.TextureCount;
                            OffsetsPAX.AddRange(subasset.Offsets);
                            ScanPAX = subasset.ScanPAX;
                        }
                        else
                            Helpers.ScanPrint("PAX subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                    case (24): //IMD
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                        if (magic == "IMGD") //IMGD
                        {
                            //Console.WriteLine("Image!");
                            ms.Seek(offset, SeekOrigin.Begin);
                            subfile = ms.ReadBytes(subsize);
                            subasset = new IMD(subfile, offset);

                            TextureCount += subasset.TextureCount;
                            OffsetsIMD.AddRange(subasset.Offsets);
                        }
                        else
                            Helpers.ScanPrint("IMD subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                    case (29): //IMZ                           
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                        if (magic == "IMGZ")//IMGZ
                        {
                            //Console.WriteLine("Image Collection!");
                            ms.Seek(offset, SeekOrigin.Begin);
                            subfile = ms.ReadBytes(subsize);
                            subasset = new IMZ(subfile, offset);

                            TextureCount += subasset.TextureCount;
                            OffsetsIMZ.AddRange(subasset.Offsets);
                        }
                        else
                            Helpers.ScanPrint("IMZ subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                    case (31): //Sound Effects
                    case (34): //Voice Audio
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(6));
                        if (magic == "ORIGIN")
                        {
                            //Console.WriteLine("Audio file!");

                            ms.ReadBytes(6);
                            short audioID = ms.ReadInt16();
                            ms.ReadInt16();
                            string name = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(32)).TrimEnd('\0');

                            TextureCount += 1;
                            OffsetsAudio.Add(-1);

                            var ScanTuple = new Tuple<string, int>(name, audioID);
                            NamesAudio.Add(ScanTuple);
                        }
                        else
                            Helpers.ScanPrint("Audio subtype found in BAR, but could not be scanned! Is this subtype correct?\nThe HD port uses a custom method for loading audio. Make sure your file was correctly made to support it.");
                        break;
                    case (36): //raw bitmap
                        //no magic for these. we just hope that any instance of this is actually a bitmap
                        {
                            //Console.WriteLine("Bitmap image!);

                            TextureCount += 1;
                            OffsetsRAW.Add(offset + 0x20000000);
                        }
                        break;
                    case (46): //BAR
                        magic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(3));
                        if (magic == "BAR")
                        {
                            ms.ReadBytes(1);
                            int subcount = ms.ReadInt32();
                            ms.ReadInt64();
                            var posOffset = (int)ms.Position;

                            for (int s = 0; s < subcount; s++)
                            {
                                ms.Seek(posOffset + (s * 0x10), SeekOrigin.Begin);
                                int subtype = ms.ReadInt32(); //subasset type
                                ms.ReadInt32(); //subasset name
                                int suboffset = ms.ReadInt32(); //subasset offset
                                subsize = ms.ReadInt32(); //subasset size
                                ms.Seek((posOffset - 0x10) + suboffset, SeekOrigin.Begin);
                                string subMagic;

                                switch (subtype)
                                {
                                    case (24): //IMD
                                        subMagic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                                        if (subMagic == "IMGD") //IMGD
                                        {
                                            //Console.WriteLine("BAR-ception Image!");
                                            ms.Seek((posOffset - 0x10) + suboffset, SeekOrigin.Begin);
                                            subfile = ms.ReadBytes(subsize);
                                            subasset = new IMD(subfile, offset + suboffset);

                                            TextureCount += subasset.TextureCount;
                                            OffsetsIMD.AddRange(subasset.Offsets);
                                        }
                                        break;
                                    case (29): //IMZ                           
                                        subMagic = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                                        if (subMagic == "IMGZ")//IMGZ
                                        {
                                            //Console.WriteLine("BAR-ception Image Collection!");
                                            ms.Seek((posOffset - 0x10) + suboffset, SeekOrigin.Begin);
                                            subfile = ms.ReadBytes(subsize);
                                            subasset = new IMZ(subfile, offset + suboffset);

                                            TextureCount += subasset.TextureCount;
                                            OffsetsIMZ.AddRange(subasset.Offsets);
                                        }
                                        break;
                                }
                            }
                        }
                        else
                            Helpers.ScanPrint("BAR subtype found in BAR, but could not be scanned! Is this subtype correct?");
                        break;
                }
            }

            //mostly needed for maps, though maybe other files need this sorting too
            Offsets.AddRange(OffsetsTIM);
            Offsets.AddRange(OffsetsPAX);
            Offsets.AddRange(OffsetsTM2);
            Offsets.AddRange(OffsetsIMD);
            Offsets.AddRange(OffsetsIMZ);
            Offsets.AddRange(OffsetsRAW);
            Offsets.AddRange(OffsetsAudio);

            if (TextureCount > 0 && SDasset.ScanMode)
            {
                int imageNum = 0;

                if (OffsetsTIM.Count > 0)
                {
                    for (int a = 0; a < OffsetsTIM.Count; a++)
                    {
                        Helpers.ScanPrint($"TIM texture was found! | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                }

                if (OffsetsPAX.Count > 0)
                {
                    for (int a = 0; a < ScanPAX.Count; a++)
                    {
                        if (ScanPAX.ElementAt(a).Value == 0)
                        {
                            Helpers.ScanPrint(ScanPAX.ElementAt(a).Key + $" | Aprox. HD Texture name: -{imageNum}.dds");
                            imageNum += 1;
                        }
                        else
                        {
                            Helpers.ScanPrint(ScanPAX.ElementAt(a).Key + $" Combine it with HD Texture -{imageNum - 1}.dds for proper HD linking.");
                            //Console.WriteLine("Combine it with HD Texture -" + (imageNum - 1) + ".dds for proper HD linking.");
                        }

                    }
                }

                if (OffsetsTM2.Count > 0)
                {
                    for (int a = 0; a < OffsetsTM2.Count; a++)
                    {
                        Helpers.ScanPrint($"TM2 texture was found! | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                }

                if (OffsetsIMD.Count > 0)
                {
                    for (int a = 0; a < OffsetsIMD.Count; a++)
                    {
                        Helpers.ScanPrint($"IMD texture was found! | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                }

                if (OffsetsIMZ.Count > 0)
                {
                    for (int a = 0; a < OffsetsIMZ.Count; a++)
                    {
                        Helpers.ScanPrint($"IMZ texture was found! | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                }

                if (OffsetsRAW.Count > 0)
                {
                    for (int a = 0; a < OffsetsRAW.Count; a++)
                    {
                        Helpers.ScanPrint($"Bitmap texture was found! | Aprox. HD Texture name: -{imageNum}.dds");
                        imageNum += 1;
                    }
                }

                if (OffsetsAudio.Count > 0)
                {
                    for (int a = 0; a < OffsetsAudio.Count; a++)
                    {
                        Helpers.ScanPrint($"Audio asset was found! | Audio ID: {NamesAudio.ElementAt(a).Item2} | HD Audio name: {NamesAudio.ElementAt(a).Item1}");
                    }
                }

            }

            if (TextureCount == 0)
            {
                //Console.WriteLine("BAR doesn't contain hd assets.");
                Invalid = true;
                return;
            }
        }
    }

    class RAW
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public RAW(byte[] AssetData, int AssetOffset)
        {
            using MemoryStream ms = new MemoryStream(AssetData);

            int magic = ms.ReadInt32();
            if (magic != 0 && AssetOffset == 0) //0x00000000
            {
                Invalid = true;
                Helpers.ScanPrint("RAW texture could not be scanned! Wrong filetype?");
                return;
            }

            ms.ReadInt32(); // color count
            int TextureInfoCount = ms.ReadInt32();
            int GSInfoCount = ms.ReadInt32();
            int OffsetDataOff = ms.ReadInt32();
            int CLUTTransinfoOff = ms.ReadInt32();
            int GsinfoOff = ms.ReadInt32();
            int dataOffset = ms.ReadInt32();

            TextureCount += GSInfoCount;

            //get all the image data offsets from the CLUT Transfer Info blocks
            Dictionary<int, int> PicOffsets = new Dictionary<int, int>();
            for (int t = 0; t < TextureInfoCount; t++)
            {
                ms.Seek((CLUTTransinfoOff + 0x90) + (t * 0x90) + 116, SeekOrigin.Begin);
                PicOffsets.Add(t, ms.ReadInt32());
            }

            //first loop to get number of pixel format 8 textures
            //this is needed to calculate the correct HD link offsets because for some reason
            //Pixel format 4 textures need to be adjusted by Pixel8 image count * 16
            int Modifier = 0;
            for (int m = 0; m < GSInfoCount; m++)
            {
                ms.Seek(GsinfoOff + 0x70 + (m * 0xA0), SeekOrigin.Begin);
                long Tex0Reg = ms.ReadInt64();
                uint PSM = (uint)(Tex0Reg >> 20) & 0x3fu;
                if (PSM != 20)
                    Modifier += 1;
            }

            //second loop to get actual offsets
            //We need to keep track of how many of each type of texture we find
            //to correctly calculate what the game expects for the HD link offsets.
            int Pxl4Count = 0;
            int Pxl8Count = 0;
            for (int p = 0; p < GSInfoCount; p++)
            {
                ms.Seek(OffsetDataOff + p, SeekOrigin.Begin);
                int CurrentKey = ms.ReadByte();

                ms.Seek(GsinfoOff + 0x70 + (p * 0xA0), SeekOrigin.Begin);
                long Tex0Reg = ms.ReadInt64();
                uint PSM = (uint)(Tex0Reg >> 20) & 0x3fu;

                int FinalOffset = AssetOffset + PicOffsets[CurrentKey] + 0x20000000;
                if (PSM == 20)
                {
                    Offsets.Add(FinalOffset + Pxl4Count + (Modifier * 0x10));
                    Pxl4Count += 1;
                }
                else
                {
                    Offsets.Add(FinalOffset + (Pxl8Count * 0x10));
                    Pxl8Count += 1;
                }
            }

            int index = Helpers.IndexOfByteArray(AssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), 0);
            while (index > -1)
            {
                ms.Seek(index + 0x0a, SeekOrigin.Begin);
                int imageToApplyTo = (int)ms.ReadInt16();
            
                ms.Seek(0x1c, SeekOrigin.Current);
                int texaOffset = ms.ReadInt32();
                int offset = index + texaOffset + 0x08 + (imageToApplyTo * 0x10) + 0x20000000;
                Offsets.Add(AssetOffset + offset);
            
                TextureCount++;
                index = Helpers.IndexOfByteArray(AssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), index + 1);
            }

            if (AssetOffset == 0)
            {
                for (int i = 0; i < Offsets.Count; i++)
                    Helpers.ScanPrint($"RAW texture found! | Suggested HD Texture name: -{i}.dds");
            }
        }
    }

    class TM2
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public TM2(byte[] AssetData, int AssetOffset)
        {
            using MemoryStream ms = new MemoryStream(AssetData);

            int magic = ms.ReadInt32();
            if (magic != 843925844 && AssetOffset == 0) //TIM2
            {
                Invalid = true;
                Helpers.ScanPrint("TM2 texture could not be scanned! Wrong filetype?");
                return;
            }

            ms.ReadInt16(); //format
            int texCount = ms.ReadInt16();
            ms.ReadInt64(); //unused
            int totalsize = 0;

            for (int i = 0; i < texCount; i++)
            {
                ms.Seek(0x10 + totalsize, SeekOrigin.Begin);
                totalsize += ms.ReadInt32();

                if (i == 0 && totalsize == 0 && texCount > 1)
                {
                    Invalid = true;
                    Helpers.ScanPrint("TM2 texture could not be scanned! Wrong filetype?");
                    return;
                }

                ms.ReadInt32(); //Clut size
                ms.ReadInt32(); //Image size
                int header = ms.ReadInt16(); //header size
                ms.Seek((header - 0x10) + 0x2, SeekOrigin.Current);
                int imageOffset = ((int)ms.Position);

                TextureCount += 1;
                Offsets.Add(AssetOffset + imageOffset + 0x20000000);
            }

            if (AssetOffset == 0)
            {
                for (int i = 0; i < Offsets.Count; i++)
                    Helpers.ScanPrint($"TM2 texture found! | Suggested HD Texture name: -{i}.dds");
            }
        }
    }
 
}
