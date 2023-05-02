using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    [SerializeField] int Nodes = 0;
    [SerializeField] int Offset = 2;
    [SerializeField] GameObject NodePrefab;

    public bool HasNodes = false;

    public void SpawnNodes(int nodes)
    {
        if (nodes <= 0)
        {
            return;
        }

        Nodes = nodes;

        for (int i = 0; i < Nodes; i++)
        {
            Vector3 pos = new Vector3(transform.position.x,transform.position.y, i * offset);
            GameObject node = Instantiate(NodePrefab, pos, Quaternion.identity);
        }

        HasNodes = true;
    }
}