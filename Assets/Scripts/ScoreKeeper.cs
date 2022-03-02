using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
    NetworkVariable<int> RedScore = new NetworkVariable<int>(0);
    NetworkVariable<int> BlueScore = new NetworkVariable<int>(0);

    [SerializeField] Text RedScoreText;
    [SerializeField] Text BlueScoreText;

    private static ScoreKeeper scoreKeeperInstance;
    public static ScoreKeeper Singleton { get
        {
            if(scoreKeeperInstance == null)
            {
                scoreKeeperInstance = new ScoreKeeper();
            }
            return scoreKeeperInstance;
        } }

    public void TeamScored(int teamIndex)
    {
        if (teamIndex == 0)
        {
            RedScore.Value += 1;
        }
        else
        {
            BlueScore.Value += 1;
        }
    }

    private void Update()
    {
        RedScoreText.text = RedScore.Value.ToString();
        BlueScoreText.text = BlueScore.Value.ToString();
    }
}
