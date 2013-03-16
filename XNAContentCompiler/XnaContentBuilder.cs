using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNAContentCompiler
{
    public class XnaContentBuilder : ContentBuilder
    {
        const string xnaVersion = ", Version=4.0.0.0, PublicKeyToken=842cf8be1de50553";

        protected override string[] PipelineAssemblies
        {
            get
            {
                return new string[]
                {
                    "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.XImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.AudioImporters" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.VideoImporters" + xnaVersion,

                    // If you want to use custom importers or processors from
                    // a Content Pipeline Extension Library, add them here.
                    //
                    // If your extension DLL is installed in the GAC, you should refer to it by assembly
                    // name, eg. "MyPipelineExtension, Version=1.0.0.0, PublicKeyToken=1234567812345678".
                    //
                    // If the extension DLL is not in the GAC, you should refer to it by
                    // file path, eg. "c:/MyProject/bin/MyPipelineExtension.dll".
                };
            }
        }

        protected override string[,] FileImporterProcessorMappings
        {
            get
            {
                return new string[,]
                {
                    {".mp3", "Mp3Importer", "SongProcessor"},
                    {".wav", "WavImporter", "SoundEffectProcessor"},
                    {".wma", "WmaImporter", "SongProcessor"},

                    {".bmp", "TextureImporter", "TextureProcessor"},
                    {".jpg", "TextureImporter", "TextureProcessor"},
                    {".png", "TextureImporter", "TextureProcessor"},
                    {".tga", "TextureImporter", "TextureProcessor"},
                    {".dds", "TextureImporter", "TextureProcessor"},

                    {".spritefont", "FontDescriptionImporter", "FontDescriptionProcessor"}
                };
            }
        }
    }
}
