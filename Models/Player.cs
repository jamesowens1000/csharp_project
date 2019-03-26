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

        public Hand CurrHand {get;set;}
        // public List<Hand> CurrHands {get;set;}   //Add this back in for when a player can have multiple hands

        //Player constructor
        public Player()
        {
            CurrHand = new Hand();
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