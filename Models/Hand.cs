using System;
using System.Collections.Generic;

namespace csharp_project.Models
{
    public class Hand
    {
        public List<Card> PlayerCards;
        public int HandValue {get;set;}
        public int BetValue {get;set;}

        //Hand Constructor
        public Hand()
        {
            HandValue = 0;
            BetValue = 0;
        }
        public bool CalculateHandValue()
        {
            HandValue = 0;
            bool BustedHand = false;
            bool HasAce = false; //Assume the hand doesn't have an Ace
            for (var i = 0; i < PlayerCards.Count; i++)
            {
                HandValue += PlayerCards[i].Value;
                if (PlayerCards[i].Value == 1)
                    HasAce = true; //Ace is found
            }

            //If the value of the Hand is less than or equal to 11 AND there is an Ace in the Hand, then add 10 to the Hand's value
            if (HandValue <= 11 && HasAce)
                HandValue += 10;
            
            if (HandValue > 21)
                BustedHand = true;
            
            return BustedHand;
        }
    }
}