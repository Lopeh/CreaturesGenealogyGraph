using System.Text.RegularExpressions;
using CreaturesGenealogyGraph.Models;

namespace CreaturesGenealogyGraph
{
    public partial class GenealogyParser
    {
        private static readonly Regex StatusPattern = StatusLineRegex();
        private static readonly Regex SpeciesPattern = SpeciesLineRegex();
        private static readonly Regex SexPattern = SexLineRegex();
        private static readonly Regex VariantPattern = VariantLineRegex();
        private static readonly Regex WarpedPattern = WarpedLineRegex();

        private readonly CreatureSet livingDescendants;
        private readonly CreatureSet livingAncestors;
        private readonly CreatureDictionary creatureDict;

        public GenealogyParser()
        {
            livingAncestors = new CreatureSet();
            livingDescendants = new CreatureSet(); 
            creatureDict = new CreatureDictionary();
        }

        public CreatureDictionary ParseGenealogy(string filePath)
        {
            var data = File.ReadAllText(filePath);
            Console.WriteLine($"Hello there {filePath}");
            var records = data.Split(new[] { "\r\n\r\n" }, StringSplitOptions.None);

            Console.WriteLine($"{records.Length} records found!");

            foreach (var record in records)
            {
                var lines = record.Trim().Split("\r\n");

                if (lines.Length >= 8)
                {
                    var nameLine = lines[0].Split(": ");
                    var (name, genomeMoniker) = SplitNameMoniker(nameLine[1]);

                    var creature = new Creature(name);

                    if (string.IsNullOrEmpty(genomeMoniker)) {
                        continue;
                    }

                    var statusMatch = StatusPattern.Match(lines[3]);
                    if (statusMatch.Success)
                    {
                        var status = int.Parse(statusMatch.Groups[1].Value);

                        // Ignore creatures with status 7 or higher
                        if (status >= 7)
                            continue;

                        if (status == 3)
                            livingDescendants.Add(genomeMoniker);

                        creature.Status = status;
                    }

                    var parentALine = lines[1].Split(": ");
                    var parentBLine = lines[2].Split(": ");
                    var speciesMatch = SpeciesPattern.Match(lines[4]);
                    var sexMatch = SexPattern.Match(lines[5]);
                    var variantMatch = VariantPattern.Match(lines[6]);
                    var warpedMatch = WarpedPattern.Match(lines[7]);

                    var parentA = CreateParentObject(parentALine);
                    var parentB = CreateParentObject(parentBLine);

                    if (!string.IsNullOrEmpty(parentA.GenomeMoniker))
                        creature.Parents.Add(parentA.GenomeMoniker, parentA);

                    if (!string.IsNullOrEmpty(parentB.GenomeMoniker) && parentB.GenomeMoniker != parentA.GenomeMoniker)
                        creature.Parents.Add(parentB.GenomeMoniker, parentB);

                    if (creature.Parents.Count > 1 && (parentA.Sex == "unknown" != (parentB.Sex == "unknown")))
                    {
                        creature.Parents[parentA.GenomeMoniker].Sex = 
                            parentA.Sex == "unknown" ? "female" : creature.Parents[parentA.GenomeMoniker].Sex;
                        creature.Parents[parentB.GenomeMoniker].Sex = 
                            parentB.Sex == "unknown" ? "male" : creature.Parents[parentB.GenomeMoniker].Sex;
                    }


                    if (speciesMatch.Success)
                        creature.Species = int.Parse(speciesMatch.Groups[1].Value);

                    if (sexMatch.Success)
                    {
                        var sex = int.Parse(sexMatch.Groups[1].Value);
                        creature.Sex = GetSexFromCode(sex);
                    }

                    if (variantMatch.Success)
                        creature.Variant = int.Parse(variantMatch.Groups[1].Value);

                    if (warpedMatch.Success)
                        creature.Warped = int.Parse(warpedMatch.Groups[1].Value);

                    creatureDict.Add(genomeMoniker, creature);
                }
            }

            return creatureDict;
        }

        private static (string name, string genomeMoniker) SplitNameMoniker(string nameWithMoniker)
        {
            var parts = nameWithMoniker.Split();

            if (parts.Length > 1)
            {
                var name = string.Join(" ", parts[..^1]);
                if (string.IsNullOrEmpty(name))
                    name = "Unknown";
                var genomeMoniker = parts[^1];
                return (name, genomeMoniker);
            }
            else if (parts.Length == 1)
            {
                var name = parts[0].Contains(".gen") ? "Unknown" : parts[0];
                var genomeMoniker = parts[0];
                return (name, genomeMoniker);
            }
            else
            {
                return (name: string.Empty, genomeMoniker: string.Empty);
            }
        }

        private static Parent CreateParentObject(string[] parentLine)
        {
            var (name, genomeMoniker) = SplitNameMoniker(parentLine[1]);
            var parent = new Parent
            {
                GenomeMoniker = genomeMoniker,
                Name = name,
                Sex = GetParentSex(parentLine[0])
            };

            return parent;
        }

        private static string GetParentSex(string parentType)
        {
            return parentType switch
            {
                "Mother" => "female",
                "Father" => "male",
                "Unknown" => "unknown",
                _ => string.Empty
            };
        }

        private static string GetSexFromCode(int sexCode)
        {
            return sexCode switch
            {
                1 => "male",
                2 => "female",
                -1 => "undetermined",
                0 => "non-binary",
                _ => string.Empty
            };
        }

        public CreatureSet GetLivingDescendants()
        {
            return livingDescendants;
        }

        public CreatureSet GetLivingAncestors()
        {
            foreach (string genomeMoniker in livingDescendants)
            {
                DFSAncestors(genomeMoniker);
            }
            return livingAncestors;
        }

        private void DFSAncestors(string genomeMoniker)
        {
            if (!livingAncestors.Contains(genomeMoniker))
            {
                livingAncestors.Add(genomeMoniker);
            }

            if (!creatureDict.ContainsKey(genomeMoniker))
            {
                return;
            }

            Creature creature = creatureDict[genomeMoniker];
            foreach (var (parentGenome, parent) in creature.Parents)
            {
                if (!string.IsNullOrEmpty(parent.Name) && !livingAncestors.Contains(parentGenome))
                {
                    DFSAncestors(parentGenome);
                }
            }
        }

        [GeneratedRegex("Status:\\s+(\\d+)")]
        private static partial Regex StatusLineRegex();
        [GeneratedRegex("Species:\\s+(\\d+)")]
        private static partial Regex SpeciesLineRegex();
        [GeneratedRegex("Sex:\\s+(-?\\d+)")]
        private static partial Regex SexLineRegex();
        [GeneratedRegex("Variant:\\s+(-?\\d+)")]
        private static partial Regex VariantLineRegex();
        [GeneratedRegex("Has Warped:\\s+(\\d+)")]
        private static partial Regex WarpedLineRegex();
    }
}
