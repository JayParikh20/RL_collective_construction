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
    
    private List<GameObject> SpawnedBlocks = new List<GameObject>();
    private List<GameObject> SpawnedTargets = new List<GameObject>();
    private GameObject BlockRoot = null;
    private GameObject TargetRoot = null;
    private List<Vector3> AgentsInitialPosition;
    private int CurrentBlockIndex = 0;
    private int GoalCompletedIndex = 0;
    private int TimeStep = 0;
    private int GoalReachedIndex = 0;
    private int AgentCollided = 0;
    private int[,] OccupancyGrid;

    void Start()
    {
        OccupancyGrid = new int[GridSize, GridSize];
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
        if(TimeStep > MaxTimeSteps) {
            for(int i=0; i <  BlockAgents.Count; i++) {
                    BlockAgents[i].GetComponent<BlockAgent>().EpisodeInterrupted();
            }
            ResetEnvironment(true);
        }
    }

    public void ResetEnvironment(bool interrupted = false) {
        OccupancyGrid = new int[GridSize, GridSize];
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
        AgentCollided = 0;
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
       
        OccupancyGrid[(int) Mathf.Floor(oldPosition.x+10), (int) Mathf.Floor(oldPosition.z+10)] = 0;
        int newPostionValue =  OccupancyGrid[(int) Mathf.Floor(localPostion.x+10), (int) Mathf.Floor(localPostion.z+10)];
        if(newPostionValue != 0) {
            // Penalizing for collision 
            AgentCollided++;
            Debug.Log("Agent Collided: "+ AgentCollided);
            callingAgent.GetComponent<BlockAgent>().AddReward(-1f);
        }
        OccupancyGrid[(int) Mathf.Floor(localPostion.x+10), (int) Mathf.Floor(localPostion.z+10)] = 1;
        // StringBuilder sb = new StringBuilder();
        // for(int k=0; k< OccupancyGrid .GetLength(1); k++)
        // {
        //     for(int j=0; j<OccupancyGrid .GetLength(0); j++)
        //     {
        //         sb.Append(OccupancyGrid[k,j]);
        //         sb.Append(' ');
        //     }
        //     sb.AppendLine();
        // }
        // Debug.Log(sb.ToString());
        // Debug.Log("|||||||||||||||||||||||||||||||||||");
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
        for(int i=0; i < NumberOfMaterials; i++) {
                    float blockX = TargetSpawnStart.transform.position.x + Mathf.Floor(i / TargetHeight);
                    float blockZ = TargetSpawnStart.transform.position.z - (TargetHeight - 1) + (i % TargetHeight);
                    GameObject target = Instantiate(Target, new Vector3(blockX, TargetSpawnStart.transform.position.y, blockZ), Quaternion.identity);
                    target.transform.SetParent(TargetRoot.transform); 
                    SpawnedTargets.Add(target);
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
            OccupancyGrid[(int) Mathf.Floor(AgentsInitialPosition[i].x+10), (int) Mathf.Floor(AgentsInitialPosition[i].z+10)] = 1;
            // StringBuilder sb = new StringBuilder();
            // for(int k=0; k< OccupancyGrid .GetLength(1); k++)
            // {
            //     for(int j=0; j<OccupancyGrid .GetLength(0); j++)
            //     {
            //         sb.Append(OccupancyGrid[k,j]);
            //         sb.Append(' ');
            //     }
            //     sb.AppendLine();
            // }
            // Debug.Log(sb.ToString());
        }
        CurrentBlockIndex = BlockAgents.Count - 1;
    }
}
