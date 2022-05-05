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
	[HideInInspector]
	public Vector3 AgentPosition;
	[HideInInspector]
	public float DistanceWithOtherAgent = 0;
	public Material NormalMaterial;
	public Material ReachedMaterial;

	bool AgentPicked = false;
	float OldTargetDist;
	float OldBlockDist;
	float OldInitialBlockDist;
	bool AgentReached = false;
	bool[] ButtonPressed = new bool[4];
	int AgentCollided = 0; 
	
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
		OldInitialBlockDist = Mathf.Infinity;
		AgentReached = false;
		AgentPosition = InitialPosition;
		AgentCollided = 0;
		gameObject.GetComponent<MeshRenderer> ().material = NormalMaterial;
    }
	
	public override void CollectObservations(VectorSensor sensor)
	{
		sensor.AddObservation(DistanceWithOtherAgent);
		sensor.AddObservation(this.transform.localPosition);
		if(Block == null) {
			sensor.AddObservation(0);
			sensor.AddObservation(InitialPosition);
			return;
		} else {
			sensor.AddObservation(AgentPicked? 1 : 0);
			if(AgentPicked) {
				sensor.AddObservation(TargetPosition);
			} else {
				sensor.AddObservation(Block.transform.localPosition);
			}
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
		}
		AgentPosition = this.transform.localPosition;
		Controller.UpdateAgentBounds(oldAgentPos, this.transform.localPosition, gameObject);
		if(DistanceWithOtherAgent < 1) {
			AddReward(-1f);
			AgentCollided++;
			Debug.Log("Agent Collided: "+ AgentCollided);
		}
		if(DistanceWithOtherAgent < 2) {
			AddReward(-0.1f);
			// Debug.Log("Agent Collision may happen");
		}
		if(Block != null) {
			if(!AgentPicked) {
				float blockDst = Vector3.Distance(this.transform.localPosition, Block.transform.localPosition);
				if(blockDst < 1) {
					AgentPicked = true;
					SetReward(1f);
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
					SetReward(1f);
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
			if(initialDst < 1) {
				if(!AgentReached) {
					SetReward(1f);
					gameObject.GetComponent<MeshRenderer> ().material = ReachedMaterial;
					AgentReached = true;
				}
			}
			if(initialDst < OldInitialBlockDist) {
				OldInitialBlockDist = initialDst;
				AddReward(0.05f);
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
