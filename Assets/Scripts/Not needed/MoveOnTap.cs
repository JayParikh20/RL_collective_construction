using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnTap : MonoBehaviour
{
	
    void Start()
    {
        
    }

    void Update()
    {
		if (Input.GetMouseButtonDown(0))
        {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				Vector3 newPosition = hit.point;
				newPosition.x = Mathf.Floor(newPosition.x) + 0.5f;
				newPosition.y = 0.5f;
				newPosition.z = Mathf.Floor(newPosition.z) + 0.5f;
				transform.position = newPosition;
			}
        }
    }
}
