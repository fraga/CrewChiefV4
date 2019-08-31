using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class SystemGrammarWrapper : GrammarWrapper
    {
        private Grammar grammar;

        public SystemGrammarWrapper(GrammarBuilderWrapper grammarBuilderWrapper)
        {
            this.grammar = new Grammar((GrammarBuilder) grammarBuilderWrapper.GetInternalGrammarBuilder());
        }

        public SystemGrammarWrapper(Grammar grammar)
        {
            this.grammar = grammar;
        }

        public object GetInternalGrammar()
        {
            return grammar;
        }

        public bool Loaded()
        {
            return grammar.Loaded;
        }
    }
}
