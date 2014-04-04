using System.Collections.Generic;
using System.Text;

public sealed class Tests
{
    readonly ITestLogger logger;
    public Tests(ITestLogger logger)
    {
        this.logger = logger;
    }
    public void RunAll()
    {
        TestDeck();
        TestGameSimple();
    }

    void TestDeck()
    {
        var deck = new Deck();
        deck.CreateRandom();

        // test shuffle
        deck.Shuffle();
    }

    void TestGameSimple()
    {
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );

        game.Start();

        logger.Log("TestGameSimple", game.ToString());
    }
}
