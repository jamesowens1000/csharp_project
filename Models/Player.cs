using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace csharp_project.Models
{
    public class Player
    {
        //These are the properties that will be stored in the DB
        [Key]
        public int PlayerID {get;set;}
        [Required]
        public string Username {get;set;}
        [Required]
        public string Password {get;set;}
        public double Money {get;set;}
        public int HandsPlayed {get;set;}
        public int HandsWon {get;set;}

        //These are the properties that will NOT be stored in the DB
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm {get;set;}

        public List<Hand> CurrHands {get;set;}

        //Player constructor
        public Player()
        {
            CurrHands = new List<Hand>();
            Hand Hand1 = new Hand();
            CurrHands.Add(Hand1);
        }

        public Card Draw()
        {
            Deck thisDeck = new Deck();
            Card drawnCard = thisDeck.Deal();
            CurrHands[0].PlayerCards.Add(drawnCard);
            return drawnCard;
        }
    }

    public class LoginPlayer
    {
        [Required]
        public string Username {get;set;}

        [Required]
        [DataType(DataType.Password)]
        public string Password {get;set;}
    }
}