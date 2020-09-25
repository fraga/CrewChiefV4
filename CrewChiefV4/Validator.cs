using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrewChiefV4
{
    class Validator
    {

        // TODO: add more undeserving **** to this list as and when they crawl out the woodwork
        // Mostly for wrecking but some notable exceptions - sangalli for thinking it's ok to threaten people, 
        // hance, hotdog and koch for being extraordinarily ignorant and rude, and so on. My app, my rules :)
         private static HashSet<String> wnkers = new HashSet<String>(StringComparer.InvariantCultureIgnoreCase) { "mr.sisterfister", "bigsilverhotdog", 
             "aline senna", "giuseppe sangalli", "patrick förster", "chris iwaski", "gazman", "peter koch",
             "andreas christiansen", "Aditas H1Z1Cases.com.",
             "maciej bugno", "patrick schilhan" /*as reward for your "make r3e app free" campaign.*/,
             "tim heinemann", "Josh Cassar", "Jesse Hoppo", "markus grönthal", "joan moreno", "N.Quinque", "epsilon",
             "^1!^5c^3hrist ^5i^3n ^5a^3ction", "slightly mad", "dickovens", "Pyromaniac", "luismat112", "Ruben Zukic", "Adam L Watson",
             "Dave Rowe"/*for brake checking doing daytona 24h and being a general dck*/, "Visar Gjikolli", "Lukasz Salwerowicz",
             "xavi lópez", "bernie ecclestone" /*roughing up in ranked*/, "Klaus Friis Andersen"};


          public static void validate(String str)
          {
             if (wnkers.Contains(str.Trim()))
             {
                 throw new ValidationException();
             }
          }
    }

    /**
     * special exception for special players so they can grumble that the app is spamming an exception. I'd call it something
     * more colourful but my usual charm deserts me.
     */
    class ValidationException : Exception
    {
        public ValidationException()
            : base() { }
    }
}
