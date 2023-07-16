using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using CreaturesGenealogyGraph.Models;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.RankedShortestPath;

namespace CreaturesGenealogyGraph
{
    public class GraphRenderer
    {
        private readonly CreatureDictionary creatureDict;
        private readonly CreatureSet livingDescendants;
        private readonly CreatureSet livingAncestors;
        private readonly bool showEggs;
        private readonly bool showLivingOnly;

        public GraphRenderer(CreatureDictionary creatureDict, CreatureSet livingDescendants, CreatureSet livingAncestors, bool showEggs, bool showLivingOnly)
        {
            this.creatureDict = creatureDict;
            this.livingDescendants = livingDescendants;
            this.livingAncestors = livingAncestors;
            this.showEggs = showEggs;
            this.showLivingOnly = showLivingOnly;
        }

        public void RenderGraph(string fileName)
        {
            // Func<Edge<string>, double> Weights = _ => 1.0;
            var graph = new BidirectionalGraph<string, Edge<string>>();
            // var algorithm = new HoffmanPavleyRankedShortestPathAlgorithm<string, Edge<string>>(graph, Weights);

            foreach (var (genomeMoniker, creature) in creatureDict)
            {
                if (!string.IsNullOrEmpty(creature.Name) || showEggs)
                {
                    if (!graph.ContainsVertex(genomeMoniker))
                    {
                        graph.AddVertex(genomeMoniker);
                    }

                    foreach (var (parentGenome, parent) in creature.Parents)
                    {
                        if (!graph.ContainsVertex(parentGenome))
                        {
                            graph.AddVertex(parentGenome);
                        }

                        graph.AddEdge(new Edge<string>(source: parentGenome, target: genomeMoniker));
                    }
                }
            }

            if (showLivingOnly) {
                RemoveNonLivingAncestors(graph);
            }

            var graphviz = new GraphvizAlgorithm<string, Edge<string>>(graph);
            // graphviz.GraphFormat.NodeSeparation = 1.0;
            // graphviz.GraphFormat.Splines = GraphvizSplineType.Ortho;
            graphviz.CommonVertexFormat.Font = new GraphvizFont("Handlee", 8.25f);
            // graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Record;
            // graphviz.CommonVertexFormat.Style = GraphvizVertexStyle.Filled;
            // graphviz.CommonVertexFormat.FillColor = GraphvizColor.White;
            graphviz.FormatVertex += (sender, args) => 
            {
                if (creatureDict.ContainsKey(args.Vertex)) {
                    var creature = creatureDict[args.Vertex];
                    var nodeStyle = new GraphvizVertex();
                    args.VertexFormat.Group = creature.Species.ToString();
                    SetCreatureNodeStyle(args.Vertex,creatureDict[args.Vertex], ref nodeStyle);
                    if (!string.IsNullOrEmpty(creature.Name))
                        args.VertexFormat.Label = creatureDict[args.Vertex].Name;
                    args.VertexFormat.Shape = nodeStyle.Shape;
                    args.VertexFormat.Style = nodeStyle.Style;
                    args.VertexFormat.FillColor = nodeStyle.FillColor;
                    args.VertexFormat.FontColor = nodeStyle.FontColor;
                    args.VertexFormat.StrokeColor = nodeStyle.StrokeColor;
                } else {
                    if (args.Vertex.Contains(".gen")){
                        args.VertexFormat.Label = args.Vertex;
                        args.VertexFormat.Shape = GraphvizVertexShape.InvHouse;
                        args.VertexFormat.FillColor = GraphvizColor.Yellow;
                        args.VertexFormat.Style = GraphvizVertexStyle.Filled;
                        args.VertexFormat.Position = new GraphvizPoint(0,0);
                    } else {
                        args.VertexFormat.Label = "Unknown";
                        args.VertexFormat.Shape = GraphvizVertexShape.Egg;
                    }
                }
            };
            graphviz.FormatEdge += (_, args) => 
            {
                if (creatureDict.ContainsKey(args.Edge.Target)){
                    var creature = creatureDict[args.Edge.Target];
                    var parent = creature.Parents[args.Edge.Source];
                    args.EdgeFormat.StrokeColor = GetEdgeColor(parent.Sex);
                } else if (args.Edge.Target.Contains(".gen")) {
                    args.EdgeFormat.StrokeColor = GraphvizColor.Yellow;
                    args.EdgeFormat.HeadArrow.Shape = GraphvizArrowShape.Dot;
                }
            };
            graphviz.Generate(new FileDotEngine(), fileName);
        }

        private void SetCreatureNodeStyle(string genomeMoniker, Creature creature, ref GraphvizVertex node)
        {
            node.StrokeColor = GraphvizColor.LightGray;
            node.Shape = GraphvizVertexShape.Rect;
            node.FontColor = GraphvizColor.Gray;
            node.FillColor = GraphvizColor.White;

            if (creature.Status == 1) // Unborn/in egg?
            {
                node.StrokeColor = GraphvizColor.LightGreen;
                node.Style = GraphvizVertexStyle.Filled;
                node.FillColor = GraphvizColor.LightSeaGreen;
                node.FontColor = GraphvizColor.White;
                node.Shape = GraphvizVertexShape.Egg;
                return;
            }

            if (livingDescendants.Contains(genomeMoniker))
            {
                node.Shape = GraphvizVertexShape.DoubleCircle;
                node.Style = GraphvizVertexStyle.Filled;
                node.FillColor = GraphvizColor.LightBlue;
                node.FontColor = GraphvizColor.Black;
            }
            else if (livingAncestors.Contains(genomeMoniker))
            {
                node.Shape = GraphvizVertexShape.Circle;
                node.Style = GraphvizVertexStyle.Filled;
                node.FillColor = GraphvizColor.LightGray;
                node.FontColor = GraphvizColor.Black;
            }

            if (creature.Status == 4) // Exported?
                node.Shape = GraphvizVertexShape.House;

            if (creature.Warped == 1) //Has warped?
            {
                // nodeOptions.FillColor += ":blue";
                node.Style = GraphvizVertexStyle.Filled;
                //nodeOptions.Angle = 90;
            }

            switch (creature.Sex)
            {
                case "male":
                    node.StrokeColor = GraphvizColor.Blue;
                    break;
                case "female":
                    node.StrokeColor = GraphvizColor.DeepPink;
                    break;
                case "non-binary":
                    node.StrokeColor = GraphvizColor.Pink;
                    break;
            }
        }

        private static GraphvizColor GetEdgeColor(string sex)
        {
            return sex switch
            {
                "male" => GraphvizColor.Blue,
                "female" => GraphvizColor.DeepPink,
                _ => GraphvizColor.Gray
            };
        }

        private void RemoveNonLivingAncestors(BidirectionalGraph<string, Edge<string>> graph)
        {
            var livingCreatureDict = new CreatureDictionary();
            foreach (var (genomeMoniker, _) in creatureDict)
            {
                if (livingDescendants.Contains(genomeMoniker) || livingAncestors.Contains(genomeMoniker))
                {
                    livingCreatureDict.Add(genomeMoniker, creatureDict[genomeMoniker]);
                }
            }

            foreach (var vertex in graph.Vertices.ToArray())
            {
                if (!livingCreatureDict.ContainsKey(vertex))
                {
                    graph.RemoveVertex(vertex);
                }
            }
        }
    }
}
