using lulzbot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This class handles an RP. It keeps track of who is playing who. It also
// automatically formats each line and outputs it to its own file. 
// THE BOT STILL LOGS EVERY RP LINE JUST TO BE SAFE

namespace lulzbot.Extensions
{
    public class RP
    {
        // This isn't meant to be called, it's just a fallback for a blank argument
        public RP()
        {

        }

        // This is the real class- it takes in an array of strings probably and
        // maybe some other stuff. The array of strings is supposed to be the
        // usernames involved in the rp and the characters they correspond to.
        // TODO:
        //  What if the user mistypes the character name?
        //  What if the character is a secondary, and is tagged in every post?
        //  
        // For args, even numbers are usernames, odd numbers are characters.
        public RP(string[] args)
        {
            int length = args.Length;
        }

        // Prompt the user to make sure they did everything right
        public void confirmRP(string[] args)
        {
            
        }
    }
}
