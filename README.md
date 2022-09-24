### Multi Agent Collective Construction Reinforcement Learning Research Project - LAS Special Topics UB CSE 610 Under Dr. Karthik Dantu & Dr. Souma Chowdhury

### Demo 
Demo for 2 agents with agent collision switched off and material collsion turned on. <br>
https://user-images.githubusercontent.com/17924429/192086872-545bc870-935b-4b99-9d2e-d167f365dfce.mp4

### Installation
*	Install Unity version > 2021
*	Install ML Agents toolkits from package installer in unity
*	pip install all needed packages using requirements.txt
*	Open one of the projects in the folder using Unity Editor

### PPO configuration file
Hyperparameters for PPO can be tuned from here <br>
``` root/config/config.yaml ```

### Commands 
To train any specific environment use <br>
``` mlagents-learn config/config.yaml --run-id={UNIQUE_NAME} –force ```

To see tensorboard results use <br>
``` tensorboard --logdir {LOG_DIR_NAME} --port 6006 ```

### Environment
Environment is majorly made of two elements – env controllers, which controls the environment initialization, resetting, agents’ data collection. Agent game objects, acts as individual entities that can be duplicated, the reward structure will need to change. Different stages of environment have different versions of parameters that can be configured.

These are the most common ones
Randomize – A Boolean that if checks randomize all target location gets randomly initialized
In Heuristics – It is a mode in toolkit, which allows to use give commands to move agents during testing, without starting training. To use environment in heuristics, select all agents, change their behavior type to Heuristics, and then in environmental controller select this Boolean.
Heuristic Index – This is used to select which agent to control during the heuristic mode.
Show Cumulative Rewards – Displays the cumulative reward for specific step
Max Timesteps – Maximum Timesteps for all agents cumulatively. The environment is configured to reset timesteps once agents have placed blocks in multiples of 4.
