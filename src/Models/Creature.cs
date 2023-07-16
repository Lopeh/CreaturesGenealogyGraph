using QuikGraph;

namespace CreaturesGenealogyGraph.Models
{
    public class Creature
    {
        public string Name { get; set; }
        public string ?GenomeMoniker { get; set; }
        public Dictionary<string, Parent> Parents { get; set; }
        public int Status { get; set; }
        public int Species { get; set; }
        public string ?Sex { get; set; }
        public int Variant { get; set; }
        public int Warped { get; set; }
        public Creature(string Name){
            this.Name = Name;
            this.Parents = new Dictionary<string, Parent>();
        }
    }

    public class Parent
    {
        public string GenomeMoniker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
    }

    public class CVertex
    {
        public double X { get; set; }
        public double Y { get; set; }

        // Creature? Creature {get;set;}
    }

    public class CEdge : IEdge<CVertex>
    {
        public CVertex Source {get;set;}
        public CVertex Target {get;set;}
        public CEdge(CVertex source, CVertex target)
        {
            Source = source;
            Target = target;
        }
    }

}