using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class AIController : MonoBehaviour
{
    GameObject[] goalLocations;
    GameObject[] panicGoalLocations;
    NavMeshAgent agent;
    float detectionRadius = 15f;
    float fleeRadius = 12f;
    [SerializeField] private Material panicColor;
    [SerializeField] private Material infectedColor;
    [SerializeField] private Material expressiveColor;

    //OCEAN traits
    private float open, consc, extro, agree, neuro; 

    //Variables to Calculate emotions
    private float empathy, fear;   
    private float thresholdVal;                        

    //Stats
    private GameObject[] totalAgents, directAgents, expAgents, indirectAgents;


    void Awake()
    {

        totalAgents = GameObject.FindGameObjectsWithTag("susceptible");
        //Set OCEAN values
        open = getRandomNumber(0f, 1f);
        consc = getRandomNumber(0f, 1f);
        extro = getRandomNumber(0f, 1f);
        agree = getRandomNumber(0f, 1f);
        neuro = getRandomNumber(0f, 1f);

        empathy = 0.5f;

        //Calculate PAD based on OCEAN
        // CalculatePAD();

        //Set expressiveness Threshold
        float meanThreshold = 0.5f - (0.5f * extro);
        float sdThreshold = meanThreshold / 10;

        thresholdVal = getRandomNumber(meanThreshold, sdThreshold);

        // empathy = (0.354f * open) + (0.177f * consc) + (0.135f * extro) + (0.312f * agree) + (0.021f * neuro);
        
        
    
    }

    private float getRandomNumber(float mean, float sd)
    {
        MathNet.Numerics.Distributions.Normal normalDist = new MathNet.Numerics.Distributions.Normal(mean, sd);
        float randomNumber = (float) normalDist.Sample();

        if(randomNumber < -1)   return -1;
        if(randomNumber > 1)    return 1;

        return randomNumber;
    }

    public void DetectNewObstacle(Vector3 position) {
        if(Vector3.Distance(position, this.transform.position) < detectionRadius)
        {
            fear = SetPanic(neuro, consc);
            
            // Debug.Log(panic + " panic");
            panicGoalLocations = GameObject.FindGameObjectsWithTag("exit");
            Vector3 fleeDirection = (this.transform.position - position).normalized;

            Vector3 newgoal = this.transform.position + fleeDirection * fleeRadius;

            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(newgoal, path);

                agent.SetDestination(panicGoalLocations[Random.Range(0, panicGoalLocations.Length)].transform.position);
                agent.speed = 6 + (fear * 3);
                agent.angularSpeed = 500;
                transform.GetComponent<Renderer>().material = panicColor;
                transform.gameObject.tag = "direct_infected";
                SetDirectExpressiveAgent();
        }
    }



   
    void ResetAgent()   {
        agent.speed = SetAgentSpeed(extro);
        agent.angularSpeed = 120;
        agent.ResetPath();
        
    }

    // Start is called before the first frame update
    void Start()
    {


        goalLocations = GameObject.FindGameObjectsWithTag("goal");
        agent = this.GetComponent<NavMeshAgent>();
        agent.SetDestination(goalLocations[Random.Range(0, goalLocations.Length)].transform.position);
        agent.speed = SetAgentSpeed(extro);
        // Debug.Log(agent.radius);

        ResetAgent();
        
    }

    void FixedUpdate()
    {
        PersonalSpace();
        FindOtherAgents();
    }

    // Update is called once per frame
    void Update()
    {
        if(agent.remainingDistance < SetExploration() && agent.speed < 4f  )   {
            agent.SetDestination(goalLocations[Random.Range(0, goalLocations.Length)].transform.position);
        } 

    }

    private void FindOtherAgents()
    {
        List<GameObject> closeAgent = new List<GameObject>();

        if(this.gameObject.tag == "susceptible")
        {
            GameObject[] directAgents = GameObject.FindGameObjectsWithTag("direct_expressive");
            GameObject[] indirectAgents = GameObject.FindGameObjectsWithTag("indirect_expressive");
            List<GameObject> allAgents = new List<GameObject>();
            foreach(var a in directAgents)
            {
                allAgents.Add(a);
            }

            foreach(var a in indirectAgents)
            {
                allAgents.Add(a);
            }
            
            for(int i = 0; i < allAgents.Count; i++)   
            {
                if(allAgents[i] == this.agent)  continue;
                float relativePos = Vector3.Distance(allAgents[i].transform.position, this.transform.position);
                if(relativePos <= 10f)
                {
                    closeAgent.Add(allAgents[i]);
                }
            }

            if(closeAgent.Count > 0)    
            {
                float dose = 0;
                foreach(var agent in closeAgent)
                {   
                    dose += getRandomNumber(0.15f, 0.01f) * agent.GetComponent<AIController>().getFear();

                }

                if(dose >= SusceptibilityThreshold(empathy))    SetPanicEmotion();
            }
        }
    }

    private float SusceptibilityThreshold(float empathy)
    {
        float mean = 0.5f - (0.5f * empathy);
        float sd = mean/10;

        return getRandomNumber(mean, sd);
    }

    private float SetAgentSpeed(float extro)
    {
        return extro + 2.5f;
    }

    private float SetExploration()
    {
        return (open + 1) * 5;
    }

    private float SetPanic(float neuro, float consc)
    {
        // Debug.Log("Consc" + consc);
        float consFunc;
        if(consc >= 0)
        {
            consFunc = (-2 * consc) + 2;
        }
        else
        {
            consFunc = 0;
        }
       
        float panic = (0.5f * neuro) +  (0.5f * consFunc);

        return Mathf.Abs(panic);

    }

    private void PersonalSpace()
    {
        if(agent.gameObject.tag == "susceptible")
        {
            if(extro >= -1f && extro <= -0.3f)      agent.radius = 0.8f;
            else if(extro > -0.3f && extro <= 0.3f) agent.radius = 0.7f;
            else                                    agent.radius = 0.5f;
        }
        else
        {
            agent.radius *= 0.4f;
        }
    }

    public float getFear()    
    {
        return fear;
    }

    private void SetPanicEmotion()
    {
        fear = SetPanic(neuro, consc);
        panicGoalLocations = GameObject.FindGameObjectsWithTag("exit");
        agent.SetDestination(panicGoalLocations[Random.Range(0, panicGoalLocations.Length)].transform.position);
        agent.speed = 6 + (fear * 3);
        agent.angularSpeed = 500;
        transform.GetComponent<Renderer>().material = infectedColor;
        transform.gameObject.tag = "indirect_infected";
        SetExpressiveAgent();
    }

    private void SetDirectExpressiveAgent()
    {
        if(getFear() > thresholdVal)    
        {
            transform.gameObject.tag = "direct_expressive";
            transform.GetComponent<Renderer>().material = expressiveColor;
        }
        
    }

    private void SetExpressiveAgent()
    {
        if(getFear() > thresholdVal)    
        {
            transform.gameObject.tag = "indirect_expressive";
            transform.GetComponent<Renderer>().material = expressiveColor;
        }
        
    }
}
