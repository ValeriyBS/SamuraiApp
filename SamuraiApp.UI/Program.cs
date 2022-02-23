using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.UI
{
    internal class Program
    {
        private static SamuraiContext _context = new SamuraiContext();
        private static SamuraiContextNoTracking _contextNT = new SamuraiContextNoTracking();
        static void Main(string[] args)
        {
            _context.Database.EnsureCreated();
            //GetSamurais("Before Add:");
            //AddSamurai();
            //GetSamurais("After Add:");
            //InsertNewSamuraiWithQuote();
            //AddQuoteToExistingSamuraiNotTracked(2);
            //Simpler_AddQuoteToExistingSamuraiNotTracked(2);

            //var samuraiWithQuotes = _context.Samurais
            //    .Include(s => s.Quotes)
            //    .ToList();

            //var filteredInclude = _context.Samurais
            //    .Include(s => s.Quotes.Where(q => q.Text.Contains("come"))).ToList();

            //var filterPrimaryEntityWithInclude = _context.Samurais.Where(s => s.Name.Contains("Sakoto"))
            //    .Include(s => s.Quotes)
            //    //.ThenInclude(sq => sq.Samurai) for chlids of quote 
            //    .Include(b => b.Battles)
            //    .FirstOrDefault();

            //var projectSomeProperties = _context.Samurais.Select(s => new { s.Id, s.Name, NumberOfQuotes=s.Quotes.Count  }).ToList();

            //ExplicitLoadQuotes();

            //var filteringWithRelatedData =
            //    _context.Samurais.Where(s => s.Quotes.Any(q => q.Text.Contains("happy"))).ToList();

            //AddingExistingSamuraiToExistingBattle();

            //RemoveSamuraiFromBattle();

            //RemoveSamuraiFromABattleExplicit();

            //RemoveSamuraiFromBattle();

            //AddHorseToAnExistingSamurai();

            //AddHorseToSamuraiObject();

            //QueryRelatedUsingRawSql();

            //QueryRelatedUsingRawSqlInterpolated("Yamato Takeda");

            QueryUsingFromSqlRawStoredProc("Thanks");

            Console.Write("Press any key...");
            Console.ReadKey();
        }

        private static void AddSamurai()
        {
            var samurai = new Samurai() { Name = "Nabizona Rogata" };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        private static void GetSamurais(string text)
        {
            var samurais = _context.Samurais.ToList();
            Console.WriteLine($"{text}: Samurai count is {samurais.Count}");

            foreach (var samurai in samurais)
            {
                Console.WriteLine(samurai.Name);
            }
        }

        private static void InsertNewSamuraiWithQuote()
        {
            var samurai = new Samurai
            {
                Name = "Kambei Shimada",
                Quotes = new List<Quote>
                {
                    new Quote()
                    {
                        Text = "I've come to save you"
                    }
                }
            };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();  
        }

        private static void AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var samurai = _context.Samurais.Find(samuraiId);
            samurai.Quotes.Add(new Quote{Text = "Now that I saved you, will you feed me dinner?"});

            using (var newContext = new SamuraiContext())
            {
                //newContext.Samurais.Update(samurai); //updates samurai with the same value in order to add quote!!!low performance
                newContext.Samurais.Attach(samurai); //does not update Samurai db record only adds quote
                newContext.SaveChanges();
            }
        }

        private static void Simpler_AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var quote = new Quote { Text = "Thanks for the dinner!", SamuraiId = samuraiId};

            using var newContext = new SamuraiContext();
            newContext.Quotes.Add(quote);
            newContext.SaveChanges();
        }

        private static void ExplicitLoadQuotes()
        {
            //make sure there's a horse in the DB, then clear the context's change tracker
            _context.Set<Horse>().Add(new Horse { SamuraiId = 1, Name = "Mr.Ed" });
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            //------------
            var samurai = _context.Samurais.Find(1);
            _context.Entry(samurai).Collection(s => s.Quotes).Load();
            _context.Entry(samurai).Reference(s => s.Horse).Load();
        }

        private static void MofidyingRelatedDataWhenTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes)
                .FirstOrDefault(s => s.Id == 2);
            samurai.Quotes[0].Text = "Did you hear that?";
            _context.SaveChanges();
        }

        private static void ModifyingRelatedDataWhenNotTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes)
                .FirstOrDefault(s => s.Id == 2);

            var quote = samurai.Quotes[0];
            quote.Text += "Did you hear that again?";

            using var newContext = new SamuraiContext();
            newContext.Entry(quote).State = EntityState.Modified;
            newContext.SaveChanges();
        }

        private static void AddingNewSamuraiToAnExistingBattle()
        {
            var battle = _context.Battles.FirstOrDefault();
            battle.Samurais.Add(new Samurai{ Name = "Takeda Shingen"});
            _context.SaveChanges();
        }

        private static void AddingExistingSamuraiToExistingBattle()
        {
            var battle = _context.Battles.Find(1);
            //var samurais = _context.Samurais.ToList();
            var samurais = _context.Samurais.Where(s => s.Name.Contains("Nabizona"));

            battle.Samurais.AddRange(samurais);
            _context.Battles.Update(battle);

            _context.SaveChanges();
        }

        private static void RemoveSamuraiFromBattle()
        {
            var battleSamurai = _context.Battles
                .Include(b => b.Samurais.Where(s => s.Name.Contains("Nabizona")))
                .Single(b => b.BattleId == 1);
            var samuraiToRemove = battleSamurai.Samurais[0];
            battleSamurai.Samurais.Remove(samuraiToRemove);
            _context.SaveChanges();

        }

        private static void RemoveSamuraiFromABattleExplicit()
        {
            var battleSamurai = _context.Set<BattleSamurai>()
                .SingleOrDefault(bs => bs.BattleId == 1 && bs.SamuraiId == 4);

            if (battleSamurai != null)
            {
                _context.Remove(battleSamurai); //_context.Set<BattleSamurai>().Remove works, too
                _context.SaveChanges();
            }
        }

        private static void AddHorseToAnExistingSamurai()
        {
            
                _context.Add(new Horse { Name = "Binky", SamuraiId = 4 });
                _context.SaveChanges();
        }

        private static void AddHorseToSamuraiObject()
        {
            var samurai = _context.Samurais.Find(2);
            if (samurai != null)
            {
                samurai.Horse = new Horse
                {
                    Name = "Roach"
                };
            }

            _context.SaveChanges();
        }

        private static void QueryRelatedUsingRawSql()
        {
            var samurais = _context.Samurais.FromSqlRaw("select Id,Name from Samurais")
                .Include(s => s.Horse)
                .Include(s => s.Quotes)
                .ToList();
        }

        private static void QueryRelatedUsingRawSqlInterpolated(string name)
        {
            var samurais = _context.Samurais.FromSqlInterpolated($"select Id,Name from Samurais where name={name}")
                .Include(s => s.Horse)
                .Include(s => s.Quotes)
                .ToList();
        }

        private static void QueryUsingFromSqlRawStoredProc(string text)
        {
            var samurais = _context.Samurais.FromSqlRaw("exec dbo.SamuraisWhoSaidAWord {0}", text)
                .ToList();

            var samurais1 = _context.Samurais.FromSqlInterpolated($"exec dbo.SamuraisWhoSaidAWord {text}")
                .ToList();
        }
    }
}
