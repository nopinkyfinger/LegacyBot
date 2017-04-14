/******************************************************************************
 * Character class - This is an object representing a character.
 * 
 * Written by Joseph Musial, 2017
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lulzbot.Extensions.RP_Tools
{
    /// <summary>
    /// This is the Character class. Each object represents a single character.
    /// </summary>
    public class Character
    {
        private String player;
        private String character;
        private String chatroom;
        private bool inUse;

        /// <summary>
        /// Default constructor.
        /// Sets string variables to "null" for debugging purposes.
        /// </summary>
        public Character()
        {
            player = "nullplayer";
            character = "nullcharacter";
            chatroom = "nullchatroom";
            inUse = false;
        }

        /// <summary>
        /// Full constructor, to fully set up the character.
        /// Takes in player, character, and chatroom
        /// </summary>
        /// <param name="inPlayer">Player name</param>
        /// <param name="inCharacter">Character name</param>
        /// <param name="inChatroom">Chatroom name</param>
        public Character(String inPlayer, String inCharacter, String inChatroom)
        {
            player = inPlayer;
            character = inCharacter;
            chatroom = inChatroom;
            inUse = true;
        }
        
        /// <summary>
        /// player setter
        /// </summary>
        /// <param name="newPlayer">new player name</param>
        public void setPlayer(String newPlayer)
        {
            player = newPlayer;
        }

        /// <summary>
        /// character setter
        /// </summary>
        /// <param name="inCharacter">new character name</param>
        public void setCharacter(String inCharacter)
        {
            character = inCharacter;
        }

        /// <summary>
        /// chatroom setter
        /// </summary>
        /// <param name="inChatroom">new chatroom name</param>
        public void setChatroom(String inChatroom)
        {
            chatroom = inChatroom;
        }

        /// <summary>
        /// inUse setter
        /// </summary>
        /// <param name="inInUse">new inUse value</param>
        public void setUse(bool inInUse)
        {
            inUse = inInUse;
        }

        /// <summary>
        /// player getter
        /// </summary>
        /// <returns>player name</returns>
        public String getPlayer()
        {
            return player;
        }

        /// <summary>
        /// character getter
        /// </summary>
        /// <returns>character name</returns>
        public String getCharacter()
        {
            return character;
        }

        /// <summary>
        /// chatroom getter
        /// </summary>
        /// <returns>chatroom name</returns>
        public String getChatroom()
        {
            return chatroom;
        }

        /// <summary>
        /// Check to see if character is in use
        /// </summary>
        /// <returns>Whether or not character is in use</returns>
        public bool isUsed()
        {
            return inUse;
        }

        /// <summary>
        /// Removes a character from the RP.
        /// </summary>
        public void removeCharacter()
        {
            // TODO: Return an int based on whether it was successful or not
            player = "nullplayer";
            character = "nullcharacter";
            chatroom = "nullchatroom";
            inUse = false;
        }
        
        /// <summary>
        /// Check to see if two characters are identical
        /// </summary>
        /// <param name="character">Second character (first is put before operator)</param>
        /// <returns>Whether or not characters are identical</returns>
        public bool Equals(Character character)
        {
            bool isEqual;

            // if chatroom, player, and character name are identical, ignoring case
            if (this.getCharacter().Equals(character.getCharacter(), StringComparison.InvariantCultureIgnoreCase) &&
            this.getChatroom().Equals(character.getChatroom(), StringComparison.InvariantCultureIgnoreCase) &&
            this.getPlayer().Equals(character.getPlayer(), StringComparison.InvariantCultureIgnoreCase))
                isEqual = true;
            else
                isEqual = false;

            return isEqual;
        }

        /// <summary>
        /// Returns a String representing the character data
        /// </summary>
        /// <returns>String representing the character data</returns>
        public override String ToString() 
        {
            String characterString = String.Format("{0}, played by {1} in the chatroom {2}",
                this.getCharacter(), this.getPlayer(), this.getChatroom());
            return characterString;
        }
    }
}
