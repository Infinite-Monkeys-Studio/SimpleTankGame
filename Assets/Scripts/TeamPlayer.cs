using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TeamPlayer : NetworkBehaviour
{
    [SerializeField] private List<MeshRenderer> meshes;
    [SerializeField] private List<Material> TeamColors;

    NetworkVariable<byte> TeamIndex = new NetworkVariable<byte>();

    public int getTeamIndex()
    {
        return (int)TeamIndex.Value;
    }

    private void Start()
    {
        OnTeamChange();
    }

    [ServerRpc]
    internal void SetTeamServerRpc(byte newTeamIndex)
    {
        
        if (newTeamIndex > 1) { return; }
        TeamIndex.Value = newTeamIndex;
    }

    private void OnEnable()
    {
        TeamIndex.OnValueChanged += OnTeamChange;
    }

    private void OnDisable()
    {
        TeamIndex.OnValueChanged -= OnTeamChange;
    }

    private void OnTeamChange(byte previousValue, byte newValue)
    {
        OnTeamChange();
    }

    private void OnTeamChange()
    {
        foreach (MeshRenderer mesh in meshes)
        {
            Material[] newMaterials = mesh.materials;
            newMaterials[1] = TeamColors[TeamIndex.Value];
            mesh.materials = newMaterials;
        }
    }
}
