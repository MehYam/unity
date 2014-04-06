using System;
using System.Collections.Generic;
using System.Text;

using HST.Game;
using HST.Util;

public sealed class Tests
{
    readonly ILogger logger;
    static readonly System.Random rnd = new System.Random();

    public Tests(ILogger logger)
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

    static void PopulateRandomDeck(Deck<Card4> deck)
    {
        for (int i = 0; i < Game.DECKSIZE; ++i)
        {
            var minion = (MinionFactory.MinionID)rnd.Next(0, (int)MinionFactory.MinionID.MAX);
            var card = CardFactory.Instance.CreateMinionCard(minion);

            deck.SetCardAt(i, card);
        }
    }

    void TestDeck()
    {
        var deck = new Deck<Card4>(Game.DECKSIZE);
        PopulateRandomDeck(deck);

        // test shuffle
        deck.Shuffle();
    }

    void TestGame()
    {
        // start game
        Game.DECKSIZE = 10;
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );

        PopulateRandomDeck(game.heros[0].deck);
        PopulateRandomDeck(game.heros[1].deck);

        // test mulligan
        game.DrawForMulligan();

        logger.Log("TestGame pre-mulligan " + game.ToStringBrief());

        DebugUtils.Assert(game.heros[0].hand.size == Game.INITIAL_DRAW);
        DebugUtils.Assert(game.heros[1].hand.size == Game.INITIAL_DRAW + 1);

        // randomly mulligan a few cards
        Func<Hand<Card4>, Hand<Card4>> pickRandom = (hand) =>
        {
            var toMulligan = new Hand<Card4>();
            foreach (var card in hand)
            {
                if (rnd.Next(0, 2) == 1)
                {
                    toMulligan.AddCard(card);
                }
            }
            return toMulligan;
        };

        var mully = pickRandom(game.heros[0].hand);
        logger.Log("TestGame hero0 mullies " + mully.size.ToString());
        game.heros[0].Mulligan(mully);

        mully = pickRandom(game.heros[1].hand);
        game.heros[1].Mulligan(mully);
        logger.Log("TestGame hero1 mullies " +  mully.size.ToString());

        game.OnPostMulligan();

        logger.Log("TestGame post-mulligan " + game.ToStringBrief());

        // play some turns
        for (int turn = 0; turn < 4; ++turn)
        {
            PlayRandomCard(game);

            logger.Log(string.Format("Turn {0} -------", game.turnNumber));

            game.NextTurn();
        }
    }

    void PlayRandomCard(Game game)
    {
        var cards = game.turnHero.hand;
        var card = cards.CardAt(rnd.Next(0, cards.size));

        Action<Hero> minionPositionNeeded = (hero) =>
        {
            GlobalGameEvent.Instance.FireMinionPositionChosen(hero, 0);
        };
        GlobalGameEvent.Instance.MinionPositionNeeded += minionPositionNeeded;

        card.Play(game);

        logger.Log(string.Format("{0} played random card {1}", game.turnHero.heroClass.ToString(), card.id));
        logger.Log(game.turnHero.ToStringBrief());

        GlobalGameEvent.Instance.MinionPositionNeeded -= minionPositionNeeded;
    }
}
