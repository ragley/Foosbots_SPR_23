using System.Collections;
using System.Collections.Generic;  
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

/* 
    TODO: 
        - Avg goals per minute
        - avg ball possession to score time
        - avg time to score
        - shot acccuracy 
*/
public class UIManager : MonoBehaviour 
{

    private int redGoal;
    private int blueGoal;
    private int episodeCount;
    private float rGpEp; // red goals per min
    private float bGpEp; // blue goals per min 

    // // panel on table
    // public GameObject Cam1Panel;

   
    public GameObject Cam2Panel; // panel beside unity-chan <3
    //public TableEnvHandler tab;
	public TMP_Text redScoreText;
    public TMP_Text blueScoreText;

	public TMP_Text rGpEpText;
	public TMP_Text bGpEpText;

    public TMP_Text blueCumRewardText;
    public TMP_Text redCumRewardText;
    public TMP_Text episodeCountText;




    void Start()
    {
        redGoal = 0; 
        blueGoal = 0;
        episodeCount = 0;

        rGpEp = 0;
        bGpEp= 0;

        redScoreText.text = "Red score: " + redGoal;
        blueScoreText.text =  "Blue score: " + blueGoal;
        episodeCountText.text = "Episode:\n" + episodeCount;
        bGpEpText.text = "Avg Blue Score \nPer Episode:  " + bGpEp;
        rGpEpText.text = "Avg Red Score \nPer Episode:  " + rGpEp;

    }

    public void scoreRed()//float elapsedTime)
    {
        redGoal += 1; 
        rGpEp = ((float)redGoal / (float)episodeCount);

        //Debug.Log("%%%%%% \t e-time passed into RED score func:\t" + elapsedTime);
        //Debug.Log("%%%%%% %%% \t Red Goal per Min:\t" + rGpEp);

        redScoreText.text = "Red score: " + redGoal;

        rGpEpText.text = "Avg Red Score \nPer Episode:  " + rGpEp;
    } //end of scoreRed function

    public void scoreBlue()//float elapsedTime)
    {   
        blueGoal += 1; 
        bGpEp = ((float)blueGoal / (float)episodeCount);
        //Debug.Log("%%%%%% \t e-time passed into BLUE score func:\t" + elapsedTime);
        //Debug.Log("%%%%%% %%% \t Blue Goal per Min:\t" + bGpEp);

        blueScoreText.text =  "Blue score: " + blueGoal;
        bGpEpText.text = "Avg Blue Score \nPer Episode:  " + bGpEp;
    } // end of scoreBlue function

    public void episodeIncrement()//float elapsedTime)
    {   
        episodeCount += 1; 
        episodeCountText.text =  "Episode:\n" + episodeCount;
       
    } 
    public void updateCumReward(float cumRewardBlue, float cumRewardRed)
    {
        blueCumRewardText.text = "Current Ep.\nReward: " + (Mathf.Round(cumRewardBlue * 10000) / 10000);
        redCumRewardText.text = "Current Ep.\nReward: " + (Mathf.Round(cumRewardRed * 10000) / 10000);
    }

} // end of CLASS 