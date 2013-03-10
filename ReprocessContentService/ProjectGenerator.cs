using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Reflection;

namespace ReprocessContentService
{
    class ProjectGenerator
    {
        internal static void Generate(XmlWriter writer, List<string> resultFiles)
        {
            var xmlTree = new XDocument(
                              new XElement("project",
                                  from file in resultFiles
                                  select new XElement("item", file)
                              )
                          );

            XDocument newTree = new XDocument();

            // Load the style sheet.
            var xslt = new XslCompiledTransform();
            xslt.Load(XmlReader.Create(new StreamReader(Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("ReprocessContentService.ProjectGenerator.xslt"))));

            // Execute the transform and output the results to a writer.
            xslt.Transform(xmlTree.CreateReader(), writer);
        }
    }
}
