using System;
using System.Collections.Generic;

namespace csharp_project.Models
{
    public class Deck
    {
        string[] Faces = new string[13] {"Ace","Two","Three","Four","Five","Six","Seven","Eight","Nine","Ten","Jack","Queen","King"};
        string[] Suits = new string[4] {"Clubs","Diamonds","Hearts","Spades"};
        public List<Card> Cards;

        //Deck constructor
        public Deck()
        {
            Cards = new List<Card>();

            for (int i = 0; i < Suits.Length; i++)
            {
                for (int j = 0; j < Faces.Length; j++)
                {
                    Card NewCard = new Card();
                    NewCard.Suit = Suits[i];
                    NewCard.Face= Faces[j];

                    //If a card has a face value of "Jack", "Queen", or "King" then we need to assign them a numeric value of 10
                    if ("JackQueenKing".Contains(NewCard.Face))
                    {
                        NewCard.Value = 10;
                    }
                    else
                    {
                        //Ace is assigned a numeric value of 1, all other cards are assigned a numeric value equivalent to their face value
                        NewCard.Value = j+1;
                    }
                }
            }
        }

        //Shuffles the deck by reordering the cards against random indexes
        public void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < Cards.Count; i++)
            {
                Card temp = Cards[i];
                int randIndex = rand.Next(i, Cards.Count);
                Cards[i] = Cards[randIndex];
                Cards[randIndex] = temp;
            }
        }

        //Deals a card by taking/ramoving the first card from the top of the deck and dealing/giving it out to a player
        public Card Deal()
        {
            Card DealtCard = new Card();
            DealtCard = Cards[0];
            Cards.RemoveAt(0);
            return DealtCard;
        }
    }
}