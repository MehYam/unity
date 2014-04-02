public class Hero
{
    public sealed class DeployedCards
    {
        private readonly Card3[] minions = new Card3[7];
    }

    public int health { get; set; }
    public int mana { get; set; }
    public Deck deck
    {
        get;
        set;
    }

    public DeployedCards deployment
    {
        get;
        private set;
    }
}
