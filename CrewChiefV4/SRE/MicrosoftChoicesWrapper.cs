using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class MicrosoftChoicesWrapper : ChoicesWrapper
    {
        private Choices internalChoices;

        public MicrosoftChoicesWrapper()
        {
            this.internalChoices = new Choices();
        }

        public MicrosoftChoicesWrapper(string[] choices)
        {
            this.internalChoices = new Choices(choices);
        }

        public void Add(string phrase)
        {
            internalChoices.Add(phrase);
        }

        public void Add(string[] phrases)
        {
            internalChoices.Add(phrases);
        }

        public object GetInternalChoices()
        {
            return internalChoices;
        }
    }
}
