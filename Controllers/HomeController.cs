using System;
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

namespace csharp_project.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            Deck newDeck = new Deck();
            newDeck.Shuffle();
            Card FirstCard = newDeck.Deal();
            Card SecondCard = newDeck.Deal();
            ViewBag.FirstCard = FirstCard.Suit+"_"+FirstCard.Face+".png";
            ViewBag.SecondCard = SecondCard.Suit+"_"+SecondCard.Face+".png";
            return View();
        }

        [HttpPost("register")]
        public IActionResult TryRegister(Player newPlayer)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Players.Any(p => p.Username == newPlayer.Username))
                {
                    ModelState.AddModelError("Username", "Username already in use!");
                }
                else
                {
                    PasswordHasher<Player> Hasher = new PasswordHasher<Player>();
                    newPlayer.Password = Hasher.HashPassword(newPlayer, newPlayer.Password);
                    dbContext.Add(newPlayer);
                    dbContext.SaveChanges();
                    Player ThisPlayer = dbContext.Players.FirstOrDefault(u => u.Username == newPlayer.Username);
                    HttpContext.Session.SetObjectAsJson("ThisPlayer", ThisPlayer);
                    return RedirectToAction("Dashboard");
                }
            }
            return View("Index", newPlayer);
        }

        [HttpPost("login")]
        public IActionResult TryLogin(LoginPlayer logPlayer)
        {
            if (ModelState.IsValid)
            {
                Player PlayerInDb = dbContext.Players.FirstOrDefault(p => p.Username == logPlayer.Username);

                if (PlayerInDb == null)
                {
                    ModelState.AddModelError("Username", "Invalid Username/Password");
                }
                else
                {
                    var hasher = new PasswordHasher<LoginPlayer>();
                    var result = hasher.VerifyHashedPassword(logPlayer, PlayerInDb.Password, logPlayer.Password);

                    if (result == 0)
                    {
                        ModelState.AddModelError("LogUser.Email", "Invalid Email/Password");
                    }
                    else
                    {
                        HttpContext.Session.SetObjectAsJson("ThisPlayer", PlayerInDb);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View("Index", logPlayer);
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            //If a player is not in session (ie. NOT Logged In), then send them back the Index page
            if (HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer") == null)
            {
                return RedirectToAction("Index");
            }

            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            ViewBag.ThisPlayer = thisPlayer;

            //If the Dealer does not have a Hand in session, then create one
            if (HttpContext.Session.GetObjectFromJson<Hand>("DealerHand") == null)
            {
                HttpContext.Session.SetObjectAsJson("DealerHand", new Hand());
            }
            ViewBag.DealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");

            //If there is no CurrentDeck, then create a new one and shuffle it
            if (HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck") == null)
            {
                Deck currDeck = new Deck();
                currDeck.Shuffle(3);
                HttpContext.Session.SetObjectAsJson("CurrentDeck", currDeck);
            }
            ViewBag.CurrentDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");

            ViewBag.Message = HttpContext.Session.GetString("message");

            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return View("Dashboard");
        }

        [HttpGet("bet/{amnt}")]
        public IActionResult AddBetAmount(int amnt)
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");

            if (thisPlayer.Money >= thisPlayer.CurrHand.BetValue)
            {
                if (thisPlayer.CurrHand.BetValue+amnt <= 100)
                {
                    thisPlayer.CurrHand.BetValue += amnt;
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

        [HttpGet("submitBet")]
        public IActionResult SubmitBet()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            thisPlayer.Money -= thisPlayer.CurrHand.BetValue;

            //Deal Cards from Deck
            Deck thisDeck = HttpContext.Session.GetObjectFromJson<Deck>("CurrentDeck");
            Hand dealerHand = HttpContext.Session.GetObjectFromJson<Hand>("DealerHand");

            thisPlayer.CurrHand.PlayerCards.Add(thisDeck.Deal());   //Deal the Player's first card
            dealerHand.PlayerCards.Add(thisDeck.Deal());    //Deal the Dealer's first card (face down)
            thisPlayer.CurrHand.PlayerCards.Add(thisDeck.Deal());   //Deal the Player's second card
            dealerHand.PlayerCards.Add(thisDeck.Deal());    //Deal the Dealer's second card

            //Check if player has BlackJack
            //If the players' cards add up to 21 and they only have 2 cards, then they win with a BlackJack
            if (thisPlayer.CurrHand.HandValue == 21 && thisPlayer.CurrHand.PlayerCards.Count == 2)
            {
                thisPlayer.Money += (thisPlayer.CurrHand.BetValue + thisPlayer.CurrHand.BetValue*2);
                HttpContext.Session.SetString("message", "You win with a BlackJack!");
            }

            HttpContext.Session.SetObjectAsJson("CurrentDeck", thisDeck);
            HttpContext.Session.SetObjectAsJson("DealerHand", dealerHand);
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return RedirectToAction("Dashboard");
        }

        [HttpGet("stand")]
        public IActionResult Stand()
        {
            Player thisPlayer = HttpContext.Session.GetObjectFromJson<Player>("ThisPlayer");
            thisPlayer.CurrHand.CalculateHandValue();

            //If the player's cards add up to more than 21, then the player busts and they lose their bet
            if (thisPlayer.CurrHand.HandValue > 21)
            {
                HttpContext.Session.SetString("message", "Sorry, You Busted!");
            }

            //Dealer Logic

            ViewBag.ThisPlayer = thisPlayer;
            HttpContext.Session.SetObjectAsJson("ThisPlayer", thisPlayer);
            return RedirectToAction("Dashboard");
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