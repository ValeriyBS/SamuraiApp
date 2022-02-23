namespace SamuraiApp.Domain
{
    public class SamuraiBattleStat
    {
        //No Id intentional, as so declares az .HsNoKey() in the SamuraiContext 
        public string Name { get; set; }
        public int? NumberOfBattles { get; set; }
        public string EarliestBattle { get; set; }
    }
}
