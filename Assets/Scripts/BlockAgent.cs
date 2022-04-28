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
	[HideInInspector]
	public Vector3 TargetPosition;
	[HideInInspector]
	public GameObject Block = null;
	[HideInInspector]
	public GameObject BlockRoot = null;
	[HideInInspector]
	public int AgentIndex;

	bool AgentPicked = false;
	float OldTargetDist;
	float OldBlockDist;
	float OldInitialBlockDist;
	bool AgentReached = false;
	
    void Start () {

    }
	
    public override void OnEpisodeBegin()
    {
		AgentPicked = false;
		OldTargetDist = Mathf.Infinity;
		OldBlockDist = Mathf.Infinity;
		OldInitialBlockDist = Mathf.Infinity;
		AgentReached = false;
    }
	
	public override void CollectObservations(VectorSensor sensor)
	{
		if(Block == null) {
			sensor.AddObservation(this.transform.localPosition);
			sensor.AddObservation(0);
			sensor.AddObservation(InitialPosition);
			return;
		}

		sensor.AddObservation(this.transform.localPosition);
		sensor.AddObservation(AgentPicked? 1 : 0);
		if(AgentPicked) {
			sensor.AddObservation(TargetPosition);
		} else {
			sensor.AddObservation(Block.transform.localPosition);
		}
	}
	
	public override void OnActionReceived(ActionBuffers actionBuffers)
	{
		// 0 - idle
		// 1 - forward
		// 2 - right
		// 3 - backward
		// 4 - left
		Controller.UpdateTimeStep();
		int action = actionBuffers.DiscreteActions[0];
		Vector3 oldAgentPos = this.transform.position;
		switch(action)
		{
			case 0: {
				break;
			}
			case 1: {
				this.transform.position = new Vector3(this.transform.position.x, 
													this.transform.position.y, 
													this.transform.position.z + 1);
				break;
			}
			case 2: {
				this.transform.position = new Vector3(this.transform.position.x + 1, 
													this.transform.position.y, 
													this.transform.position.z);
				break;
			}
			case 3: {
				this.transform.position = new Vector3(this.transform.position.x, 
													this.transform.position.y, 
													this.transform.position.z - 1);
				break;
			}
			case 4: {
				this.transform.position = new Vector3(this.transform.position.x - 1, 
													this.transform.position.y, 
													this.transform.position.z);
				break;
			}
		}

		Controller.UpdateAgentBounds(oldAgentPos, this.transform.localPosition, gameObject);
		if(Block != null) {
			if(!AgentPicked) {
				float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
				if(blockDst < 1) {
					AgentPicked = true;
					SetReward(1);
					Block.transform.SetParent(this.transform);
					Block.transform.localPosition = new Vector3(0f, 1f, 0f);
				}
				if(blockDst < OldBlockDist) {
					OldBlockDist = blockDst;
					AddReward(0.05f);
				}
			}

			if(AgentPicked) {
				float targetDst = Vector3.Distance(this.transform.localPosition, TargetPosition);
				if(targetDst < 1) {
					SetReward(1);
					AgentPicked = false;
					Block.transform.SetParent(BlockRoot.transform);
					Block.transform.position = this.transform.position;
					OldTargetDist = Mathf.Infinity;
					OldBlockDist = Mathf.Infinity;
					Controller.AgentGoalCompleted(gameObject);
				} 
				if(targetDst < OldTargetDist) {
					OldTargetDist = targetDst;
					AddReward(0.05f);
				}
			}
		} else {
			float initialDst = Vector3.Distance(this.transform.localPosition, InitialPosition);
			if(initialDst < OldInitialBlockDist) {
				OldInitialBlockDist = initialDst;
				AddReward(0.05f);
			}
			if(initialDst < 1) {
				if(!AgentReached) {
					SetReward(1f);
					AgentReached = true;
				}
			}
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
