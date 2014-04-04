using System;
using System.Collections.Generic;
using System.Text;

using HST.Util;

public sealed class Tests
{
    readonly ITestLogger logger;
    public Tests(ITestLogger logger)
    {
        this.logger = logger;
    }
    public void RunAll()
    {
        TestLanguage();
        TestDeck();
        TestGame();
    }

    void TestLanguage()
    {
        // getting an enumerator part way through an array and looping it
        var array = new int[] { 1, 2, 3, 4, 5 };
        var e = array.GetEnumerator();
    }

    static void PopulateRandomDeck(Deck<AbstractCard> deck)
    {
        for (int i = 0; i < Game.DECKSIZE; ++i)
        {
            deck.SetCardAt(i, CardFactory.Instance.CreateRandomMinionCard());
        }
    }

    void TestDeck()
    {
        var deck = new Deck<AbstractCard>(Game.DECKSIZE);
        PopulateRandomDeck(deck);

        // test shuffle
        deck.Shuffle();
    }

    void TestGame()
    {
        // start game
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );

        PopulateRandomDeck(game.hero[0].deck);
        PopulateRandomDeck(game.hero[1].deck);

        // test mulligan
        game.DrawForMulligan();

        DebugUtils.Assert(game.hero[0].hand.size == Game.INITIAL_DRAW);
        DebugUtils.Assert(game.hero[1].hand.size == Game.INITIAL_DRAW + 1);

        // randomly mulligan a few cards
        var rnd = new System.Random();
        Func<Hand<AbstractCard>, Hand<AbstractCard>> pickRandom = (hand) =>
        {
            var toMulligan = new Hand<AbstractCard>();
            foreach (var card in hand)
            {
                if (rnd.Next(1, 2) == 1)
                {
                    toMulligan.AddCard(card);
                }
            }
            return toMulligan;
        };


        game.hero[0].Mulligan(pickRandom(game.hero[0].hand));
        game.hero[1].Mulligan(pickRandom(game.hero[1].hand));

        game.OnPostMulligan();

        DebugUtils.Assert(game.hero[0].hand.size == Game.INITIAL_DRAW);
        DebugUtils.Assert(game.hero[1].hand.size == Game.INITIAL_DRAW + 2);

        logger.Log("TestGame", game.ToString());
    }
}
