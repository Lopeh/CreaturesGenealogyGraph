using System.Collections.Generic;

namespace CreaturesGenealogyGraph.Models
{
    public sealed class CreatureSet : HashSet<string>
    {
        public CreatureSet()
        {

        }
        public CreatureSet(IEnumerable<string> collection) : base(collection)
        {
        }
    }
}
