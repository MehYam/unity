using System;
using System.Collections;
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
        TestCSharp();

        LoadCards();
        TestDeck();
        TestGame();
    }

    void TestCSharp()
    {
        // getting an enumerator part way through an array and looping it
        var array = new int[] { 1, 2, 3, 4, 5 };
        var e = array.GetEnumerator();
    }

    static void PopulateRandomDeck(Deck<AbstractCard> deck)
    {
        for (int i = 0; i < Game.DECKSIZE; ++i)
        {
            var card = CardFactory.Instance.GetCard(rnd.Next(0, CardFactory.Instance.NumCards));

            deck.SetCardAt(i, card);
        }
    }

    void TestDeck()
    {
        var deck = new Deck<AbstractCard>(Game.DECKSIZE);
        PopulateRandomDeck(deck);

        // test shuffle
        deck.Shuffle();
    }

    static string LoadFile(string filename)
    {
        using (var file = new System.IO.StreamReader(filename))
        {
            string text = file.ReadToEnd();
            return text;
        }
    }
    static void LoadCards()
    {
        string jsonText = LoadFile("C:\\source\\unity\\HST\\Assets\\cards.json");
        string jsonTextSupplemental = LoadFile("C:\\source\\unity\\HST\\Assets\\cards_supplemental.json");

        CardFactory.Instance.LoadCards(jsonText, jsonTextSupplemental);
    }
    static void LoadDeck(string filename, Deck<AbstractCard> deck)
    {
        using (var cards = new System.IO.StreamReader(filename))
        {
            string jsonText = cards.ReadToEnd();
            var json = MJSON.hashtableFromJson(jsonText);

            var cardsNode = (ArrayList)MJSON.getNode(json, "cards");
            int i = 0;
            foreach (string cardName in cardsNode)
            {
                deck.SetCardAt(i++, CardFactory.Instance.GetCard(cardName));
            }
        }
    }


    void TestGame()
    {
        //Game.DECKSIZE = 10;
        var game = new Game(
            Hero.CreateHero(Hero.CLASS.DRUID),
            Hero.CreateHero(Hero.CLASS.PALADIN)
        );
        LoadDeck("c:\\source\\unity\\HST\\VSProj\\HST\\testdeck_druid.json", game.heros[0].deck);
        LoadDeck("c:\\source\\unity\\HST\\VSProj\\HST\\testdeck_paladin.json", game.heros[1].deck);

        // test mulligan
        game.DrawForMulligan();

        logger.Log("TestGame pre-mulligan\n" + game.ToString());

        DebugUtils.Assert(game.heros[0].hand.size == Game.INITIAL_DRAW);
        DebugUtils.Assert(game.heros[1].hand.size == Game.INITIAL_DRAW + 1);

        // mulligan cards costing more than 3
        Func<Hand<AbstractCard>, Hand<AbstractCard>> pickRandom = (hand) =>
        {
            var toMulligan = new Hand<AbstractCard>();
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

        logger.Log("TestGame post-mulligan\n" + game.ToStringBrief());

        // play some turns
        const int TURNS = 20;
        for (int plays = 0; plays < TURNS; ++plays)
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
                logger.Log(string.Format("{0} plays card {1}", game.turnHero.heroClass.ToString(), card));
                logger.Log(game.turnHero.ToStringBrief());

                if (card is MinionCard)
                {
                    ((MinionCard)card).Play(game, game.turnHero, 0);
                }
                else
                {
                }
                return;
            }
        }
        logger.Log(string.Format("{0} plays no cards", game.turnHero.heroClass.ToString()));
    }

    void AttackRandomly(Game game)
    {
        // if the hero has an awake minion, attack something with it
        
        ICharacter attacker = null;
        do
        {
            // need a double loop here since the field collection changes as we're iterating it, due
            // to minions dying and being removed
            attacker = null;
            foreach (var minion in game.turnHero.field)
            {
                if (minion.canAttack)
                {
                    attacker = minion;
                    break;
                }
            }
            if (attacker != null)
            {
                Attack(game, attacker);
            }
        } while (attacker != null);
    }
    static void Attack(Game game, ICharacter attacker)
    {
        ICharacter victim = null;
        var nDefendingMinions = game.turnDefender.field.size;

        if (nDefendingMinions > 0)
        {
            // attack a minion
            victim = game.turnDefender.field[rnd.Next(0, nDefendingMinions)];
        }
        else
        {
            // or hit face
            victim = game.turnDefender;
        }

        //game.Attack(attacker, victim);
    }

}
