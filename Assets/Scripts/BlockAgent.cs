using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class BlockAgent : Agent
{
	[HideInInspector]
	public ConstructionController Controller = null;
	[HideInInspector]
	public Vector3 InitialPosition;
	// [HideInInspector]
	// public Vector3 TargetPosition;
	[HideInInspector]
	public GameObject Block = null;
	[HideInInspector]
	public GameObject BlockRoot = null;
	[HideInInspector]
	public int AgentIndex;
	public Material NormalMaterial;
	public Material ReachedMaterial;
	public bool showCummReward = false;

	bool AgentPicked = false;
	float OldTargetDist;
	float OldBlockDist;
	// float OldInitialBlockDist;
	bool AgentReached = false;
	bool[] ButtonPressed = new bool[4];

	[HideInInspector]
	public int[,] AgentObservation = new int[5, 5];
	
    void Start () {

    }

	void Update() {
		ButtonPressed[0] = Input.GetKeyDown(KeyCode.W);
		ButtonPressed[1] = Input.GetKeyDown(KeyCode.D);
		ButtonPressed[2] = Input.GetKeyDown(KeyCode.S);
		ButtonPressed[3] = Input.GetKeyDown(KeyCode.A);
	}
	
    public override void OnEpisodeBegin()
    {
		AgentPicked = false;
		OldTargetDist = Mathf.Infinity;
		OldBlockDist = Mathf.Infinity;
		// OldInitialBlockDist = Mathf.Infinity;
		AgentReached = false;
		gameObject.GetComponent<MeshRenderer>().material = NormalMaterial;
		for(int i = 0; i < AgentObservation.GetLength(0); i++)
        {
            for(int j = 0; j < AgentObservation.GetLength(1); j++)
            {
                AgentObservation[i,j] = 1;
            }
        }
    }
	
	public override void CollectObservations(VectorSensor sensor)
	{
		List<float> obsList = new List<float>();
		for(int i = 0; i < AgentObservation.GetLength(0); i++)
        {
            for(int j = 0; j < AgentObservation.GetLength(1); j++)
            {
                obsList.Add((float) AgentObservation[i,j]);
            }
        }
		sensor.AddObservation(obsList);
		sensor.AddObservation(this.transform.localPosition);

		sensor.AddObservation(AgentPicked? 1 : 0);
		// if(Block == null) {
		// 	sensor.AddObservation(0);
		// 	// sensor.AddObservation(this.transform.localPosition);
		// 	return;
		// } else {
		// 	sensor.AddObservation(AgentPicked? 1 : 0);
		// 	if(AgentPicked) {
		// 		// sensor.AddObservation(new Vector3(0f, 0.5f, 0f));
		// 	} else {
		// 		// sensor.AddObservation(Block.transform.localPosition);
		// 	}
		// }
	}
	
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// 0 - idle
		// 1 - forward
		// 2 - right
		// 3 - backward
		// 4 - left
		Controller.UpdateTimeStep();
		if(!Controller.InHeuristics) {
			AddReward(-0.005f);
		}
		int action = actionBuffers.DiscreteActions[0];
		Vector3 oldAgentPos = this.transform.position;
		if(!AgentReached) {
			switch(action)
			{
				case 0: {
					break;
				}
				case 1: {
					this.transform.position = new Vector3(this.transform.position.x, 
														this.transform.position.y, 
														this.transform.position.z + 1);
					if(AgentObservation[1,2] == 3) {
						AddReward(-0.1f);
						// Controller.ResetEnvironment(true);
					}
					// if(AgentObservation[1,2] == 2) {
					// 	Debug.Log("Agent Collided");
					// 	AddReward(-1f);
					// }
					// if(AgentObservation[0,2] == 2) {
					// 	AddReward(-0.04f);
					// }
					break;
				}
				case 2: {
					this.transform.position = new Vector3(this.transform.position.x + 1, 
														this.transform.position.y, 
														this.transform.position.z);
					if(AgentObservation[2,3] == 3) {
						AddReward(-0.1f);
						// Controller.ResetEnvironment(true);
					}
					// if(AgentObservation[2,3] == 2) {
					// 	Debug.Log("Agent Collided");
					// 	AddReward(-1f);
					// }
					// if(AgentObservation[2,4] == 2) {
					// 	AddReward(-0.04f);
					// }
					break;
				}
				case 3: {
					this.transform.position = new Vector3(this.transform.position.x, 
														this.transform.position.y, 
														this.transform.position.z - 1);
					if(AgentObservation[3,2] == 3) {
						AddReward(-0.1f);
						// Controller.ResetEnvironment(true);
					}
					// if(AgentObservation[3,2] == 2) {
					// 	Debug.Log("Agent Collided");
					// 	AddReward(-1f);
					// }
					// if(AgentObservation[4,2] == 2) {
					// 	AddReward(-0.04f);
					// }
					break;
				}
				case 4: {
					this.transform.position = new Vector3(this.transform.position.x - 1, 
														this.transform.position.y, 
														this.transform.position.z);
					if(AgentObservation[2,1] == 3) {
						AddReward(-0.1f);
						// Controller.ResetEnvironment(true);
					}
					// if(AgentObservation[2,1] == 2) {
					// 	Debug.Log("Agent Collided");
					// 	AddReward(-1f);
					// }
					// if(AgentObservation[2,0] == 2) {
					// 	AddReward(-0.04f);
					// }
					break;
				}
			}
		}

		Controller.UpdateAgentBounds(oldAgentPos, this.transform.localPosition, gameObject);
		if(Block != null || true) {
			if(!AgentPicked) {
				// float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
				// if(blockDst < 1) {
				// 	AgentPicked = true;
				// 	AddReward(1f);
				// 	Block.transform.SetParent(this.transform);
				// 	Block.transform.localPosition = new Vector3(0f, 1f, 0f);
				// }
				// if(blockDst < OldBlockDist) {
				// 	OldBlockDist = blockDst;
				// 	AddReward(0.05f);
				// }
				GameObject blockToRemove = null;
				foreach(GameObject block in Controller.SpawnedBlocks) {
					float blockPickDst = Vector3.Distance(this.transform.localPosition, block.transform.position);
					if(blockPickDst < 1) {
						AddReward(1f);
						AgentPicked = true;
						block.transform.SetParent(this.transform, false);
						block.transform.localPosition = new Vector3(0f, 1f, 0f);
						blockToRemove = block;
						Block = block;
					}
				}
				if(blockToRemove != null) {
					Controller.SpawnedBlocks.Remove(blockToRemove);
				}
			}

			if(AgentPicked) {
				// float targetDst = Vector3.Distance(this.transform.localPosition, new Vector3(0f, 0.5f, 0f));
				// if(targetDst < 1) {
				// 	AddReward(1f);
				// 	AgentPicked = false;
				// 	Block.transform.SetParent(BlockRoot.transform);
				// 	Block.transform.position = this.transform.position;
				// 	OldTargetDist = Mathf.Infinity;
				// 	OldBlockDist = Mathf.Infinity;
				// 	Controller.AgentGoalCompleted(gameObject);
				// 	if(Block == null) {
				// 		AgentReached = true;
				// 		gameObject.GetComponent<MeshRenderer> ().material = ReachedMaterial;
				// 	}
				// } 
				// if(targetDst < OldTargetDist) {
				// 	OldTargetDist = targetDst;
				// 	AddReward(0.05f);
				// }
				GameObject targetBlockToRemove = null;
				foreach(GameObject target in Controller.SpawnedTargets) {
					float targetBlockDst = Vector3.Distance(this.transform.localPosition, target.transform.position);
					if(targetBlockDst < 1) {
						AddReward(1f);
						AgentPicked = false;
						// Debug.Log(Block.gameObject.name);
						// Debug.Log(BlockRoot.gameObject.name);
						Block.transform.SetParent(BlockRoot.transform, false);
						Block.transform.position = this.transform.position;
						Controller.DoneBlocks.Add(Block);
						Block = null;
						OldTargetDist = Mathf.Infinity;
				 		OldBlockDist = Mathf.Infinity;
						Controller.AgentGoalCompleted(gameObject);
						// if(Block == null) {
						// 	AgentReached = true;
						// 	gameObject.GetComponent<MeshRenderer> ().material = ReachedMaterial;
						// }
						targetBlockToRemove = target;
					}
				}
				if(targetBlockToRemove != null) {
					Controller.SpawnedTargets.Remove(targetBlockToRemove);
				}
			}
		// } else {
			// float initialDst = Vector3.Distance(this.transform.localPosition, InitialPosition);
			// if(initialDst < 1) {
			// 	if(!AgentReached) {
			// 		AddReward(1f);
			// 		gameObject.GetComponent<MeshRenderer> ().material = ReachedMaterial;
			// 		AgentReached = true;
			// 	}
			// }
			// if(initialDst < OldInitialBlockDist) {
			// 	OldInitialBlockDist = initialDst;
			// 	AddReward(0.01f);
			// }
		}
		if(showCummReward) {
 			Debug.Log(gameObject.name + "- Cumrwd: " + GetCumulativeReward());
		}
	}
	
	public override void Heuristic(in ActionBuffers actionsOut)
	{
		// 0 - idle
		// 1 - forward
		// 2 - right
		// 3 - backward
		// 4 - left
		
		var discreteActionsOut = actionsOut.DiscreteActions;
		discreteActionsOut[0] = -1;

		if(Controller.HeuristicIndex != AgentIndex) {
			return;
		}
	
		if(ButtonPressed[0]) {
			discreteActionsOut[0] = 1;
		}	
		
		if(ButtonPressed[1]) {
			discreteActionsOut[0] = 2;
		}

		if(ButtonPressed[2]) {
			discreteActionsOut[0] = 3;
		}
		
		if(ButtonPressed[3]) {
			discreteActionsOut[0] = 4;
		}
	}
}
