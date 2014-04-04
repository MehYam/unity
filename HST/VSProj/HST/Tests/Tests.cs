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

    static void PopulateRandomDeck(Deck<AbstractCard> deck)
    {
        for (int i = 0; i < Game.DECKSIZE; ++i)
        {
            deck.SetCardAt(i, AbilityFactory.Instance.CreateRandomMinionCard());
        }
    }

    void TestDeck()
    {
        var deck = new Deck<AbstractCard>(Game.DECKSIZE);
        PopulateRandomDeck(deck);

        // test shuffle
        deck.Shuffle();
    }

    void TestGameSimple()
    {
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );

        PopulateRandomDeck(game.hero[0].deck);
        PopulateRandomDeck(game.hero[1].deck);

        game.Start();

        logger.Log("TestGameSimple", game.ToString());
    }
}
