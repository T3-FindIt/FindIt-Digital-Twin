using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

class Node
{
    public int x;
    public int z;
    public Node(int x, int z)
    {
        this.x = x;
        this.z = z;
    }
}
