using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class SystemChoicesWrapper : ChoicesWrapper
    {
        private Choices internalChoices;

        public SystemChoicesWrapper()
        {
            this.internalChoices = new Choices();
        }

        public SystemChoicesWrapper(string[] choices)
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
