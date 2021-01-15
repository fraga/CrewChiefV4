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
            // this wil dump the SRE grammar object to the console (as a list of the phrases in all of its choices). 
            // Perhaps it should be a debug line?
            // Console.WriteLine("Create grammar with contents\n " + ((GrammarBuilder)grammarBuilderWrapper.GetInternalGrammarBuilder()).DebugShowPhrases);
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
