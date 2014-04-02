using UnityEngine;
using System.Collections;

public sealed class Main
{
    // singleton
    static private Main _instance;
    private Main() { }

    static public Main Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Main();
            }
            return _instance;
        }
    }
}
