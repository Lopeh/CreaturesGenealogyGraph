using System;
using CreaturesGenealogyGraph.Models;

namespace CreaturesGenealogyGraph
{
    class Program
    {
        static void Main(string[] args)
        {
            string? file = args.Length > 0 ? args[0] : FindDefaultGenealogyFile();

            if (file != null)
            {
                var genealogyParser = new GenealogyParser();
                var creatureDict = genealogyParser.ParseGenealogy(file);

                Console.WriteLine($"{creatureDict.Count} creatures found.");

                var graphRenderer = new GraphRenderer(
                    creatureDict, 
                    genealogyParser.GetLivingDescendants(), 
                    genealogyParser.GetLivingAncestors(), 
                    showEggs: false, showLivingOnly: false);
                graphRenderer.RenderGraph("genealogy_graph");

                Console.WriteLine("Genealogy graph created successfully.");
            }
            else
            {
                Console.WriteLine("No genealogy file found. Please provide a valid genealogy file.");
            }
        }

        static string? FindDefaultGenealogyFile()
        {
            string scriptFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "./";
            string[] genealogyFiles = System.IO.Directory.GetFiles(scriptFolder, "*.genealogy");
            return genealogyFiles.Length > 0 ? genealogyFiles[0] : null;
        }

        static void DFSAncestors(string genomeMoniker, CreatureDictionary creatureDict, HashSet<string> livingAncestors)
        {
            if (!livingAncestors.Contains(genomeMoniker) && creatureDict.ContainsKey(genomeMoniker))
            {
                livingAncestors.Add(genomeMoniker);

                foreach (var (parentGenome, parent) in creatureDict[genomeMoniker].Parents)
                {
                    if (parent.Name != null && !livingAncestors.Contains(parentGenome))
                    {
                        DFSAncestors(parentGenome, creatureDict, livingAncestors);
                    }
                }
            }
        }
    }
}