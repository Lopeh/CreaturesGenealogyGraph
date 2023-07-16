using System.Collections.Generic;

namespace CreaturesGenealogyGraph.Models
{
    public class CreatureDictionary : Dictionary<string, Creature>
    {
        private readonly Dictionary<string, Creature> _creatureDictionary;

        public CreatureDictionary()
        {
            _creatureDictionary = new Dictionary<string, Creature>();
        }

        public override bool Equals(object? obj)
        {
            return obj is CreatureDictionary dictionary &&
                   EqualityComparer<Dictionary<string, Creature>>.Default.Equals(_creatureDictionary, dictionary._creatureDictionary);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}