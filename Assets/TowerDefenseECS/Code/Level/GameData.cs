using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    private static GameData m_Instance;
    public static GameData Instace
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new GameData();

            return m_Instance;
        }
    }
}
