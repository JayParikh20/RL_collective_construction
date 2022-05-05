using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;

public class ConstructionController : MonoBehaviour
{
    public int GridSize;
    public int NumberOfMaterials;
    public int MaterialHeight;   //Blocks will start spawing from right to top direction
    public int TargetHeight;
    public GameObject BlockSpawnStart;
    public GameObject TargetSpawnStart;
    public GameObject Block;
    public GameObject Target;
    public bool SlowDownTime;
    public List<GameObject> BlockAgents;
    public int HeuristicIndex = 0;
    public int MaxTimeSteps = -1;
    public bool InHeuristics = false;
    public bool Randomized = false;
    
    private List<GameObject> SpawnedBlocks = new List<GameObject>();
    private List<GameObject> SpawnedTargets = new List<GameObject>();
    private GameObject BlockRoot = null;
    private GameObject TargetRoot = null;
    private List<Vector3> AgentsInitialPosition;
    private int CurrentBlockIndex = 0;
    private int GoalCompletedIndex = 0;
    private int TimeStep = 0;
    // private int TimeStepMultiplier = 1;
    private int GoalReachedIndex = 0;

    void Start()
    {
        if(MaxTimeSteps < 0) {
            Debug.Log("Max Time Step not set!");
        }
        AgentsInitialPosition = new List<Vector3>();
        for(int i=0; i < BlockAgents.Count; i++) {
            AgentsInitialPosition.Add(BlockAgents[i].transform.localPosition);
        }
        ResetEnvironment();
        Time.timeScale = SlowDownTime ? 0.2f : 1f;
    }

    public void UpdateTimeStep() {
        if(InHeuristics) return;

        TimeStep++;  
        if(CurrentBlockIndex % 4 == 0) {
            TimeStep=0;
        }
        if(TimeStep > MaxTimeSteps) {
            for(int i=0; i <  BlockAgents.Count; i++) {
                    BlockAgents[i].GetComponent<BlockAgent>().EpisodeInterrupted();
            }
            ResetEnvironment(true);
            Debug.Log("MaxTimeStep reached");
        }
    }

    public void ResetEnvironment(bool interrupted = false) {
        for(int i=0; i <  BlockAgents.Count; i++) {
            BlockAgents[i].GetComponent<BlockAgent>().Block = null;
            if(interrupted) {
                BlockAgents[i].GetComponent<BlockAgent>().EpisodeInterrupted();
            } else {
                BlockAgents[i].GetComponent<BlockAgent>().EndEpisode();
            }
           
        }
        TimeStep = 0;
        CurrentBlockIndex = 0;
        GoalCompletedIndex = 0;
        // TimeStepMultiplier = 1;
        SpawnMaterialBlocks();
        SpawnTargetBlocks();
        HandleAgents();
    }

    public void UpdateAgentBounds(Vector3 oldPosition, Vector3 localPostion, GameObject callingAgent) {
        if(localPostion.x > (GridSize/2) || localPostion.x < -(GridSize/2)) {
			foreach(var agent in BlockAgents) {
                agent.GetComponent<BlockAgent>().EndEpisode();
            }
            ResetEnvironment();
            return;
		}
		if(localPostion.z > (GridSize/2) || localPostion.z < -(GridSize/2)) {
			foreach(var agent in BlockAgents) {
                agent.GetComponent<BlockAgent>().EndEpisode();
            }
            ResetEnvironment();
            return;
		}

        float agent12_distance = Vector3.Distance(BlockAgents[0].GetComponent<BlockAgent>().AgentPosition, BlockAgents[1].GetComponent<BlockAgent>().AgentPosition);
        BlockAgents[0].GetComponent<BlockAgent>().DistanceWithOtherAgent = agent12_distance;
        BlockAgents[1].GetComponent<BlockAgent>().DistanceWithOtherAgent = agent12_distance;
    }

    public void AgentGoalCompleted(GameObject callingAgent) {
        GoalCompletedIndex++;
        if(GoalCompletedIndex >= NumberOfMaterials) {
            GoalReachedIndex++;
            Debug.Log("Goal Reached: "+ GoalReachedIndex);
            ResetEnvironment();
            return;
        }
        CurrentBlockIndex++;
        if(CurrentBlockIndex < NumberOfMaterials) {
            callingAgent.GetComponent<BlockAgent>().Block = SpawnedBlocks[CurrentBlockIndex];
            callingAgent.GetComponent<BlockAgent>().TargetPosition = SpawnedTargets[CurrentBlockIndex].transform.localPosition;
        } else {
            callingAgent.GetComponent<BlockAgent>().Block = null;
            callingAgent.GetComponent<BlockAgent>().TargetPosition = Vector3.zero;
        }
    }

    void SpawnMaterialBlocks() {
        if(BlockRoot != null) {
            foreach (var gameObject in SpawnedBlocks) {
                Destroy(gameObject);
            }
            Destroy(BlockRoot);
            BlockRoot = null;
            SpawnedBlocks = new List<GameObject>();
        }
        if(NumberOfMaterials==0 || MaterialHeight==0) {
            throw new System.NotImplementedException("Either Number of Materials or Material Height is Zero!");
        }
        BlockRoot = new GameObject("Blocks");
        BlockRoot.transform.SetParent(this.transform.parent);
        for(int i=0; i < NumberOfMaterials; i++) {
                    float blockX = BlockSpawnStart.transform.position.x + (MaterialHeight-2) - Mathf.Floor(i / MaterialHeight);
                    float blockZ = BlockSpawnStart.transform.position.z + (i % MaterialHeight);
                    GameObject block = Instantiate(Block, new Vector3(blockX, BlockSpawnStart.transform.position.y, blockZ), Quaternion.identity);
                    block.transform.SetParent(BlockRoot.transform); 
                    SpawnedBlocks.Add(block);
        }
        BlockSpawnStart.SetActive(false);
    }

    void SpawnTargetBlocks() {
        if(TargetRoot != null) {
            foreach (var gameObject in SpawnedTargets) {
                Destroy(gameObject);
            }
            Destroy(TargetRoot);
            TargetRoot = null;
            SpawnedTargets = new List<GameObject>();
        }
        if(NumberOfMaterials==0 || TargetHeight==0) {
            throw new System.NotImplementedException("Either Number of Materials or Target Height is Zero!");
        }
        TargetRoot = new GameObject("Targets");
        TargetRoot.transform.SetParent(this.transform.parent);
        if(Randomized) {
            List<Vector3> positions = new List<Vector3>();
            for(int i=0; i < NumberOfMaterials; i++) {
                float blockX = UnityEngine.Random.Range(0, 5) + 0.5f;
                float blockZ = UnityEngine.Random.Range(0, 5) + 0.5f;
                Vector3 pos =  new Vector3(blockX, TargetSpawnStart.transform.position.y, blockZ);
                while(positions.Contains(pos)) {
                    blockX = UnityEngine.Random.Range(0, 5) + 0.5f;
                    blockZ = UnityEngine.Random.Range(0, 5) + 0.5f;
                    pos =  new Vector3(blockX, TargetSpawnStart.transform.position.y, blockZ);
                }
                GameObject target = Instantiate(Target, pos, Quaternion.identity);
                target.transform.SetParent(TargetRoot.transform); 
                SpawnedTargets.Add(target);
                positions.Add(pos);
            }
        } else {
            for(int i=0; i < NumberOfMaterials; i++) {
                float blockX = TargetSpawnStart.transform.position.x + Mathf.Floor(i / TargetHeight);
                float blockZ = TargetSpawnStart.transform.position.z - (TargetHeight - 1) + (i % TargetHeight);
                GameObject target = Instantiate(Target, new Vector3(blockX, TargetSpawnStart.transform.position.y, blockZ), Quaternion.identity);
                target.transform.SetParent(TargetRoot.transform); 
                SpawnedTargets.Add(target);
            }
        }
        TargetSpawnStart.SetActive(false);
    }

    void HandleAgents() {
        for(int i=0; i <  BlockAgents.Count; i++) {
            BlockAgents[i].transform.localPosition = AgentsInitialPosition[i];
            BlockAgents[i].GetComponent<BlockAgent>().AgentIndex = i;
            BlockAgents[i].GetComponent<BlockAgent>().InitialPosition = AgentsInitialPosition[i];
            BlockAgents[i].GetComponent<BlockAgent>().Controller = this;
            BlockAgents[i].GetComponent<BlockAgent>().BlockRoot = BlockRoot;
            BlockAgents[i].GetComponent<BlockAgent>().Block = SpawnedBlocks[i];
            BlockAgents[i].GetComponent<BlockAgent>().TargetPosition = SpawnedTargets[i].transform.localPosition;
        }
        CurrentBlockIndex = BlockAgents.Count - 1;
        float agent12_distance = Vector3.Distance(BlockAgents[0].GetComponent<BlockAgent>().AgentPosition, BlockAgents[1].GetComponent<BlockAgent>().AgentPosition);
        BlockAgents[0].GetComponent<BlockAgent>().DistanceWithOtherAgent = agent12_distance;
        BlockAgents[1].GetComponent<BlockAgent>().DistanceWithOtherAgent = agent12_distance;
    }
}
