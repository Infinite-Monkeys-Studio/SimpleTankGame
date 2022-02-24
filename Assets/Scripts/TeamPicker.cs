using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TeamPicker : MonoBehaviour
{
    public void PickTeam(int teamIndex)
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject.TryGetComponent(out TeamPlayer teamPlayer))
        {
            teamPlayer.SetTeamServerRpc((byte)teamIndex);
        }
    }
}
