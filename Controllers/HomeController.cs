﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using csharp_project.Models;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

namespace csharp_project.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        //Index
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        //Register
        [HttpPost("register")]
        public IActionResult TryRegister(IndexViewModel modelData)
        {
            Player regPlayer = modelData.RegPlayer;
            if (ModelState.IsValid)
            {
                if (dbContext.Players.Any(p => p.Username == regPlayer.Username))
                {
                    ModelState.AddModelError("RegPlayer.Username", "Username already in use!");
                }
                else
                {
                    PasswordHasher<Player> Hasher = new PasswordHasher<Player>();
                    regPlayer.Password = Hasher.HashPassword(regPlayer, regPlayer.Password);
                    Player createdPlayer = new Player();
                    createdPlayer.Username = regPlayer.Username;
                    createdPlayer.Password = regPlayer.Password;
                    dbContext.Add(createdPlayer);
                    dbContext.SaveChanges();
                    Player ThisPlayer = dbContext.Players.FirstOrDefault(u => u.Username == createdPlayer.Username);
                    HttpContext.Session.SetObjectAsJson("ThisPlayer", ThisPlayer);
                    return RedirectToAction("Dashboard");
                }
            }
            return View("Index", modelData);
        }

        //Login
        [HttpPost("login")]
        public IActionResult TryLogin(IndexViewModel modelData)
        {
            LoginPlayer logPlayer = modelData.LogPlayer;
            if (ModelState.IsValid)
            {
                Player PlayerInDb = dbContext.Players.FirstOrDefault(p => p.Username == logPlayer.Username);

                if (PlayerInDb == null)
                {
                    ModelState.AddModelError("LogPlayer.Username", "Invalid Username/Password");
                }
                else
                {
                    var hasher = new PasswordHasher<LoginPlayer>();
                    var result = hasher.VerifyHashedPassword(logPlayer, PlayerInDb.Password, logPlayer.Password);

                    if (result == 0)
                    {
                        ModelState.AddModelError("LogPlayer.Username", "Invalid Username/Password");
                    }
                    else
                    {
                        HttpContext.Session.SetObjectAsJson("ThisPlayer", PlayerInDb);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View("Index", modelData);
        }

        //DashBoard
        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            //If a player is not in session (ie. NOT Logged In), then send them back the Index page
            if (HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer") == null)
            {
                return RedirectToAction("Index");
            }

            //If the Dealer does not have a Hand in session, then create one
            if (HttpContext.Session.GetObjectFromJson<Hand>("DealerHand") == null)
            {
                HttpContext.Session.SetObjectAsJson("DealerHand", new Hand());
            }
            Hand DealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            ViewBag.DealerHand = DealerHand;

            ViewBag.Message = HttpContext.Session.GetString("message");

            ViewBag.MessageSplit = HttpContext.Session.GetString("messagesplit");

            ViewBag.Stand = HttpContext.Session.GetString("Stand");

            ViewBag.Split = HttpContext.Session.GetString("SplitHand");

            ViewBag.CurrHand = HttpContext.Session.GetString("CurrHandStand");

            ViewBag.BetAmount = HttpContext.Session.GetInt32("CurrBetAmnt");

            ViewBag.EndGame = HttpContext.Session.GetString("Endgame");

            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            ViewBag.ThisPlayer = thisPlayer;

            Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);

            double WinRatio;
            if (RetrievedPlayer.HandsPlayed < 1)
            {
                WinRatio = 0;
            }
            else
            {
                WinRatio = (double)RetrievedPlayer.HandsWon/RetrievedPlayer.HandsPlayed;
            }
            string sWinRate = WinRatio.ToString("P", CultureInfo.InvariantCulture);
            ViewBag.WinRatio = sWinRate;

            if (thisPlayer.CurrHand != null)
            {
                List<string> PlayerCards = new List<string>();
                foreach (var card in thisPlayer.CurrHand.PlayerCards)
                {
                    PlayerCards.Add(card.Suit + "_" + card.Face + ".png");
                }
                ViewBag.PlayerCards = PlayerCards;
                ViewBag.PlayerHandValue = thisPlayer.CurrHand.HandValue;
            }
<<<<<<< HEAD
            if (thisPlayer.SplitHand != null)
            {
                List<string> PlayerCards = new List<string>();
                foreach (var card in thisPlayer.SplitHand.PlayerCards)
                {
                    PlayerCards.Add(card.Suit + "_" + card.Face + ".png");
                }
                ViewBag.SplitCards = PlayerCards;
            }
            Hand DealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
=======

>>>>>>> 75adbdd34a2f921130b1a8cd9fedd417c68def3f
            if (DealerHand.PlayerCards != null)
            {
                List<string> DealerCards = new List<string>();
                foreach (var dCard in DealerHand.PlayerCards)
                {
                    DealerCards.Add(dCard.Suit + "_" + dCard.Face + ".png");
                    Console.WriteLine("Dealer has " + dCard.Face + " of " + dCard.Suit);
                }
                ViewBag.DealerCards = DealerCards;
                ViewBag.DealerHandValue = DealerHand.HandValue;
            }
            // HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return View("Dashboard");
        }

        //AddBetAmount
        [HttpGet("bet/{amnt}")]
        public IActionResult AddBetAmount(int amnt)
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");

            if (HttpContext.Session.GetInt32("CurrBetAmnt") == null)
            {
                HttpContext.Session.SetInt32("CurrBetAmnt", 0);
            }

            HttpContext.Session.Remove("message");  //Clear out session message

            if (thisPlayer.Money >= HttpContext.Session.GetInt32("CurrBetAmnt") + amnt)
            {
                if (HttpContext.Session.GetInt32("CurrBetAmnt") + amnt <= 100)
                {
                    int tempCurrBet = (int)HttpContext.Session.GetInt32("CurrBetAmnt");
                    tempCurrBet += amnt;
                    HttpContext.Session.SetInt32("CurrBetAmnt", tempCurrBet);
                }
                else
                {
                    HttpContext.Session.SetString("message", "You've hit your max bet amount!");
                }
            }
            else
            {
                HttpContext.Session.SetString("message", "You can't bet that much!");
            }
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);

            return RedirectToAction("Dashboard");
        }

        //SubmitBet
        [HttpGet("submitBet")]
        public IActionResult SubmitBet()
        {
            HttpContext.Session.SetString("Endgame", "false");
            // HttpContext.Session.SetString("CurrHand", "false");
            HttpContext.Session.SetString("SplitHand", "false");
            HttpContext.Session.SetString("CurrHandStand", "false");
            HttpContext.Session.SetString("SplitHandStand", "false");

            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");

            thisPlayer.CurrHand = new Hand();
            thisPlayer.CurrHand.BetValue = (int)HttpContext.Session.GetInt32("CurrBetAmnt");
            thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
            Console.WriteLine("Money Remaining: " + thisPlayer.Money);

            Deck thisDeck = new Deck();
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");

            //Shuffle Deck three times;
            thisDeck.Shuffle(3);

            //Instantiate a List of Cards for both the Player and the Dealer
            thisPlayer.CurrHand.PlayerCards = new List<Card>();
            dealerHand.PlayerCards = new List<Card>();

            //Deal Cards from Deck
            thisPlayer.CurrHand.PlayerCards.Add(thisDeck.Deal());   //Deal the Player's first card
            dealerHand.PlayerCards.Add(thisDeck.Deal());    //Deal the Dealer's first card (face down)
            thisPlayer.CurrHand.PlayerCards.Add(thisDeck.Deal());   //Deal the Player's second card
            dealerHand.PlayerCards.Add(thisDeck.Deal());    //Deal the Dealer's second card
            foreach (var i in thisPlayer.CurrHand.PlayerCards)
            {
                Console.WriteLine("Player has " + i.Face + " of " + i.Suit);

            }
            thisPlayer.CurrHand.CalculateHandValue();
            Console.WriteLine("Amount of cards in deck " + thisDeck.Cards.Count);
            Console.WriteLine("Current hand value is " + thisPlayer.CurrHand.HandValue);

            HttpContext.Session.Remove("message");  //Clear out session message

            //Check if player has BlackJack
            //If the players' cards add up to 21 and they only have 2 cards, then they win with a BlackJack
            if (thisPlayer.CurrHand.HandValue == 21 && thisPlayer.CurrHand.PlayerCards.Count == 2)
            {
                Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
                RetrievedPlayer.HandsPlayed++;
                RetrievedPlayer.HandsWon++;
                thisPlayer.Money += (thisPlayer.CurrHand.BetValue * 3);
                Console.WriteLine("Money Remaining: " + thisPlayer.Money);
                RetrievedPlayer.Money += (thisPlayer.CurrHand.BetValue * 2);
                dbContext.SaveChanges();

                HttpContext.Session.SetString("message", "You win with a BlackJack!");
                HttpContext.Session.SetString("Endgame", "true");
            }

            HttpContext.Session.SetObjectAsJson("CurrentDeck", thisDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);

            return RedirectToAction("Dashboard");
        }

        //Hit
        [HttpGet("hit")]
        public IActionResult Hit()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);

            thisPlayer.CurrHand.PlayerCards.Add(currDeck.Deal());

            thisPlayer.CurrHand.CalculateHandValue();
            Console.WriteLine("Current hand value is " + thisPlayer.CurrHand.HandValue);

            if ((thisPlayer.CurrHand.HandValue > 21 && HttpContext.Session.GetString("SplitHand") == "false"))
            {
                Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
                RetrievedPlayer.HandsPlayed++;
                RetrievedPlayer.Money -= thisPlayer.CurrHand.BetValue;
                dbContext.SaveChanges();
                HttpContext.Session.SetString("message", "Sorry, you busted and you lose your bet!");
                HttpContext.Session.SetString("Endgame", "true");
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("Dashboard");
            }
            if ((thisPlayer.CurrHand.HandValue > 21 && HttpContext.Session.GetString("SplitHand") == "true"))
            {
                Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
                RetrievedPlayer.HandsPlayed++;
                RetrievedPlayer.Money -= thisPlayer.CurrHand.BetValue;
                dbContext.SaveChanges();
                HttpContext.Session.SetString("message", "Sorry, you busted and you lose your bet!");
                HttpContext.Session.SetString("CurrHandStand", "true");
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("Dashboard");
            }
            HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);

            return RedirectToAction("Dashboard");
        }

        //Double
        [HttpGet("double")]
        public IActionResult Double()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);

            if (thisPlayer.Money - thisPlayer.CurrHand.BetValue >= 0)
            {
                thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                Console.WriteLine("Money Remaining: " + thisPlayer.Money);
                thisPlayer.CurrHand.BetValue = thisPlayer.CurrHand.BetValue * 2;
                HttpContext.Session.SetInt32("CurrBetAmnt", thisPlayer.CurrHand.BetValue);
                thisPlayer.CurrHand.PlayerCards.Add(currDeck.Deal());
                thisPlayer.CurrHand.CalculateHandValue();
                if (thisPlayer.CurrHand.HandValue > 21 && HttpContext.Session.GetString("SplitHand") == "false")
                {
                    Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
                    RetrievedPlayer.HandsPlayed++;
                    RetrievedPlayer.Money -= (thisPlayer.CurrHand.BetValue * 2);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetString("message", "Sorry, you busted and you lose your bet!");
                    HttpContext.Session.SetString("Endgame", "true");
                    HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                    HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                    HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                    return RedirectToAction("Dashboard");
                }
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("StandCurrHand");
            }
            else
            {
                HttpContext.Session.SetString("message", "You don't have enough to double!");
            }

            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            return RedirectToAction("StandCurrHand");
        }
        //Split
        [HttpGet("Split")]
        public IActionResult Split()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);
            if (thisPlayer.Money - thisPlayer.CurrHand.BetValue >= 0)
            {
                thisPlayer.SplitHand = new Hand();
                thisPlayer.SplitHand.PlayerCards = new List<Card>();
                HttpContext.Session.SetString("SplitHand", "true");
                thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                Console.WriteLine("Money Remaining: " + thisPlayer.Money);
                thisPlayer.SplitHand.BetValue = thisPlayer.CurrHand.BetValue;

                Card tempcard = thisPlayer.CurrHand.PlayerCards[1];
                thisPlayer.CurrHand.PlayerCards.RemoveAt(1);

                thisPlayer.SplitHand.PlayerCards.Add(tempcard);
                thisPlayer.CurrHand.PlayerCards.Add(currDeck.Deal());
                thisPlayer.SplitHand.PlayerCards.Add(currDeck.Deal());
                thisPlayer.CurrHand.CalculateHandValue();
                thisPlayer.SplitHand.CalculateHandValue();
                Console.WriteLine("Current hand value is " + thisPlayer.CurrHand.HandValue);
                Console.WriteLine("Split hand value is " + thisPlayer.SplitHand.HandValue);

                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("Dashboard");
            }
            else
            {
                HttpContext.Session.SetString("message", "You don't have enough to Split!");
            }
            HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return RedirectToAction("Dashboard");
        }
        //StandCurrHand
        [HttpGet("StandCurrHand")]
        public IActionResult StandCurrHand()
        {
            HttpContext.Session.SetString("CurrHandStand", "true");
            if (HttpContext.Session.GetString("SplitHand") == "true")
            {
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("DealerLogic");
        }
        //StandSplitHand
        [HttpGet("StandSplitHand")]
        public IActionResult StandSplitHand()
        {
            HttpContext.Session.SetString("SplitHandStand", "true");
            return RedirectToAction("DealerLogic");
        }
        //HitSplit
        [HttpGet("hitsplit")]
        public IActionResult HitSplit()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);

            thisPlayer.SplitHand.PlayerCards.Add(currDeck.Deal());

            thisPlayer.SplitHand.CalculateHandValue();
            Console.WriteLine("Split hand value is " + thisPlayer.SplitHand.HandValue);

            if (thisPlayer.SplitHand.HandValue > 21)
            {
                Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
                RetrievedPlayer.HandsPlayed++;
                RetrievedPlayer.Money -= thisPlayer.SplitHand.BetValue;
                dbContext.SaveChanges();
                HttpContext.Session.SetString("messagesplit", "Split Hand: Sorry, you busted and you lose your bet!");
                HttpContext.Session.SetString("Endgame", "true");
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("Dashboard");
            }
            HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return RedirectToAction("Dashboard");
        }
        //DoubleSplit
        [HttpGet("doublesplit")]
        public IActionResult DoubleSplit()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);

            if (thisPlayer.Money - thisPlayer.SplitHand.BetValue >= 0)
            {
                thisPlayer.Money -= thisPlayer.SplitHand.BetValue;
                Console.WriteLine("Money Remaining: " + thisPlayer.Money);
                thisPlayer.SplitHand.BetValue = thisPlayer.SplitHand.BetValue * 2;
                thisPlayer.SplitHand.PlayerCards.Add(currDeck.Deal());
         
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
                HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
                HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
                return RedirectToAction("StandSplitHand");
            }
            else
            {
                HttpContext.Session.SetString("message", "You don't have enough to double!");
            }

            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            return RedirectToAction("Dashboard");
        }
        //DealerLogic
        [HttpGet("DealerLogic")]
        public IActionResult DealerLogic()
        {
            Deck currDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");
            currDeck.Cards.RemoveRange(0, 52);

            dealerHand.CalculateHandValue();

            while (dealerHand.HandValue < 17)
            {
                dealerHand.PlayerCards.Add(currDeck.Deal());
                dealerHand.CalculateHandValue();
            }

            HttpContext.Session.SetString("Stand", "true");

            HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            return RedirectToAction("DetermineWinner");
        }

        //DetermineWinner
        [HttpGet("DetermineWinner")]
        public IActionResult DetermineWinner()
        {
            HttpContext.Session.Remove("message");  //Clear out session message

            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            Player RetrievedPlayer = dbContext.Players.FirstOrDefault(p => p.Username == thisPlayer.Username);
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");

            //Calculate the values of both hands; just for good measure
            thisPlayer.CurrHand.CalculateHandValue();
            dealerHand.CalculateHandValue();
            if (HttpContext.Session.GetString("CurrHandStand") == "true")
            {
                RetrievedPlayer.HandsPlayed++;

                //If the player's cards add up to more than 21, then the player busts and they lose their bet

                if (thisPlayer.CurrHand.HandValue == 21 && thisPlayer.CurrHand.PlayerCards.Count == 2)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.CurrHand.BetValue * 2;
                    HttpContext.Session.SetString("message", "You win with a BlackJack!");
                }
                else if (thisPlayer.CurrHand.HandValue > 21)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    RetrievedPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    HttpContext.Session.SetString("message", "Sorry, you busted and you lose your bet!");
                }
                //If the player's cards add up to 21 or less, and the dealer busts, then the player wins
                else if (thisPlayer.CurrHand.HandValue <= 21 && dealerHand.HandValue > 21)
                {
                    RetrievedPlayer.HandsWon++;
                    thisPlayer.Money += (thisPlayer.CurrHand.BetValue * 2);
                    Console.WriteLine("Money Remaining After Win: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.CurrHand.BetValue;
                    HttpContext.Session.SetString("message", "You beat the dealer, as they have busted!");
                }
                //If neither busts, and the player's cards are more than the dealer's cards, then the player wins
                else if (thisPlayer.CurrHand.HandValue > dealerHand.HandValue)
                {
                    RetrievedPlayer.HandsWon++;
                    thisPlayer.Money += (thisPlayer.CurrHand.BetValue * 2);
                    Console.WriteLine("Money Remaining After Win: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.CurrHand.BetValue;
                    HttpContext.Session.SetString("message", "You beat the dealer!");
                }
                //If neither busts, and the player's cards are equal to the dealer's cards, then nobody wins
                else if (thisPlayer.CurrHand.HandValue == dealerHand.HandValue)
                {
                    RetrievedPlayer.HandsPushed++;
                    thisPlayer.Money += thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Push: " + thisPlayer.Money);
                    HttpContext.Session.SetString("message", "You tied the dealer, the hand is a push!");
                }
                //If neither busts, and the player's cards are less than the dealer's cards, then the dealer wins
                else if (thisPlayer.CurrHand.HandValue < dealerHand.HandValue)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    RetrievedPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    HttpContext.Session.SetString("message", "Sorry, dealer wins and you lose your bet!");
                }
            }
            if (HttpContext.Session.GetString("SplitHandStand") == "true")
            {
                RetrievedPlayer.HandsPlayed++;

                //If the player's cards add up to more than 21, then the player busts and they lose their bet
                if (thisPlayer.SplitHand.HandValue == 21 && thisPlayer.SplitHand.PlayerCards.Count == 2)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.SplitHand.BetValue * 2;
                    HttpContext.Session.SetString("messagesplit", "You win with a BlackJack!");
                }
                if (thisPlayer.SplitHand.HandValue > 21)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    RetrievedPlayer.Money -= thisPlayer.SplitHand.BetValue;
                    HttpContext.Session.SetString("messagesplit", "Split Hand: Sorry, you busted and you lose your bet!");
                }
                //If the player's cards add up to 21 or less, and the dealer busts, then the player wins
                else if (thisPlayer.SplitHand.HandValue <= 21 && dealerHand.HandValue > 21)
                {
                    RetrievedPlayer.HandsWon++;
                    thisPlayer.Money += (thisPlayer.SplitHand.BetValue * 2);
                    Console.WriteLine("Money Remaining After Win: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.SplitHand.BetValue;
                    HttpContext.Session.SetString("messagesplit", "Split Hand: You beat the dealer, as they have busted!");
                }
                //If neither busts, and the player's cards are more than the dealer's cards, then the player wins
                else if (thisPlayer.SplitHand.HandValue > dealerHand.HandValue)
                {
                    RetrievedPlayer.HandsWon++;
                    thisPlayer.Money += (thisPlayer.SplitHand.BetValue * 2);
                    Console.WriteLine("Money Remaining After Win: " + thisPlayer.Money);
                    RetrievedPlayer.Money += thisPlayer.SplitHand.BetValue;
                    HttpContext.Session.SetString("messagesplit", "Split Hand: You beat the dealer!");
                }
                //If neither busts, and the player's cards are equal to the dealer's cards, then nobody wins
                else if (thisPlayer.SplitHand.HandValue == dealerHand.HandValue)
                {
                    RetrievedPlayer.HandsPushed++;
                    thisPlayer.Money += thisPlayer.SplitHand.BetValue;
                    Console.WriteLine("Money Remaining After Push: " + thisPlayer.Money);
                    HttpContext.Session.SetString("messagesplit", "Split Hand: You tied the dealer, the hand is a push!");
                }
                //If neither busts, and the player's cards are less than the dealer's cards, then the dealer wins
                else if (thisPlayer.SplitHand.HandValue < dealerHand.HandValue)
                {
                    // thisPlayer.Money -= thisPlayer.CurrHand.BetValue;
                    RetrievedPlayer.Money -= thisPlayer.SplitHand.BetValue;
                    Console.WriteLine("Money Remaining After Loss: " + thisPlayer.Money);
                    HttpContext.Session.SetString("messagesplit", "Split Hand: Sorry, dealer wins and you lose your bet!");
                }
            }
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Player's Hands Won: " + RetrievedPlayer.HandsWon);
            Console.WriteLine("Player's Hands Pushed: " + RetrievedPlayer.HandsPushed);
            Console.WriteLine("Player's Hands Played: " + RetrievedPlayer.HandsPlayed);
            Console.WriteLine("Player's Money: " + RetrievedPlayer.Money);
            //Commit the Player's HandsPlayed, HandsWon, and Money to the database
            Console.WriteLine("Saving Player to DB");
            dbContext.SaveChanges();

            HttpContext.Session.SetString("Endgame", "true");
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            return RedirectToAction("Dashboard");
        }

        //EndGame
        [HttpGet("endgame")]
        public IActionResult Endgame()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            HttpContext.Session.Remove("CurrentDeck");
            HttpContext.Session.Remove("DealerHand");
            HttpContext.Session.Remove("Stand");
            HttpContext.Session.Remove("CurrBetAmnt");

            thisPlayer.CurrHand = null;
            thisPlayer.SplitHand = null;
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);


            return RedirectToAction("Dashboard");
        }

        //Leaderboard
        [HttpGet("leaderboard")]
        public IActionResult Leaderboard()
        {
            List<Player> AllPlayers = dbContext.Players.ToList();
            ViewBag.AllPlayers = AllPlayers;
            return View("Leaderboard");
        }

        //Logout
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }

    public static class SessionExtensions
    {
        // We can call ".SetObjectAsJson" just like our other session set methods, by passing a key and a value
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            // This helper function simply serializes the object to JSON and stores it as a string in session
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // generic type T is a stand-in indicating that we need to specify the type on retrieval
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            string value = session.GetString(key);
            // Upon retrieval the object is deserialized based on the type we specified
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}