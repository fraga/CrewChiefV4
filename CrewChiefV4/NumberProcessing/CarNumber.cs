using CrewChiefV4.Audio;
using System.Collections.Generic;

namespace CrewChiefV4.NumberProcessing
{
    public class CarNumber
    {
        private const string zerozero = "numbers/zerozero";
        private const string doubleoh = "numbers/double_oh";

        private int number;
        private string numberString;
        public CarNumber(int carNumber)
        {
            this.number = carNumber;
            this.numberString = carNumber.ToString();
        }
        public CarNumber(string carNumber)
        {
            this.number = int.Parse(carNumber);
            this.numberString = carNumber;
        }
        public List<MessageFragment> getMessageFragments()
        {
            List<MessageFragment> fragments = new List<MessageFragment>();
            // if we're not English, read the car number with the default number reader
            if (!NumberReaderFactory.IS_ENGLISH)
            {
                fragments.Add(MessageFragment.Integer(this.number));
                return fragments;
            }
            // some edge cases - 0, 00 and 000
            if (numberString == "0")
            {
                fragments.Add(MessageFragment.Integer(0));
                return fragments;
            }
            else if (numberString == "00")
            {
                if (SoundCache.availableSounds.Contains(zerozero))
                {
                    fragments.Add(MessageFragment.Text(zerozero));
                }
                else
                {
                    fragments.Add(MessageFragment.Integer(0));
                    fragments.Add(MessageFragment.Integer(0));
                }
                return fragments;
            }
            else if (numberString == "000")
            {
                if (SoundCache.availableSounds.Contains(doubleoh))
                {
                    fragments.Add(MessageFragment.Text(doubleoh));
                }
                else if (SoundCache.availableSounds.Contains(zerozero))
                {
                    fragments.Add(MessageFragment.Text(zerozero));
                }
                else
                {
                    fragments.Add(MessageFragment.Integer(0));
                    fragments.Add(MessageFragment.Integer(0));
                }
                fragments.Add(MessageFragment.Integer(0));
                return fragments;
            }
            if (number < 0 || number > 1000 || number % 100 == 0)
            {
                // round number of hundreds or unprocessable: just read it
                fragments.Add(MessageFragment.Integer(number));
            }
            else
            {
                // read as "two-twentysix", or "six zero one"
                int hundreds = number / 100;
                int remainder = number % 100;
                bool addedLeadingZeros = false;
                // if we have no hundreds, check for leading zeros in the number string and read them if necessary
                if (hundreds == 0)
                {
                    if (numberString.Length == 3 && numberString[0] == '0' && numberString[1] == '0')
                    {
                        if (SoundCache.availableSounds.Contains(doubleoh))
                        {
                            fragments.Add(MessageFragment.Text(doubleoh));
                        }
                        else
                        {
                            fragments.Add(MessageFragment.Text(zerozero));
                        }
                        addedLeadingZeros = true;
                    }
                    else if ((numberString.Length == 2 || numberString.Length == 3) && numberString[0] == '0')
                    {
                        fragments.Add(MessageFragment.Integer(0));
                        addedLeadingZeros = true;
                    }
                }
                else
                {
                    fragments.Add(MessageFragment.Integer(hundreds));
                }
                // if the remainder < 10, add a 'zero' if we haven't already
                if (remainder < 10)
                {
                    if (!addedLeadingZeros)
                    {
                        fragments.Add(MessageFragment.Integer(0));
                    }
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
