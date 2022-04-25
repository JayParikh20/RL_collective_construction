using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
 
[System.Serializable]
public class AgentSet
{
	public GameObject block;
	public GameObject target;
}
 
public class BlockAgent : Agent
{
	public bool initializeRandomly = false;
	// public Vector2 groundSize = new Vector2(20, 20);
	public float bound = 10f;
	public GameObject Block;
	public GameObject Target;
	bool agentPicked = false;
	List<Vector3> blockInitalPositions;
	Vector3 agentInitalPosition;
	float oldTargetDist;
	float oldBlockDist;
	int stepCount = 0;
	public int MaxSteps;
	int goalReachCount = 0;

	bool WPressed = false;
	bool DPressed = false;
	bool APressed = false;
	bool SPressed = false;
	
	public AgentSet[] agentSets;
	int timeStepCounter = 1;
	int currentAgentSetIndex = 0;
	public bool InHeuristics = false;
	public bool SlowDownTime = false;
	
    void Start () {
		blockInitalPositions = new List<Vector3>(agentSets.Length);
		for(int i=0; i < agentSets.Length; i++){
			blockInitalPositions.Add(agentSets[i].block.transform.position);
		}
		agentInitalPosition = this.transform.position;
		if(SlowDownTime) {
			Time.timeScale = 0.2f;
		} else {
			Time.timeScale = 1f;
		}
    }
	
	void Update() {
		WPressed = Input.GetKeyDown(KeyCode.W);
		DPressed = Input.GetKeyDown(KeyCode.D);
		APressed = Input.GetKeyDown(KeyCode.A);
		SPressed = Input.GetKeyDown(KeyCode.S);
	}
	
    public override void OnEpisodeBegin()
    {
		// Debug.Log("Episode Begin");
		for(int i=0; i < agentSets.Length; i++){
			agentSets[i].block.transform.SetParent(this.transform.parent);
			agentSets[i].block.transform.position = blockInitalPositions[i];
		}
		this.transform.localPosition = agentInitalPosition;
		
		for(int i=0; i < agentSets.Length; i++){
			agentSets[i].target.transform.position = new Vector3(Random.Range(0, 10) + 0.5f,
											   0.5f,
											   Random.Range(0, 10) + 0.5f);
		}
		// this.transform.position = new Vector3(Random.Range(-49, 49) + 0.5f,
											   // 0.5f,
											   // Random.Range(-49, 49) + 0.5f);
	
		stepCount = 0;
		currentAgentSetIndex = 0;
		timeStepCounter = 1;
		// For current agent set
		agentPicked = false;
		oldTargetDist = Mathf.Infinity;
		oldBlockDist = Mathf.Infinity;
		
    }
	
	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(this.transform.localPosition);
		sensor.AddObservation(agentPicked? 1 : 0);
		if(agentPicked) {
			sensor.AddObservation(agentSets[currentAgentSetIndex].target.transform.localPosition);
		} else {
			sensor.AddObservation(agentSets[currentAgentSetIndex].block.transform.localPosition);
		}
	}
	
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		if(!InHeuristics) {
			stepCount++;
			if(stepCount > MaxSteps * timeStepCounter) {
				EndEpisode();
			}
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
		
		// float agentBlock1Dst = Vector3.Distance(this.transform.localPosition, agentSets[0].block.transform.localPosition);
		// float agentBlock2Dst = Vector3.Distance(this.transform.localPosition, agentSets[1].block.transform.localPosition);
		// float agentBlock3Dst = Vector3.Distance(this.transform.localPosition, agentSets[2].block.transform.localPosition);
		// float agentBlock4Dst = Vector3.Distance(this.transform.localPosition, agentSets[3].block.transform.localPosition);
		// if(agentBlock1Dst < 1 && currentAgentSetIndex != 0) {
			// AddReward(-1f);
		// }
		// if(agentBlock2Dst < 1 && currentAgentSetIndex != 1) {
			// AddReward(-1f);
		// }
		// if(agentBlock3Dst < 1 && currentAgentSetIndex != 2) {
			// AddReward(-1f);
		// }
		// if(agentBlock4Dst < 1 && currentAgentSetIndex != 3) {
			// AddReward(-1f);
		// }
		
		 
		// Reward 4
		if(!agentPicked){
			float blockDst = Vector3.Distance(this.transform.localPosition, agentSets[currentAgentSetIndex].block.transform.localPosition);
			if(blockDst < 1) {
				agentPicked = true;
				SetReward(1);
				agentSets[currentAgentSetIndex].block.transform.SetParent(this.transform);
				agentSets[currentAgentSetIndex].block.transform.localPosition = new Vector3(0f, 1f, 0f);
			}
			if(blockDst < oldBlockDist) {
				oldBlockDist = blockDst;
				AddReward(0.05f);
			}
		}
		if(agentPicked) {
			float targetDst = Vector3.Distance(this.transform.localPosition, agentSets[currentAgentSetIndex].target.transform.localPosition);
			if(targetDst < 1) {
				SetReward(1);
				if(currentAgentSetIndex < agentSets.Length - 1) {
					agentPicked = false;
					agentSets[currentAgentSetIndex].block.transform.SetParent(this.transform.parent);
					agentSets[currentAgentSetIndex].block.transform.position = this.transform.position;
					currentAgentSetIndex++;
					oldTargetDist = Mathf.Infinity;
					oldBlockDist = Mathf.Infinity;
					if(currentAgentSetIndex % 4 == 0) {
						timeStepCounter++;
					}
				} else {
					goalReachCount++;
					Debug.Log("Goal Reached: " + goalReachCount);
					EndEpisode();
				}
				// Debug.Log("currentAgentSetIndex: " + currentAgentSetIndex);

			} 
			if(targetDst < oldTargetDist) {
				oldTargetDist = targetDst;
				AddReward(0.05f);
			}

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
	
		if(WPressed) {
			discreteActionsOut[0] = 0;
		}	
		
		if(DPressed) {
			discreteActionsOut[0] = 1;
		}

		if(SPressed) {
			discreteActionsOut[0] = 2;
		}
		
		if(APressed) {
			discreteActionsOut[0] = 3;
		}
	}
}
