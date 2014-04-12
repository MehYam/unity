using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using HST.Util;

namespace HST.Game
{
    public class AbstractCard
    {
        public readonly string id;
        public readonly string name;
        public readonly string text;
        public readonly int cost;
        //public readonly ReadOnlyCollection<IEffect> effects;

        public AbstractCard(string id, string name, string text, int cost)
        {
            this.id = id;
            this.name = name;
            this.text = text;
            this.cost = cost;
        }

        public override string ToString()
        {
            return string.Format("{0,20},{1,2},{3,8}, {2}", name, cost, text, id);
        }
    }

    public sealed class MinionCard : AbstractCard
    {
        //public readonly Action<Game, ICharacter> battlecry.

        public readonly Minion template;   // we'll call Clone off this minion instance when we want to create one.
        public MinionCard(string id, string name, string text, int cost, Minion template)
            : base(id, name, text, cost)
        {
            this.template = template;
        }

        public void Play(Game game, Hero hero, int indexFromCenter)
        {
            hero.field.AddMinionAt(indexFromCenter, template.Clone());

            //KAI: battlecry will complicate this somewhat.

            GlobalGameEvent.Instance.FireCardPlayComplete(game, this);
        }
    }
    public sealed class SpellCard : AbstractCard
    {
        public enum TARGET { NONE, MINION, HERO, CHARACTER };
        public enum TARGET_AFFINITY { NONE, FRIENDLY_ONLY, ENEMY_ONLY };

        public readonly TARGET targeting;
        public readonly TARGET_AFFINITY affinity;
        readonly Action<Game, ICharacter> spellAction;
        public SpellCard(string id, string name, string text, int cost) : base(id, name, text, cost)
        {
            targeting = TARGET.MINION;
            affinity = TARGET_AFFINITY.NONE;
            spellAction = SpellFactory.Instance.GetSpellAction(name);
        }
        public void Play(Game game, ICharacter target)
        {
            if (spellAction != null)
            {
                spellAction(game, target);
            }
            GlobalGameEvent.Instance.FireCardPlayComplete(game, this);
        }
    }
    public sealed class WeaponCard : AbstractCard
    {
        public WeaponCard(string id, string name, string text, int cost) : base(id, name, text, cost) { }
    }
    public sealed class HeroPowerCard : AbstractCard
    {
        public HeroPowerCard(string id, string name, string text, int cost) : base(id, name, text, cost) { }
    }
    public class RecostedCard<T> where T : AbstractCard
    {
        public readonly int costDelta;
        public RecostedCard(int costDelta) { this.costDelta = costDelta; }
    }

    public class CardFactory
    {
        CardFactory() { } // hide constructor

        static CardFactory _instance;
        static public CardFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardFactory();
                }
                return _instance;
            }
        }

        enum CardType { MINION = 4, SPELL = 5, WEAPON = 7, HERO_POWER = 10 };

        IList<AbstractCard> _cards = new List<AbstractCard>();
        Dictionary<string, AbstractCard> _cardLookupByName = new Dictionary<string, AbstractCard>();
        Dictionary<string, AbstractCard> _cardLookupByFileID = new Dictionary<string, AbstractCard>();

        static readonly string THE_COIN = "GAME_005";
        public void LoadCards(string jsonTextCards, string jsonTextCardSupplemental)
        {
            Action<AbstractCard> AddCard = (card) =>
            {
                if (card != null)
                {
                    _cards.Add(card);
                    _cardLookupByName[card.name] = card;
                    _cardLookupByFileID[card.id] = card;
                }
            };

            // create the coin manually, it doesn't show up in our json because the card xml gives it no cost
            AddCard(new SpellCard(THE_COIN, "The Coin", "Gain 1 Mana Crystal this turn only.", 0));

            var jsonCards = MJSON.hashtableFromJson(jsonTextCards);
            //var jsonCardSupplemental = MJSON.hashtableFromJson(jsonTextCardSupplemental);

            var cardsNode = (ArrayList)MJSON.getNode(jsonCards, "cards");
            foreach (Hashtable card in cardsNode)
            {
                // not a lot of error checking here, this is a file that's not apt to change
                // without a lot of testing first, so we'll play this fast and loose.
                var cardID  = MJSON.SafeGetValue(card, "id");
                var name = MJSON.SafeGetValue(card, "name");
                var cost = MJSON.SafeGetInt(card, "cost");
                var cardType = (CardType)MJSON.SafeGetInt(card, "type");
                var text = MJSON.SafeGetValue(card, "text");
                var targetingText = MJSON.SafeGetValue(card, "targetingText");

                var atk = MJSON.SafeGetInt(card, "atk");
                var freeze = MJSON.SafeGetBool(card, "freeze");

                IList<IEffect> effects = new List<IEffect>();
                AbstractCard newCard = null;
                switch (cardType)
                {
                    case CardType.MINION:
                        var health = MJSON.SafeGetInt(card, "health");
                        var battlecry = MJSON.SafeGetBool(card, "battlecry");
                        var taunt = MJSON.SafeGetBool(card, "taunt");
                        var stealth = MJSON.SafeGetBool(card, "stealth");
                        var charge = MJSON.SafeGetBool(card, "charge");
                        var divineShield = MJSON.SafeGetBool(card, "divineShield");
                        var windfury = MJSON.SafeGetBool(card, "windfury");
                        var combo = MJSON.SafeGetBool(card, "combo");
                        var overload = MJSON.SafeGetBool(card, "overload");
                        var spellpower = MJSON.SafeGetBool(card, "spellpower");

                        var minionTemplate = new Minion(name, atk, health, null);
                        minionTemplate.charge = charge;
                        minionTemplate.divineShield = divineShield;
                        minionTemplate.stealth = stealth;
                        minionTemplate.taunt = taunt;
                        minionTemplate.windfury = windfury;

                        newCard = new MinionCard(cardID, name, text, cost, minionTemplate);
                        break;
                    case CardType.SPELL:
                        //KAI: the card xml has "ReferencedTag", something we should possibly use here
                        var secret = MJSON.SafeGetBool(card, "secret");
                        newCard = new SpellCard(cardID, name, text, cost);
                        break;
                    case CardType.WEAPON:
                        var durability = MJSON.SafeGetInt(card, "durability");
                        newCard = new WeaponCard(cardID, name, text, cost);
                        break;
                    case CardType.HERO_POWER:
                        newCard = new HeroPowerCard(cardID, name, text, cost);
                        break;
                }
                AddCard(newCard);
            }
        }

        public int NumCards { get { return _cards.Count; } }
        public AbstractCard GetCard(int index)
        {
            return _cards[index];
        }
        public AbstractCard GetCard(string cardName)
        {
            return _cardLookupByName[cardName];
        }
        public AbstractCard GetCardByID(string cardID)
        {
            return _cardLookupByFileID[cardID];
        }
        public AbstractCard GetCoin()
        {
            return GetCardByID(THE_COIN);
        }
    }
}
