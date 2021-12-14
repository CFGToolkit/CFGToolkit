using System.Collections.Generic;

namespace CFGToolkit.Grammar.Readers
{
    public class UniqueIdentifiers
    {
        private Dictionary<string, int> _counter = new Dictionary<string, int>();

        public string GetUniqueName(string name)
        {
            if (!_counter.ContainsKey(name))
            {
                _counter[name] = 2;
                return name;
            }
            else
            {
                var res = name + "_" + _counter[name];
                _counter[name] += 1;
                return res;
            }
        }
    }
}
