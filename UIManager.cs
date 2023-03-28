using System.Collections;
using System.Collections.Generic;  
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class UIManager : MonoBehaviour
{
    public string redScore;
    public string blueScore;

    public int redGoal;
    public int blueGoal;

    // panel on table
    public GameObject Cam1Panel;

    // panel beside unity-chan <3
    public GameObject Cam2Panel;

	public TMP_Text redScoreText;
    public TMP_Text blueScoreText;

    public void score(string team)
    {
        if (team == "Red")
        {
            redGoal += 1; 
            redGoal = int.Parse(redScore);
            //redScoreText.text = "RED SCORE: " + redScore;
            Cam2Panel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = redScore;
        }

        if (team == "Blue")
        {
            blueGoal += 1; 
            int.Parse(blueScore);
            Cam2Panel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = blueScore;
        }
        
    } // end of Score function

} // end of CLASS 