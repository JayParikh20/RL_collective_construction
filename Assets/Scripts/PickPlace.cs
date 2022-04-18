using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickPlace : MonoBehaviour
{
	public Collider material_area_pick;
	public Collider material_area_drop;
	
	bool agentHolding = false;
	GameObject cube;
	
    void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    void Update()
    {
        if (material_area_pick.bounds.Contains(transform.position) && !agentHolding)
        {
            agentHolding = true;
        }
		
		if (material_area_drop.bounds.Contains(transform.position) && agentHolding)
        {
            agentHolding = false;
			cube.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f);
        }
		
		if(agentHolding) {
			cube.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
		}
    }
}
