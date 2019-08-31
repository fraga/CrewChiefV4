using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class MicrosoftGrammarWrapper : GrammarWrapper
    {
        private Grammar grammar;
        public MicrosoftGrammarWrapper(GrammarBuilderWrapper grammarBuilderWrapper)
        {
            this.grammar = new Grammar((GrammarBuilder)grammarBuilderWrapper.GetInternalGrammarBuilder());
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
