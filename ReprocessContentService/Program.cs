using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace ReprocessContentService
{
    class Program
    {
        private const string SOURCE_DIR = "source";
        private const string COMPILED_DIR = "compiled";
        private const string CONTENT_DIR = COMPILED_DIR + "/Content";

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Not enough arguments, expected 1 (name).");
                Environment.Exit(1);
            }

            var name = args[0];

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
            return path.Substring(Path.Combine(Environment.CurrentDirectory + "/", SOURCE_DIR).Length + 1);
        }

        private static string GetRelativeCompiledPath(string path)
        {
            return path.Substring(Path.Combine(Environment.CurrentDirectory + "/", COMPILED_DIR).Length + 1);
        }

        private static void RecursivelyProcessDirectories(List<string> resultFiles, string inputDirectory, string outputDirectory)
        {
            foreach (var directory in new DirectoryInfo(inputDirectory).GetDirectories())
            {
                var fullOutputDirectory = Path.Combine(outputDirectory + "/", directory.Name);

                Console.WriteLine("Processing directory " + GetRelativeSourcePath(directory.FullName));

                // Create output directory if it doesn't exist.
                if (!Directory.Exists(fullOutputDirectory))
                    Directory.CreateDirectory(fullOutputDirectory);

                // Reprocess any directories inside this one.
                RecursivelyProcessDirectories(resultFiles, directory.FullName, fullOutputDirectory);

                // Process any files inside these directories.
                resultFiles.AddRange(ProcessDirectory(directory.FullName, fullOutputDirectory).Select(v => GetRelativeCompiledPath(Path.Combine(fullOutputDirectory + "/", v))));
            }
        }

        private static List<string> ProcessDirectory(string inputDirectory, string outputDirectory)
        {
            var resultFiles = new List<string>();

            // We assume we're in the directory where all of the resources are.
            var contentBuilder = new XNAContentCompiler.ContentBuilder();

            // Add all files.
            var extensions = new string[]
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
            foreach (var extension in extensions)
            {
                foreach (var file in new DirectoryInfo(inputDirectory).GetFiles(extension))
                {
                    contentBuilder.Add(file.FullName, file.GetNameWithoutExtension());
                }
            }

            // Build and check for error.
            var error = contentBuilder.Build();
            if (error != null)
            {
                Console.Error.WriteLine(error);
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
