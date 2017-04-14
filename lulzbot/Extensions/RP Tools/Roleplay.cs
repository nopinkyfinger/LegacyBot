/******************************************************************************
 * Roleplay class - this is an object representing a roleplay.
 * 
 * Written by Joseph Musial, 2017
 * 
 * PARAMETERS:
 * const int MAX_ALLOWED_CHARACTERS - the maximum amount of characters in one
 *      rp. Currently set to 64. Should be more than enough.  
 * Character[] characterArray - an array of all the characters in the RP
 * String chatroom - the chatroom in which the RP is taking place
 * Bot bot - the bot object, used to send messages.
 *      TODO: find a cleaner way to send messages to chat
 * bool active - keeps track of whether or not the rp is currently being played
 * bool taken - whether or not rp has been used today, not currently in use but may be needed later.
 * 
 * METHODS:
 * public Roleplay() - default constructor.
 * public Roleplay(String inChatroom, Bot inBot) - overloaded constructor
 * public void addCharacter - adds a character to RP
 * public void removeCharacter - removes a character from the RP
 * public int getNextAvailableSlot - gets next unused character slot
 * public int getCharacterSlot - gets slot of specified character.
 * 
 * TODO:
 * Try to find a way to send messages without importing the bot, if possible. Low Priority.
 * Would it be more memory efficient to take in a pointer to the bot?
 * Clean up the code (after everything is done)
 */



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lulzbot.Types;
//TODO: Clean this up, maybe get some sleep

namespace lulzbot.Extensions.RP_Tools
{
    /// <summary>
    /// This is a class representing a roleplay. Summary tag used.
    /// </summary>
    public class Roleplay
    {
        private const int MAX_ALLOWED_CHARACTERS = 64;
        private Character[] characterArray = new Character[MAX_ALLOWED_CHARACTERS];
        private String chatroom;
        private String path;
        private Bot bot;
        private bool active; // is the rp currently in use
        private bool taken; // has the rp been used today. May scrap in favor of something else. Like a dynamic system scanning through text files.
        private int logID; // The number for the text files.
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public Roleplay()
        {
            active = false;
            taken = false;
            logID = -1;
            path = String.Empty;

            for (int i = 0; i < MAX_ALLOWED_CHARACTERS; i++)
                characterArray[i] = new Character();
        }

        /// <summary>
        /// Overloaded constructor, takes a bot and a chatroom name
        /// </summary>
        /// <param name="inChatroom">Chatroom name</param>
        /// <param name="inBot">The bot currently being used. This is a hacky solution.
        /// It's passed in to enable this class to send messages. A better method is needed.</param>
        public Roleplay(String inChatroom, Bot inBot)
        {
            chatroom = inChatroom;
            bot = inBot;
            active = false;
            taken = false;
            for (int i = 0; i < MAX_ALLOWED_CHARACTERS; i++)
                characterArray[i] = new Character();
        }

        /// <summary>
        /// Add a character to the rp
        /// </summary>
        /// <param name="newCharacter">The character to add</param>
        public void addCharacter(Character newCharacter)
        {
            //int slot = getNextAvailableSlot(characterArray, newCharacter);
            int slot = getNextAvailableSlot();

            if (slot == -1)
                bot.Say(newCharacter.getChatroom(), "Failed to add new character. int slot = -1");
            else
            {
                characterArray[slot] = newCharacter;
            }
        }

        // Edited Jan 26
        /// <summary>
        /// Remove a character from the RP
        /// </summary>
        /// <param name="inCharacter">The character to remove</param>
        public void removeCharacter(Character inCharacter)
        {
            int slot = getCharacterSlot(inCharacter);

            if (slot == -1) // character not found
                bot.Say(chatroom, "Failed to find " + inCharacter.ToString()
                    + ". Are you sure you typed everything right?");
            else // character found
            {
                // Code inside added jan 26
                characterArray[slot].removeCharacter();
            }
        }

        //public int getNextAvailableSlot(Character[] charList, Character newCharacter)
        // Removed params Jan 26
        /// <summary>
        /// Get the next unused character slot
        /// </summary>
        /// <returns>The value of the next unused character slot</returns>
        public int getNextAvailableSlot()
        {
            bool hasSlot = false;
            int index = 0;
            int slot = -1;

            while (hasSlot == false)
            {
                if (characterArray[index].isUsed() == false)
                {
                    slot = index;
                    hasSlot = true;
                }
                index++;
            }
            return slot;
        }

        /// <summary>
        /// Get the slot that matches a character.
        /// </summary>
        /// <param name="character">The character to search for</param>
        /// <returns>The slot of the specified character. Returns -1 if it couldn't be found.</returns>
        public int getCharacterSlot(Character character)
        {
            bool hasSlot = false;
            int index = 0;
            int slot = -1;

            while (hasSlot == false && index < MAX_ALLOWED_CHARACTERS)
            {
                if (character.Equals(characterArray[index]))
                {
                    hasSlot = true;
                    slot = index;
                }
                index++;
            }
            return slot;
        }

        // Jan 26
        /// <summary>
        /// Checks to see if specified character is in the RP
        /// </summary>
        /// <param name="character">The character to search for</param>
        /// <returns>Whether or not the character is in the RP</returns>
        public bool hasCharacter(Character character)
        {
            int index = getCharacterSlot(character);
            bool hasChar;
            

            if (index != -1)
                hasChar = true;
            else
                hasChar = false;

            return hasChar;
        }

        /// <summary>
        /// Returns a string representing the data in the RP
        /// </summary>
        /// <returns>String representing the data in the RP</returns>
        public override string ToString()
        {
            String str = "";
            for (int i = 0; i < MAX_ALLOWED_CHARACTERS; i++)
            {
                if (characterArray[i].isUsed())
                    str = str + characterArray[i].ToString() + ". ";
            }
            return str;
        }

        /// <summary>
        /// Clears the character array for the rp.
        /// </summary>
        public void clear()
        {
            for (int i = 0; i < MAX_ALLOWED_CHARACTERS; i++)
            {
                characterArray[i] = new Character();
            }
        }

        // TODO: This seems hacky. Try to find another way.
        /// <summary>
        /// Set the bot as the current bot so we can say things from this class
        /// </summary>
        /// <param name="newBot">The bot to set the bot variable equal to</param>
        public void setBot(Bot newBot)
        {
            bot = newBot;
        }

        /// <summary>
        /// Setter for chatroom, shouldn't be needed but you never know.
        /// </summary>
        /// <param name="newChatroom">The new chatroom</param>
        public void setChatroom(String newChatroom)
        {
            chatroom = newChatroom;
        }

        /// <summary>
        /// Setter for active
        /// </summary>
        /// <param name="isActive">New active value</param>
        public void setActive(bool isActive)
        {
            active = isActive;
        }

        /// <summary>
        /// Setter for taken
        /// </summary>
        /// <param name="isTaken">New taken value</param>
        public void setTaken(bool isTaken)
        {
            taken = isTaken;
        }

        /// <summary>
        /// Sets the log ID
        /// </summary>
        /// <param name="newID">The new ID for the RP</param>
        public void setLogID(int newID)
        {
            logID = newID;
        }

        /// <summary>
        /// Sets the logging path
        /// </summary>
        /// <param name="newPath"></param>
        public void setPath(String newPath)
        {
            path = newPath;
        }
        
        /// <summary>
        /// Returns the bot used. Shouldn't be needed but adding just in case
        /// </summary>
        /// <returns>The bot that's used</returns>
        public Bot getBot()
        {
            return bot;
        }

        /// <summary>
        /// Getter for chatroom
        /// </summary>
        /// <returns>Name of chatroom the RP is in</returns>
        public String getChatroom()
        {
            return chatroom;
        }

        /// <summary>
        /// Getter for active
        /// </summary>
        /// <returns>Whether or not the RP is active</returns>
        public bool isActive()
        {
            return active;
        }

        /// <summary>
        /// Getter for taken.
        /// </summary>
        /// <returns>Whether or not the RP has been used today</returns>
        public bool isTaken()
        {
            return taken;
        }

        public int getLogID()
        {
            return logID;
        }

        public String getPath()
        {
            return path;
        }
    }
}
