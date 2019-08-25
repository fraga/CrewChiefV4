using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    interface GrammarBuilderWrapper
    {
        void SetCulture(CultureInfo cultureInfo); // internal.Culture = ...

        void Append(ChoicesWrapper choicesWrapper);

        void Append(String text);

        object GetInternalGrammarBuilder();
    }
}
