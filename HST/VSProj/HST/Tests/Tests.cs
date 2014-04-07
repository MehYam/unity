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
        Logger.Impl = this.logger = logger;
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
        //Game.DECKSIZE = 10;
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );

        PopulateRandomDeck(game.heros[0].deck);
        PopulateRandomDeck(game.heros[1].deck);

        // test mulligan
        game.DrawForMulligan();

        //logger.Log("TestGame pre-mulligan " + game.ToStringBrief());

        DebugUtils.Assert(game.heros[0].hand.size == Game.INITIAL_DRAW);
        DebugUtils.Assert(game.heros[1].hand.size == Game.INITIAL_DRAW + 1);

        // mulligan cards costing more than 3
        Func<Hand<Card4>, Hand<Card4>> pickRandom = (hand) =>
        {
            var toMulligan = new Hand<Card4>();
            foreach (var card in hand)
            {
                if (card.cost > 3)
                {
                    toMulligan.AddCard(card);
                }
            }
            return toMulligan;
        };

        var mully = pickRandom(game.heros[0].hand);
        logger.Log(string.Format("TestGame {0} mullies {1}", game.heros[0].heroClass, mully.size.ToString()));
        game.heros[0].Mulligan(mully);

        mully = pickRandom(game.heros[1].hand);
        game.heros[1].Mulligan(mully);
        logger.Log(string.Format("TestGame {0} mullies {1}", game.heros[1].heroClass, mully.size.ToString()));

        game.OnPostMulligan();

        logger.Log("TestGame post-mulligan " + game.ToStringBrief());

        // play some turns
        for (int turn = 0; turn < 6; ++turn)
        {
            logger.Log(string.Format("Turn {0} -------------------------------------", game.turnNumber));
            
            PlayCardRandomly(game);
            AttackRandomly(game);
            game.NextTurn();
        }
    }

    // these ***randomly functions will turn into the move finder part of the AI
    void PlayCardRandomly(Game game)
    {
        var hand = game.turnHero.hand;

        // find a card we can afford
        foreach (var card in hand)
        {
            if (card.cost <= game.turnHero.mana)
            {
                Action<Hero> minionPositionNeeded = (hero) =>
                {
                    GlobalGameEvent.Instance.FireMinionPositionChosen(hero, 0);
                };
                GlobalGameEvent.Instance.MinionPositionNeeded += minionPositionNeeded;
                
                card.Play(game);

                GlobalGameEvent.Instance.MinionPositionNeeded -= minionPositionNeeded;

                logger.Log(string.Format("{0} plays card {1}", game.turnHero.heroClass.ToString(), card.id));
                logger.Log(game.turnHero.ToStringBrief());
                return;
            }
        }
        logger.Log(string.Format("{0} plays no cards", game.turnHero.heroClass.ToString()));
    }

    void AttackRandomly(Game game)
    {
        // if the hero has an awake minion, attack something with it
        ICharacter attacker = null;
        foreach (var minion in game.turnHero.field)
        {
            if (minion.awake)
            {
                attacker = minion;
                break;
            }
        }
        if (attacker != null)
        {
            ICharacter attackee = null;
            var nDefendingMinions = game.turnDefender.field.size;

            if (nDefendingMinions > 0)
            {
                // attack a minion
                attackee = game.turnDefender.field[rnd.Next(0, nDefendingMinions)];
            }
            else
            {
                // or hit face
                attackee = game.turnDefender;
            }

            game.Attack(attacker, attackee);
        }
        else
        {
            logger.Log(string.Format("{0} can't attack with minions", game.turnHero.heroClass.ToString()));
        }
    }
}
