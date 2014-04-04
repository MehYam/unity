using System.Collections;

public class Game
{
    readonly Hero[] _heros = new Hero[2];
    public Hero[] heros
    {
        get
        {
            return _heros;
        }
    }

    int turn { get; private set; }
    public Game()
    {
        turn = 1;
    }

    public override string ToString()
    {
        
    }
}
