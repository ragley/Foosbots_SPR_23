using System.Collections;
using System.Collections.Generic;  
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

/* 
    TODO: 
        - average goals per minute
        - avg ball possession to score time
        - avg time to score
        - shot acccuracy 
*/
public class UIManager : MonoBehaviour 
{
    private int redGoal;
    private int blueGoal;

    // // panel on table
    // public GameObject Cam1Panel;

   
    public GameObject Cam2Panel; // panel beside unity-chan <3
    //public TableEnvHandler tab;
	public TMP_Text redScoreText;
    public TMP_Text blueScoreText;
    public TMP_Text blueCumRewardText;
    public TMP_Text redCumRewardText;


    void Start()
    {
        redGoal = 0; 
        blueGoal = 0;

        redScoreText.text = "Red score: " + redGoal;
        blueScoreText.text =  "Blue score: " + blueGoal;
    }

    public void scoreRed()
    {
        redGoal += 1; 
        redScoreText.text = "Red score: " + redGoal;
    } //end of scoreRed()

    public void scoreBlue()
    {   
        blueGoal += 1; 
        blueScoreText.text =  "Blue score: " + blueGoal;
    } // end of scoreBlue function

    public void updateCumReward(float cumRewardBlue, float cumRewardRed)
    {
        blueCumRewardText.text = "Current Ep. \n Reward: " + (Mathf.Round(cumRewardBlue * 10000) / 10000);
        redCumRewardText.text = "Current Ep. \n Reward: " + (Mathf.Round(cumRewardRed * 10000) / 10000);

    }

} // end of CLASS 