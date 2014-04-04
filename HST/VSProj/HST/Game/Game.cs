using System.Collections;

public class Game
{
    static public readonly int DECKSIZE = 30;
    static public readonly int INITIAL_DRAW = 3;
    readonly Hero[] _heros = new Hero[2];
    public Hero[] hero
    {
        get
        {
            return _heros;
        }
    }

    public int turn { get; private set; }
    public Game(Hero first, Hero second)
    {
        turn = 1;

        _heros[0] = first;
        _heros[1] = second;
    }

    public void DrawForMulligan()
    {
        _heros[0].deck.Shuffle();
        _heros[1].deck.Shuffle();

        _heros[0].Draw(INITIAL_DRAW);
        _heros[1].Draw(INITIAL_DRAW + 1);
    }

    public void OnPostMulligan()
    {
        _heros[0].Draw(INITIAL_DRAW);
        _heros[1].Draw(INITIAL_DRAW + 1);
        
        _heros[1].hand.AddCard(CardFactory.Instance.CreateCoin());
    }


    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(_heros[0].ToString());
        sb.AppendLine(_heros[1].ToString());

        return sb.ToString();
    }
}
