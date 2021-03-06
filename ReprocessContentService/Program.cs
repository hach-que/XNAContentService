﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using XNAContentCompiler;

namespace ReprocessContentService
{
    class Program
    {
        private const string SOURCE_DIR = "source";
        private const string COMPILED_DIR = "compiled";
        private const string CONTENT_DIR = COMPILED_DIR + "/Content";

        private static bool USE_MONO = true;

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Not enough arguments, expected 1 (name).");
                Environment.Exit(1);
            }

            var name = args[0];
            if (args.Length == 2 && args[1].ToLowerInvariant() == "false")
                USE_MONO = false;

            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR)))
            {
                Console.Error.WriteLine("Source directory does not exist at " + Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR));
                Environment.Exit(1);
            }
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory + "/", COMPILED_DIR)))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory + "/", COMPILED_DIR));
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory + "/", CONTENT_DIR)))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory + "/", CONTENT_DIR));

            // Recursively process directories.
            var resultFiles = new List<string>();
            RecursivelyProcessDirectories(
                resultFiles,
                Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR),
                Path.Combine(Environment.CurrentDirectory + "/", CONTENT_DIR)
                );

            // Generate the project file.
            using (var writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory + "/", COMPILED_DIR + "/", name + ".mdproj")))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true
                };
                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {
                    ProjectGenerator.Generate(xmlWriter, resultFiles);
                    Console.WriteLine("Generated project file " + name + ".mdproj");
                }
            }
        }

        private static string GetRelativeSourcePath(string path)
        {
            if (path.Length <= Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR).Length)
                return "(root)";
            return path.Substring(Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR).Length + 1);
        }

        private static string GetRelativeCompiledPath(string path)
        {
            return path.Substring(Path.Combine(Environment.CurrentDirectory + "/", COMPILED_DIR).Length + 1);
        }

        private static void RecursivelyProcessDirectories(List<string> resultFiles, string inputDirectory, string outputDirectory)
        {
            Console.WriteLine("Processing directory " + GetRelativeSourcePath(inputDirectory));

            // Create output directory if it doesn't exist.
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // Process any files inside this directory.
            resultFiles.AddRange(ProcessDirectory(inputDirectory, outputDirectory).Select(v => GetRelativeCompiledPath(Path.Combine(outputDirectory + "/", v))));

            // Reprocess any directories inside this one.
            foreach (var directory in new DirectoryInfo(inputDirectory).GetDirectories())
            {
                var fullOutputDirectory = Path.Combine(outputDirectory + "/", directory.Name);

                RecursivelyProcessDirectories(resultFiles, directory.FullName, fullOutputDirectory);
            }
        }

        private static List<string> ProcessDirectory(string inputDirectory, string outputDirectory)
        {
            var resultFiles = new List<string>();

            // We assume we're in the directory where all of the resources are.
            var contentBuilder = USE_MONO ? (ContentBuilder)new MonoGameContentBuilder() : new XnaContentBuilder();

            // Add all files.
            var extensions = new List<string>
            {
                "*.bmp",
                "*.jpg",
                "*.png",
                "*.tga",
                "*.dds",
                "*.wav",
                "*.mp3",
                "*.wma",
                "*.spritefont"
            };
            if (USE_MONO)
                extensions.Add("*.fx");
            foreach (var extension in extensions)
            {
                foreach (var file in new DirectoryInfo(inputDirectory).GetFiles(extension))
                {
                    var importer = contentBuilder.Importers.FindByName(Path.GetExtension(file.FullName).ToLower());
                    contentBuilder.Add(file.FullName, file.GetNameWithoutExtension(), importer.Value, importer.Other);
                }
            }

            // Build and check for error.
            var error = contentBuilder.Build();
            if (error != null)
            {
                Console.Error.WriteLine(error);
                Environment.Exit(1);
                return resultFiles;
            }

            // Delete all existing .xnb files in the output directory.
            foreach (var file in new DirectoryInfo(outputDirectory).GetFiles("*.xnb"))
            {
                try
                {
                    file.Delete();
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine("Unable to delete existing XNB output for " + file.Name);
                    Console.Error.WriteLine(ex.Message);
                }
            }

            // Copy all of the output files to the output directory.
            foreach (var file in new DirectoryInfo(contentBuilder.OutputDirectory).GetFiles("*.xnb"))
            {
                try
                {
                    file.CopyTo(Path.Combine(outputDirectory + "/", file.GetNameWithXNBExtension()));
                    resultFiles.Add(file.GetNameWithXNBExtension());
                    Console.WriteLine("-- " + file.GetNameWithXNBExtension());
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine("Unable to copy XNB output for " + file.GetNameWithXNBExtension());
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return resultFiles;
        }
    }

    public static class FileInfoExtensions
    {
        public static string GetNameWithoutExtension(this FileInfo file)
        {
            return file.Name.Substring(0, file.Name.Length - file.Extension.Length);
        }

        public static string GetNameWithXNBExtension(this FileInfo file)
        {
            return file.GetNameWithoutExtension() + ".xnb";
        }
    }
}
