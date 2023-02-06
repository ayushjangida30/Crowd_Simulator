using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistics : MonoBehaviour
{   
    private GameObject[] totalAgents;
    private GameObject[] directAgents;
    private GameObject[] directExpressiveAgents;
    private GameObject[] indirectExpressiveAgents;
    private GameObject[] indirectAgents;
    private float val;
    private double b;



    // Start is called before the first frame update
    void Start()
    {
        totalAgents = GameObject.FindGameObjectsWithTag("susceptible");
        val = 0;
        
    }

    void FixedUpdate()
    {
        val += Time.deltaTime;
        b = System.Math.Round (val, 2);
        
    }

    // Update is called once per frame
    void Update()
    {
        directAgents = GameObject.FindGameObjectsWithTag("direct_infected");
        directExpressiveAgents = GameObject.FindGameObjectsWithTag("direct_expressive");
        indirectExpressiveAgents = GameObject.FindGameObjectsWithTag("indirect_expressive");
        indirectAgents = GameObject.FindGameObjectsWithTag("indirect_infected");

       if(b % 2f == 0)
       {
           Debug.Log("Total agents: " + totalAgents.Length + "\n Directly infected Agents: " + (directAgents.Length +  directExpressiveAgents.Length) + " \n Indirectly infected Agents: " + (indirectAgents.Length + indirectExpressiveAgents.Length) + "\n Expressive Agents: " + (directExpressiveAgents.Length + indirectExpressiveAgents.Length) + "\n Timer: " + b);
       } 
        
    }
}
