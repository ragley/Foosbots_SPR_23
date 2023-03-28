using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;



public class TableEnvHandler : MonoBehaviour
{

    // GAMEOBJECTS
    public GameObject redTeam;    
    public SelfPlayAgentInfTest redAgent;
    public GameObject blueTeam;
    public SelfPlayAgentInfTest blueAgent;
   

    public Ball ball;
    Rigidbody ballRB;
    
    public UIManager ui;

    // BOOLS



    // INTS
    public int MaxEnvSteps;
    int resetTimer;
    int idleTimer;
    public int maxIdleTime;
    public int epCount;
    
    // FLOATS
    public float autoKickStrength;
    public float velocityThreshold;

    
    // VECTORS

    // Agent Parameters
    public float spinPenaltyMult;
    public float shotRewardMultiplier;
    public float kickReward;
    public float goalRewardValue;
    public float timeStepPenalty;
    public float timeStep;
    public bool useSpinPenalty, useSingleShotReward, useContShotReward, useKickReward, usePossessionEval, useTimePenalty;
    public bool regKicks, regGoals, inputPosition;
    public bool isRedPlaying, isBluePlaying;


    //TODO: Make all scoring controls settable here

    // Start is called before the first frame update
    void Start()
    {

        ballRB = ball.GetComponent<Rigidbody>();        
        ball.autoKickStrength = autoKickStrength;
        Application.targetFrameRate = 60;
        
        timeStepPenalty = -1f * (1f / MaxEnvSteps);

        setAgentVars();
        redAgent.Reset();
        blueAgent.Reset();
        
        // Start session off with ball in center
        ball.Reset(false);

        resetTimer = 0;

        epCount = 0;

    }

    void FixedUpdate()
    {
        // Increment Reset Timer
        resetTimer += 1;

        // Update UI 
        ui.updateCumReward(blueAgent.GetCumulativeReward(), redAgent.GetCumulativeReward());

        // Ball in goal
        if (ball.inGoalColor == redAgent.allyColor)
        {
            // Update UI
            ui.scoreBlue();
            // Set Reward Values
            redAgent.SetReward(-1f * goalRewardValue);
            blueAgent.AddReward(goalRewardValue);
            
            // Output summary to console
            //EndSummary("Red Loss. ", "Blue Win. ");
            
            // End episode and reset scene
            redAgent.EndEpisode();
            blueAgent.EndEpisode();
            ResetScene();


        } else if(ball.inGoalColor == blueAgent.allyColor)
        {
            ui.scoreRed();
            redAgent.AddReward(goalRewardValue);
            blueAgent.SetReward(-1f * goalRewardValue);
            
            //EndSummary("Red Win. ", "Blue Loss. ");
            
            redAgent.EndEpisode();
            blueAgent.EndEpisode();
            ResetScene();

        } 

        // Idle Timer
        if (ballRB.velocity.magnitude <= velocityThreshold)
        {
            idleTimer += 1;
            if (idleTimer > maxIdleTime){
                
                ball.AutoKick();
                idleTimer = 0;
            }
        }

        // Episode Reset 
        if (resetTimer >= MaxEnvSteps && MaxEnvSteps > 0)
        {
            
            //EndSummary("Draw.", "Draw.");
            redAgent.EndEpisode();
            blueAgent.EndEpisode();
            ResetScene();
        }

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetScene()
    {
        redAgent.Reset();
        blueAgent.Reset();
        ball.Reset();
        resetTimer = 0;
        setAgentVars();
        epCount += 1;
    }

    public void EndSummary(string endTypeRed, string endTypeBlue)
    {        
        // Print out the reward summaries
        print(blueAgent.SummaryStr(endTypeBlue));
        print(redAgent.SummaryStr(endTypeRed));

    }

    public void setAgentVars()
    {
        // Sync the variables between agents
        // If changed, they will sync to this file at the end of an episode
        // Remember to add them here if you want them to sync or otherwise they will potentially be different!
        redAgent.spinPenaltyMult = blueAgent.shotRewardMultiplier = spinPenaltyMult;
        redAgent.shotRewardMultiplier = blueAgent.shotRewardMultiplier = shotRewardMultiplier;
        redAgent.kickReward = blueAgent.kickReward = kickReward;
        redAgent.goalRewardValue = blueAgent.goalRewardValue = goalRewardValue;
        redAgent.timeStepPenalty = blueAgent.timeStepPenalty = timeStepPenalty;
        redAgent.timeStep = blueAgent.timeStep = timeStep;
        redAgent.useTimePenalty = blueAgent.useTimePenalty = useTimePenalty;
        redAgent.useSpinPenalty = blueAgent.useSpinPenalty = useSpinPenalty;
        redAgent.useSingleShotReward = blueAgent.useSingleShotReward = useSingleShotReward;
        redAgent.useContShotReward = blueAgent.useContShotReward = useContShotReward;
        redAgent.usePossessionEval = blueAgent.usePossessionEval = usePossessionEval;
        redAgent.useKickReward = blueAgent.useKickReward = useKickReward;
        redAgent.regKicks = blueAgent.regKicks = regKicks;
        redAgent.regGoals = blueAgent.regGoals = regGoals;
        redAgent.inputPosition = blueAgent.inputPosition = inputPosition;
        redAgent.MaxEnvSteps = blueAgent.MaxEnvSteps = MaxEnvSteps;
    
    }


}
