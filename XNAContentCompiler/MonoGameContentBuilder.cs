﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace XNAContentCompiler
{
    public class MonoGameContentBuilder : ContentBuilder
    {
        const string xnaVersion = ", Version=4.0.0.0, PublicKeyToken=842cf8be1de50553";

        protected override string[] PipelineAssemblies
        {
            get
            {
                return new string[]
                {
                    Path.Combine(Environment.CurrentDirectory, "MonoGame\\MonoGameContentProcessors.dll"),
                    "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.XImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.AudioImporters" + xnaVersion,
                    "Microsoft.Xna.Framework.Content.Pipeline.VideoImporters" + xnaVersion,
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

                    {".spritefont", "FontDescriptionImporter", "FontDescriptionProcessor"},

                    {".fx", "EffectImporter", "MGEffectProcessor"}
                };
            }
        }
    }
}
