using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    public GameObject cam1;
    public GameObject cam2;
    public bool cam1enabled;
    public bool cam2enabled;
    
 
    public void Start() 
    {
        cam1.SetActive(true);
        cam1enabled = true;
        cam2.SetActive(false);
        cam2enabled = false;
     }
 
    public void Update() 
    {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (cam1enabled == true){
                cam1enabled = false;
                cam1.SetActive(false);
                cam2.SetActive(true);
                cam2enabled = true;
            }
            else{
                cam1enabled = true;
                cam1.SetActive(true);
                cam2.SetActive(false);
                cam2enabled = false;
            }
        }
            
    }
}
