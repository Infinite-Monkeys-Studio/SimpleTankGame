using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : NetworkBehaviour
{
    NetworkVariable<int> RedScore = new NetworkVariable<int>(0);
    NetworkVariable<int> BlueScore = new NetworkVariable<int>(0);

    [SerializeField] Text RedScoreText;
    [SerializeField] Text BlueScoreText;

    public void TeamScored(int teamIndex)
    {
        Debug.Log($"Team {teamIndex} scored");
        
        if(NetworkManager.Singleton.IsServer)
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

        Debug.Log($"Red: {RedScore.Value}  Blue: {BlueScore.Value}");
    }

    private void OnEnable()
    {
        RedScore.OnValueChanged += ScoreChanged;
        BlueScore.OnValueChanged += ScoreChanged;
    }

    private void OnDisable()
    {
        RedScore.OnValueChanged -= ScoreChanged;
        BlueScore.OnValueChanged -= ScoreChanged;
    }

    private void ScoreChanged(int previousValue, int newValue)
    {
        RedScoreText.text = RedScore.Value.ToString();
        BlueScoreText.text = BlueScore.Value.ToString();
    }
}
