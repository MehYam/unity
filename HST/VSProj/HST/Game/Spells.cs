using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HST.Util;

namespace HST.Game
{
    public sealed class SpellFactory
    {
        static public readonly SpellFactory Instance = new SpellFactory();


        public Action<Game, ICharacter> GetSpellAction(string spellName)
        {
            Action<Game, ICharacter> action = null;
            spellActions.TryGetValue(spellName, out action);
            return action;
        }

        readonly Dictionary<string, Action<Game, ICharacter>> spellActions = new Dictionary<string, Action<Game, ICharacter>>();
        void AddSpell(string name, Action<Game, ICharacter> action)
        {
            spellActions[name] = action;
        }
        SpellFactory() 
        {
            //TODO: all the spell damages need to get modified by any +spell dmg minions
            AddSpell("The Coin", (game, target) =>
            {
                var hero = game.turnHero;
                
                //TODO: add a one-turn modifier to the hero that increases his mana and mana crystals by one
            });
            AddSpell("Frostbolt", (game, target) =>
            {
                target.ReceiveAttack(3);
                target.frozen = true;
            });
            AddSpell("Redemption", (game, target) =>
            {
                //TODO: add a secret to the paladin
            });
            AddSpell("Divine Favor", (game, target) =>
            {
                //TODO: what does this do in the event of a deck running out?
                var toDraw = game.turnDefender.hand.size - game.turnHero.hand.size;
                if (toDraw > 0)
                {
                    game.turnHero.Draw(toDraw);
                }
            });
            AddSpell("Consecration", (game, target) =>
            {
                var attack = 2;
                var defender = game.turnDefender;
                foreach (var minion in defender.field)
                {
                    minion.ReceiveAttack(attack);
                }
                game.turnDefender.ReceiveAttack(2);
            });
            AddSpell("Innervate", (game, target) =>
            {
                var hero = game.turnHero;
                //TODO: add a one-turn modifier to the hero that increases his mana and mana crystals by two

            });
            AddSpell("Claw", (game, target) =>
            {
                //TODO: give the druid +2 atk and shields
            });
            AddSpell("Mark of the Wild", (game, target) =>
            {
                var minion = target as Minion;
                //TODO: give the minion +2/+2

                minion.taunt = true;
            });
            AddSpell("Wild Growth", (game, target) =>
            {
                ++game.turnHero.crystals;
            });
            AddSpell("Swipe", (game, target) =>
            {
                //TODO: this assumes target = defender, can target self with this spell
                target.ReceiveAttack(4);
                foreach (var minion in game.turnDefender.field)
                {
                    if (target != minion)
                    {
                        minion.ReceiveAttack(1);
                    }
                }
                if (target != game.turnDefender)
                {
                    game.turnDefender.ReceiveAttack(1);
                }
            });
            AddSpell("Starfire", (game, target) =>
            {
                target.ReceiveAttack(5);
                game.turnHero.Draw(1);
            });
            AddSpell("placeholder", (game, target) =>
            {
            });
            AddSpell("placeholder", (game, target) =>
            {
            });
            AddSpell("placeholder", (game, target) =>
            {
            });
            AddSpell("placeholder", (game, target) =>
            {
            });
        }
    }
}
