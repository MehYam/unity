using System.Collections;

public class Game
{
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

    public void Start()
    {
        _heros[0].Draw(3);
        _heros[1].Draw(4);

        // _heros[1] add the Coin
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(_heros[0].ToString());
        sb.AppendLine(_heros[1].ToString());

        return sb.ToString();
    }
}
