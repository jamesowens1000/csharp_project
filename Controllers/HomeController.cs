using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using csharp_project.Models;
using Microsoft.EntityFrameworkCore;

namespace csharp_project.Controllers
{
    public class HomeController : Controller
    {
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
    }
}