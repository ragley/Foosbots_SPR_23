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
    public SelfPlayAgentJoint redAgent; // new (changed class name, was SelfPlayAgentInfTest. Didnt want to create new envcontrol file '-_- ) 
    public GameObject blueTeam;
    public SelfPlayAgentJoint blueAgent; // new (changed class name, was SelfPlayAgentInfTest. 8==D ) 
   

    public Ball ball;
    Rigidbody ballRB;
    
    public UIManager ui;

    // BOOLS



    // INTS
    public int MaxEnvSteps;
    public int resetTimer;
    int idleTimer;
    public int maxIdleTime;
    public int epCount;
    public int decInterval;
    
    // FLOATS
    public float autoKickStrength;
    public float velocityThreshold;

    // public float elapsedTime;

    
    // VECTORS

    // Agent Parameters
    public float spinPenaltyMult;
    public float shotRewardMultiplier;
    public float kickReward;
    public float goalRewardValue;
    public float timeStepPenalty;
    public float timeStep;
    public float maxAngularVelocity;
    public bool useSpinPenalty, useSingleShotReward, useContShotReward;
    public bool useNegShotPenalty, useKickReward, usePossessionEval, useTimePenalty;
    public bool regKicks, regGoals;
    public bool isRedPlaying, isBluePlaying;
    public enum playType {touch_ball, reg_play};
    public playType Play_Type;
    public SelfPlayAgentJoint.InputType inputType;
    public Ball.CollisionRod collisionRod;
    public float hitAngularVelocity;
    public float hitVelocityReward;
    
    public float lesson = 0f;

    //TODO: Make all scoring controls settable here

    // Start is called before the first frame update
    void Start()
    {


        ballRB = ball.GetComponent<Rigidbody>();        
        ball.autoKickStrength = autoKickStrength;
        Application.targetFrameRate = 60;
        
        timeStepPenalty = -1f * (1f / (MaxEnvSteps / decInterval));
        lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("reward_switch", 0.0f);
        //print(lesson);
        ResetScene();        
        setAgentVars();
        // Start session off with ball in center
        epCount = 0;
        ball.AutoKick();
        // elapsedTime = 0;

    }

    void FixedUpdate()
    {
        // Increment Reset Timer
        resetTimer += 1;
        // resetTimerInterval / 12
        // if((resetTimer % decInterval) == 0)
        // {
        //     haltVelocity();
        // }

        // Update UI 
        lesson = Academy.Instance.EnvironmentParameters.GetWithDefault("reward_switch", 0.0f);
        //print(lesson);

        if (lesson == 4.0f || lesson == 5.0f)
        {
            Play_Type = playType.reg_play;
            
            if (lesson == 4.0f)
            {
                useContShotReward = true;
                useNegShotPenalty = true;
            }
            if (lesson == 5.0f)
            {
                useContShotReward = false;
                useNegShotPenalty = true;
            }

        } else
        {
            Play_Type = playType.reg_play;
        }

        ui.updateCumReward(blueAgent.GetCumulativeReward(), redAgent.GetCumulativeReward());
        if (Play_Type == playType.reg_play) {
            // Ball in goal
            if (ball.inGoalColor == redAgent.allyColor)
            {
                // Update UI
                ui.scoreBlue();//elapsedTime);
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
                blueAgent.SetReward(0f);
                redAgent.SetReward(0f);
                redAgent.EndEpisode();
                blueAgent.EndEpisode();
                ResetScene();
            }

        }
        // Training set up for blue side only.
        if (Play_Type == playType.touch_ball)
        { 
            if (ball.kicked == true){
                if (ball.lastKickedColor == blueAgent.allyColor)
                {
                    redAgent.EndEpisode();
                    ui.scoreBlue();
                    
                    hitAngularVelocity = ball.hitAngularVelocity;
                    hitVelocityReward = hitAngularVelocity / maxAngularVelocity;
                    
                    if (lesson == 0.0f)
                    {
                        blueAgent.AddReward(1f);
                    } else if(lesson == 1.0f)
                    {
                        if (hitAngularVelocity > 0)
                        {
                            blueAgent.AddReward(1f);
                        }
                    }else if(lesson == 2.0f)
                    {
                        blueAgent.AddReward(hitAngularVelocity);
                    }

                    blueAgent.EndEpisode();
                    ResetScene();
                }
            }

            if (ball.inGoalColor == redAgent.allyColor)
            {

                redAgent.EndEpisode();
                blueAgent.EndEpisode();
                ResetScene();


            } else if(ball.inGoalColor == blueAgent.allyColor)
            {

                redAgent.EndEpisode();
                blueAgent.EndEpisode();
                ResetScene();

            } 
            
            if (ballRB.velocity.magnitude <= velocityThreshold)
            {
                idleTimer += 1;
                if (idleTimer > maxIdleTime){
                    
                    ball.AutoKick();
                    idleTimer = 0;
                }
            }
            if (resetTimer >= MaxEnvSteps && MaxEnvSteps > 0)
            {
                //EndSummary("Draw.", "Draw.");
                redAgent.EndEpisode();
                blueAgent.EndEpisode();
                ResetScene();
            }
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

        // if (Play_Type == playType.touch_ball)
        // {
        //     ball.ResetFull(Random.Range(-0.00539f, 0.00539f), Random.Range(-0.00298f, 0.00298f));

        // }
        // else 
        // {
        //     ball.Reset();
        // }
        ball.ResetFull(Random.Range(-0.00539f, 0.00539f), Random.Range(-0.00298f, 0.00298f));
        
        ball.ballRB.angularDrag = Random.Range(0.90f, 1.1f) * 0.45f;
        hitAngularVelocity = 0f;
        hitVelocityReward = 0f;
        resetTimer = 0;
        setAgentVars();
        ui.episodeIncrement();
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
        redAgent.spinPenaltyMult = blueAgent.spinPenaltyMult = spinPenaltyMult;
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
        redAgent.inputType = blueAgent.inputType = inputType;
        redAgent.MaxEnvSteps = blueAgent.MaxEnvSteps = MaxEnvSteps;
        redAgent.useNegShotPenalty = blueAgent.useNegShotPenalty = useNegShotPenalty;
        redAgent.maxAngularVelocity = blueAgent.maxAngularVelocity = maxAngularVelocity;
        redAgent.isPlaying = isRedPlaying;
        blueAgent.isPlaying = isBluePlaying;
    
    }

    public void haltVelocity()
    {
            redAgent.haltVelocity();
            
            blueAgent.haltVelocity();          
            
            //print("set zero");
    }
}
