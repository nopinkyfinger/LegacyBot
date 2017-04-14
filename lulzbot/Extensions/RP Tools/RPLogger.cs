using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lulzbot.Types;

namespace lulzbot.Extensions.RP_Tools
{
    class RpLogger
    {
        private Roleplay[] botRPList;
        private Bot bot;
        public RpLogger(ref Roleplay[] listOfRPs, Bot botIn)
        {
            botRPList = listOfRPs;
            bot = botIn;
        }

        public void logAction(String msg, String sender, String chatroom)
        {
            int nameEnd;
            int rpID;
            int firstSpace;
            String post;
            String characterName;
            Character character;

            // Writes all /me posts
            String day = Tools.strftime("%B %d %Y");
            String month = Tools.strftime("%Y-%m %B");
            String rpPath = String.Format("Storage/RP Logs/{0}/{1}/{2}.txt", chatroom, month, day);
            String rpContent = String.Format("* {0} {1}{2}", sender, msg, Environment.NewLine);
            Tools.WriteFile(rpPath, rpContent, true);

            nameEnd = msg.IndexOf(':');
            firstSpace = msg.IndexOf(' ');
            // If the first colon comes before the first space, it sees it as a tagged character.
            if (nameEnd != -1 && (nameEnd < firstSpace))
            {
                characterName = msg.Substring(0, nameEnd);
                post = msg.Substring(nameEnd + 2);

                character = new Character(sender, characterName, chatroom);

                rpID = findRP(character);

                if (rpID == -1)
                {
                    bot.Say(chatroom, character.ToString() + " is not currently in any active RPs. "
                        + "Please add this character to an RP to have it filter logged.");
                }
                else
                {
                    Tools.WriteFile(botRPList[rpID].getPath(), msg + "\n", true);
                    //bot.Say(chatroom, "Saved RP post to " + botRPList[rpID].getPath());
                }
            }
        }

        public void test()
        {
            bot.Say("#Testingstuffs", botRPList[0].ToString());
        }

        public int findRP(Character character)
        {
            bool hasCharacter = false;
            int slot;
            int index = 0;
            while (hasCharacter == false && index < 128)
            {
                if (botRPList[index].hasCharacter(character))
                    hasCharacter = true;
                else
                    index++;
            }

            if (hasCharacter == true)
            {
                slot = index;
            }
            else
            {
                slot = -1;
                bot.Say("#Testingstuffs", "Did not find RP");
            }
            return slot;
        }
    }
}
