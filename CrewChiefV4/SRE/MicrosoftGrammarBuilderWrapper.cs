using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class MicrosoftGrammarBuilderWrapper : GrammarBuilderWrapper
    {
        private GrammarBuilder grammarBuilder;

        public MicrosoftGrammarBuilderWrapper()
        {
            this.grammarBuilder = new GrammarBuilder();
        }

        public MicrosoftGrammarBuilderWrapper(ChoicesWrapper choicesWrapper)
        {
            this.grammarBuilder = new GrammarBuilder((Choices)choicesWrapper.GetInternalChoices());
        }

        void GrammarBuilderWrapper.Append(ChoicesWrapper choicesWrapper)
        {
            grammarBuilder.Append((Choices)choicesWrapper.GetInternalChoices());
        }

        void GrammarBuilderWrapper.Append(string text)
        {
            grammarBuilder.Append(text);
        }

        object GrammarBuilderWrapper.GetInternalGrammarBuilder()
        {
            return grammarBuilder;
        }

        void GrammarBuilderWrapper.SetCulture(CultureInfo cultureInfo)
        {
            grammarBuilder.Culture = cultureInfo;
        }
    }
}
