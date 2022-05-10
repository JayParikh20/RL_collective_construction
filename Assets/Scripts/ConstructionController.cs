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
    public bool SpecificShape = false;
    
    private List<GameObject> SpawnedBlocks = new List<GameObject>();
    private List<GameObject> SpawnedTargets = new List<GameObject>();
    private GameObject BlockRoot = null;
    private GameObject TargetRoot = null;
    private List<Vector3> AgentsInitialPosition;
    private int CurrentBlockIndex = 0;
    private int GoalCompletedIndex = 0;
    public int TimeStep = 0;
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
        HandleAgentsObservation();
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
        if(SpecificShape) {
            List<Vector3> Hshape = new List<Vector3>();
            Hshape.Add(new Vector3(4.5f, 0.5f, 3.5f));
            Hshape.Add(new Vector3(4.5f, 0.5f, 2.5f));
            Hshape.Add(new Vector3(4.5f, 0.5f, 1.5f));
            Hshape.Add(new Vector3(4.5f, 0.5f, 0.5f));

            Hshape.Add(new Vector3(3.5f, 0.5f, 1.5f));
            Hshape.Add(new Vector3(2.5f, 0.5f, 1.5f));
            Hshape.Add(new Vector3(1.5f, 0.5f, 1.5f));

            Hshape.Add(new Vector3(0.5f, 0.5f, 2.5f));
            Hshape.Add(new Vector3(0.5f, 0.5f, 1.5f));
            Hshape.Add(new Vector3(0.5f, 0.5f, 0.5f));

            
           
            for(int i=0; i < NumberOfMaterials; i++) {
                GameObject target = Instantiate(Target, Hshape[i], Quaternion.identity);
                target.transform.SetParent(TargetRoot.transform); 
                SpawnedTargets.Add(target);
            }
        }
        else if(Randomized) {
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
        HandleAgentsObservation();
    }

    // TODO: HandleAgent is getting called twice by both agents, but this fucntion handles for both agents
    void HandleAgentsObservation() {
        int[,] obs = new int[5, 5];
        int[,] obs1 = new int[5, 5];
        for(int m = 0; m < obs.GetLength(0); m++)
        {
            for(int n = 0; n < obs.GetLength(1); n++)
            {
                obs[m,n] = 1;
                obs1[m,n] = 1;
            }
        }
       
        Vector3 callingAgent1Pos = new Vector3(BlockAgents[0].transform.position.x+ (GridSize/2), 
                                                BlockAgents[0].transform.position.y,
                                                BlockAgents[0].transform.position.z+ (GridSize/2));
        Vector3 callingAgent2Pos = new Vector3(BlockAgents[1].transform.position.x+ (GridSize/2), 
                                                BlockAgents[1].transform.position.y,
                                                BlockAgents[1].transform.position.z+ (GridSize/2));
        foreach(var gameObject in SpawnedBlocks) {
            Vector3 materialPos = new Vector3(gameObject.transform.position.x+ (GridSize/2),
                                                gameObject.transform.position.y,
                                                gameObject.transform.position.z+ (GridSize/2));
            float materialDst = Vector3.Distance(callingAgent1Pos, materialPos);
            if(BlockAgents[0].GetComponent<BlockAgent>().Block != null) {
                if(materialDst <= 2.83f && !GameObject.ReferenceEquals(gameObject, BlockAgents[0].GetComponent<BlockAgent>().Block)) {
                    int xPos = (int)(materialPos.x - callingAgent1Pos.x);
                    int zPos = (int)(materialPos.z - callingAgent1Pos.z);
                    xPos = (xPos) + 2;
                    zPos = 2 - (zPos);
                    obs[zPos, xPos] = 0;
                }
            }
            
            float material2Dst = Vector3.Distance(callingAgent2Pos, materialPos);
            if(BlockAgents[1].GetComponent<BlockAgent>().Block != null) {
                if(material2Dst <= 2.83f && !GameObject.ReferenceEquals(gameObject, BlockAgents[1].GetComponent<BlockAgent>().Block)) {
                    int xPos = (int)(materialPos.x - callingAgent2Pos.x);
                    int zPos = (int)(materialPos.z - callingAgent2Pos.z);
                    xPos = (xPos) + 2;
                    zPos = 2 - (zPos);
                    obs1[zPos, xPos] = 0;
                }
            }
           
        }
        
        float agent12Dst = Vector3.Distance(callingAgent1Pos, callingAgent2Pos);
        float agent21Dst = Vector3.Distance(callingAgent2Pos, callingAgent1Pos);
        if(agent12Dst != 0 && agent12Dst <= 2.83f) {
            int xPos = (int)(callingAgent2Pos.x - callingAgent1Pos.x);
            int zPos = (int)(callingAgent2Pos.z - callingAgent1Pos.z);
            xPos = (xPos) + 2;
            zPos = 2 - (zPos);
            obs[zPos, xPos] = 2;
        }
        if(agent21Dst != 0 && agent21Dst <= 2.83f) {
            int xPos = (int)(callingAgent1Pos.x - callingAgent2Pos.x);
            int zPos = (int)(callingAgent1Pos.z - callingAgent2Pos.z);
            xPos = (xPos) + 2;
            zPos = 2 - (zPos);
            obs1[zPos, xPos] = 2;
        }
        
        // string arrayString = "";
        // arrayString += BlockAgents[0].gameObject.name;
        // arrayString += "\n";
        // for (int m = 0; m < obs.GetLength(0); m++)
        // {
        //     for (int n = 0; n < obs.GetLength(1); n++)
        //     {
        //             arrayString += string.Format("{0} ", obs[m, n]);
        //     }
        //     arrayString += System.Environment.NewLine + System.Environment.NewLine;
        // }
        // Debug.Log(arrayString);

        // string arrayString2 = "";
        // arrayString2 += BlockAgents[1].gameObject.name;
        // arrayString2 += "\n";
        // for (int m = 0; m < obs1.GetLength(0); m++)
        // {
        //     for (int n = 0; n < obs1.GetLength(1); n++)
        //     {
        //             arrayString2 += string.Format("{0} ", obs1[m, n]);
        //     }
        //     arrayString2 += System.Environment.NewLine + System.Environment.NewLine;
        // }
        // Debug.Log(arrayString2);

        BlockAgents[0].GetComponent<BlockAgent>().AgentObservation = obs;
        BlockAgents[1].GetComponent<BlockAgent>().AgentObservation = obs1;
    }
}
