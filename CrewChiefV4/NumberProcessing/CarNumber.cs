using System.Collections.Generic;

namespace CrewChiefV4.NumberProcessing
{
    public class CarNumber
    {
        private int number;
        public CarNumber(int carNumber)
        {
            this.number = carNumber;
        }
        public CarNumber(string carNumber)
        {
            this.number = int.Parse(carNumber);
        }
        public List<MessageFragment> getMessageFragments()
        {
            List<MessageFragment> fragments = new List<MessageFragment>();
            if (number <= 0 || number < 100 || number > 1000 || number % 100 == 0)
            {
                // <100 or round number of hundreds: just read it
                fragments.Add(MessageFragment.Integer(number));
            }
            else
            {
                // read as "two-twentysix", or "six zero one"
                int hundreds = number / 100;
                int remainder = number % 100;
                fragments.Add(MessageFragment.Integer(hundreds));
                // if the remainder < 10, add a 'zero'
                if (remainder < 10)
                {
                    fragments.Add(MessageFragment.Integer(0));
                    fragments.Add(MessageFragment.Integer(remainder));
                }
                // if the remainder starts with the same as the hundreds (e.g. 551), read as "five five one"
                else if (hundreds > 1 && remainder / 10 == hundreds)
                {
                    fragments.Add(MessageFragment.Integer(remainder / 10));
                    fragments.Add(MessageFragment.Integer(remainder % 10));
                }
                // for others read as "thirty five" or whatever
                else
                {
                    fragments.Add(MessageFragment.Integer(remainder));
                }
            }
            return fragments;
        }
    }
}
