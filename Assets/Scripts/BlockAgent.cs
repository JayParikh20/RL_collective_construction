using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BlockAgent : Agent
{
	public bool initializeRandomly = false;
	// public Vector2 groundSize = new Vector2(20, 20);
	public float bound = 10f;
	public GameObject Block;
	public GameObject Target;
	public int pickReward = 1;
	public int placeReward = 100;
	bool agentPicked = false;
	Vector3 blockInitalPosition;
	Vector3 agentInitalPosition;
	float oldTargetDist;
	float oldBlockDist;
	int stepCount = 0;
	public int MaxSteps = 200;
	int goalReachCount = 0;
	float timeSinceDecision = 0f;
	
    void Start () {
		blockInitalPosition = Block.transform.position;
		agentInitalPosition = this.transform.position;
    }
	
	public void FixedUpdate() {
        WaitTimeInference();
    }
 
	void WaitTimeInference() {
			if (timeSinceDecision >= 2) {
				timeSinceDecision = 0f;
				RequestDecision();
			} else {
				timeSinceDecision += Time.fixedDeltaTime;
			}
	}

    public override void OnEpisodeBegin()
    {
		if(initializeRandomly) {
			Debug.Log("random Intial position not coded");
			// this.transform.localPosition = new Vector3(Mathf.Round(Random.value * groundSize[0]),
                                           // 0.5f,
                                           // Mathf.Round(Random.value * groundSize[1]));
		} else {
			this.transform.localPosition = agentInitalPosition;
		}
		Block.transform.SetParent(this.transform.parent);
		Block.transform.position = blockInitalPosition;
		Target.transform.position = new Vector3(Random.Range(-5, 5) + 0.5f,
											   0.5f,
											   Random.Range(-5, 5) + 0.5f);
	    this.transform.localPosition = new Vector3(Random.Range(-5, 5) + 0.5f,
											   0.5f,
											   Random.Range(-5, 5) + 0.5f);
		float tempDst = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
		while(tempDst < 1) {
			this.transform.localPosition = new Vector3(Random.Range(-5, 5) + 0.5f,
											   0.5f,
											   Random.Range(-5, 5) + 0.5f);
		    tempDst = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
		}
		agentPicked = false;
		oldTargetDist = Mathf.Infinity;
		oldBlockDist = Mathf.Infinity;
		stepCount = 0;
    }
	
	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(this.transform.localPosition);
		// sensor.AddObservation(Block.transform.localPosition);
		
		if(agentPicked) {
			sensor.AddObservation(Target.transform.localPosition);
		} else {
			sensor.AddObservation(Block.transform.localPosition);
		}
	}
	
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		stepCount++;
		if(stepCount > MaxSteps) {
			EndEpisode();
		}
		// 0 - idle NA
		// 1 - forward
		// 2 - right
		// 3 - backward
		// 4 - left
		int action = actionBuffers.DiscreteActions[0];
		switch(action)
		{
			case 0: {
				this.transform.position = new Vector3(this.transform.position.x, 
													this.transform.position.y, 
													this.transform.position.z + 1);
				break;
			}
			case 1: {
				this.transform.position = new Vector3(this.transform.position.x + 1, 
													this.transform.position.y, 
													this.transform.position.z);
				break;
			}
			case 2: {
				this.transform.position = new Vector3(this.transform.position.x, 
													this.transform.position.y, 
													this.transform.position.z - 1);
				break;
			}
			case 3: {
				this.transform.position = new Vector3(this.transform.position.x - 1, 
													this.transform.position.y, 
													this.transform.position.z);
				break;
			}
		}
		if(this.transform.localPosition.x > (bound) || this.transform.localPosition.x < -(bound)){
			EndEpisode();
		}
		if(this.transform.localPosition.z > (bound) || this.transform.localPosition.z < -(bound)){
			EndEpisode();
		}
		
		//Reward struct 1
		// float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
		// if(blockDst < 1) {
			// if(!agentPicked){
				// agentPicked = true;
				// SetReward(pickReward);
				// Block.transform.SetParent(this.transform);
				// Block.transform.localPosition = new Vector3(0f, 1f, 0f);
			// }
		// }
		
		// if(agentPicked) {
			// float targetDst = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
			// if(targetDst < 1) {
				// SetReward(placeReward);
				// EndEpisode();
			// }
			// if(targetDst < oldTargetDist) {
				// oldTargetDist = targetDst;
				// AddReward(0.5f);
			// }
		// }
		
		////////////////---------------------------///////////////
		
		
		// Reward 2 
		// float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
		// if(blockDst < 1) {
			// if(!agentPicked){
				// agentPicked = true;
				// SetReward(5);
				// Block.transform.SetParent(this.transform);
				// Block.transform.localPosition = new Vector3(0f, 1f, 0f);
			// }
		// } else {
			// if(!agentPicked){
				// AddReward(-0.01f);
			// }
		// }
		// if(blockDst < oldBlockDist) {
			// if(!agentPicked){
				// oldBlockDist = blockDst;
				// AddReward(1f);
			// }
		// }
		
		// if(agentPicked) {
			// float targetDst = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
			// if(targetDst < 1) {
				// SetReward(100);
				// EndEpisode();
			// } else {
				// AddReward(-0.02f);
			// }
			// if(targetDst < oldTargetDist) {
				// oldTargetDist = targetDst;
				// AddReward(2f);
			// }
		// }
		
		////////////////---------------------------///////////////
		
		
		// Reward 3 
		 // float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
		 // if(blockDst < 1) {
			// SetReward(50);
			// EndEpisode();
		 // } else {
			// AddReward(-0.01f);
		 // }
		 
		 ////////////////---------------------------///////////////
		 
		// Reward 4
		if(!agentPicked){
			float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
			if(blockDst < 1) {
				agentPicked = true;
				SetReward(1);
				Block.transform.SetParent(this.transform);
				Block.transform.localPosition = new Vector3(0f, 1f, 0f);
			}
			if(blockDst < oldBlockDist) {
				oldBlockDist = blockDst;
				AddReward(0.05f);
			}
		}
		if(agentPicked) {
			float targetDst = Vector3.Distance(this.transform.localPosition, Target.transform.localPosition);
			if(targetDst < 1) {
				SetReward(100);
				goalReachCount++;
				Debug.Log("Goal Reached: " + goalReachCount);
				EndEpisode();
			} 
			if(targetDst < oldTargetDist) {
				oldTargetDist = targetDst;
				AddReward(0.05f);
			}
			// else {
				// SetReward(-0.02f);
			// }
		}
		
		// Debug.Log("Cumm Reward: " + GetCumulativeReward() + ", last action: " + action);
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
	
		if(Input.GetKeyDown(KeyCode.W)) {
			discreteActionsOut[0] = 0;
		}	
		
		if(Input.GetKeyDown(KeyCode.D)) {
			discreteActionsOut[0] = 1;
		}

		if(Input.GetKeyDown(KeyCode.S)) {
			discreteActionsOut[0] = 2;
		}
		
		if(Input.GetKeyDown(KeyCode.A)) {
			discreteActionsOut[0] = 3;
		}
	}
}
