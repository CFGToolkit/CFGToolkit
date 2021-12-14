using System.Collections;
using System.Collections.Generic;

namespace CFGToolkit.Grammar.Structure
{
    public class Expressions : IEnumerable<Expression>
    {
        private List<Expression> _alternatives { get; set; } = new List<Expression>();

        public Expressions()
        {

        }
        public Expressions(params object[] alternatives)
        {
            foreach (var item in alternatives)
            {
                if (item is Expression e)
                {
                    _alternatives.Add(e);
                }

                if (item is IEnumerable<Expression> en)
                {
                    _alternatives.AddRange(en);
                }
            }
        }
        public Expression this[int i]
        {
            get { return _alternatives[i]; }
            set { _alternatives[i] = value; }
        }

        public int Count => _alternatives.Count;

        public IEnumerator<Expression> GetEnumerator()
        {
            return _alternatives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _alternatives.GetEnumerator();
        }

        public Expressions Clone()
        {
            return new Expressions(new List<Expression>(this));
        }

        public void AddRange(IEnumerable<Expression> alternatives)
        {
            _alternatives.AddRange(alternatives);
        }
        public void Add(Expression item)
        {
            _alternatives.Add(item);
        }

        public override string ToString()
        {
            return string.Join("|", _alternatives);
        }
    }
}
