using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class Worker : MonoBehaviour
{
    // Resources we need to action
    private GameResourceSO carried;
    [HideInInspector]
    public GameResourcesList resourceList;
    // Building we interact with, it should NOT have anything assigned
    [HideInInspector]
    public GameObject container;

    // Variables used for logic
    public bool isCarrying = false;
    private bool product = false;
    // These 3 guys below show an error message within logs if left on private so we're hiding them to leave inspector clear
    [HideInInspector]
    public List<GameObject> extractionContainer;
    [HideInInspector]
    public List<GameObject> productionContainer;
    [HideInInspector]
    public List<GameObject> storageContainer;

    // Navigating and animation. Crate belongs here as for some reason it's not part of carrying animation and we need to make it one
    public NavMeshAgent agent;
    public Animator animator;
    public GameObject crate;

    // Things used for resourceUI
    public TMP_Text resourceText;
    public Canvas workerCanvas;
    private float timer = 0f;
    private float timeLimit = 3f;

   void Start()
    {
        extractionContainer = new List<GameObject>();
        productionContainer = new List<GameObject>();
        storageContainer = new List<GameObject>();

        container = null;
        crate.SetActive(false);
        resourceText.enabled = false;

        agent.stoppingDistance = 3f;
        foreach (Transform childTransform in GetComponentInParent<BuildingsContainerHolder>().buildingCont.transform)
        {
            TargetsUpdate(gameObject);
        }
    }


    void Update()
    {
        resourceUI();

        // When false go pick up wood
        if(isCarrying == false && extractionContainer.Count > 0)
        {
            Extraction();
            return;
        }

        if (isCarrying == true && product == false && productionContainer.Count > 0)
        {
            Production();
            return;
        } 
        else if(isCarrying && product && storageContainer.Count > 0)
        {
            Storage();
            return;
        }        
    }

    // Store all possible targets to minimize searching
    public void TargetsUpdate(GameObject Target)
    {
        switch (Target.tag)
        {
            case "Extraction":
                extractionContainer.Add(Target);
                break;

            case "Production":
                productionContainer.Add(Target);
                break;

            default:
                storageContainer.Add(Target);
                break;
        }
    }

    /*
    Sorting relevant lists would optimize processes below if we checked first ~20 elements however it would require slight tweaks to the logic below
    and there is really no need for this type of resolution on a project where all relevant lists combined aren't going to exceed 10 elements
    */

    private void Extraction()
    {
        if (container != null)
        {
            // Do the important thing
            if (Vector3.Distance(this.transform.position, container.transform.position) <= 3f)
            {
                resourceList = container.GetComponent<GameResourcesList>();
                animator.SetBool("isMoving", false);
                carried = container.GetComponent<ExtractionBuilding>().resourceSO;

                if (resourceList.TryUse(carried, 1))
                {
                    container = null;
                    animator.SetBool("carrying", true);
                    crate.SetActive(true);
                    Production();
                } 
                else
                {
                    Debug.Log("Waiting for wood");
                    return;
                }
                
                isCarrying = true;
                return;
            }
            return;
        }

        int index = 0;
        // this needs to be high
        float sample = 9999999f;
        float aDistance;

        // Find closest building
        foreach (GameObject test in extractionContainer)
        {
            Transform subject = test.transform;
            aDistance = Vector3.Distance(this.transform.position, subject.position);

            if(aDistance < sample)
            {
                sample = aDistance;
                index = extractionContainer.IndexOf(test);
            }
        }

        // Go to the building
        agent.SetDestination(extractionContainer[index].transform.position);
        animator.SetBool("isMoving", true);
        container = extractionContainer[index];
    }

    /*
    HELLO DON'T TOUCH Production() OR Storage() BEFORE READING:
    Methods below cause an error to pop up in log. Issue is within GameResourcesList and how it's sctructured since CreateResource() doesn't intake an ammount.
    Would have to speak with Designers on how to best fix this as giving a hard vallue there could cause problems depending on the big picture
    and intaking ammount into CreateResource() would also cause potential issues in the future, sadly it's Sunday and causes no issues
    to functionality so it waits.
    */
    private void Production()
    {
        if (container != null)
        {
            // Do the important thing
            if (Vector3.Distance(this.transform.position, container.transform.position) <= 3f)
            {
                resourceList = container.GetComponent<GameResourcesList>();
                animator.SetBool("isMoving", false);

                resourceList.Add(carried, 1);
                animator.SetBool("carrying", false);
                crate.SetActive(false);
                carried = container.GetComponent<ProductionBuilding>().outputResourceSO;

                if (storageContainer.Count > 0 && resourceList.TryUse(carried, 1))
                {
                    product = true;
                    container = null;
                    animator.SetBool("carrying", true);
                    crate.SetActive(true);
                    Storage();
                } 
                else
                {
                    isCarrying = false;
                    product = false;
                    carried = null;
                    container = null;
                }

                return;
            }
            return;
        }

        int index = 0;
        // this needs to be ridiculously high
        float sample = 9999999f;
        float aDistance;

        // Find closest building
        foreach (GameObject test in productionContainer)
        {
            Transform subject = test.transform;
            aDistance = Vector3.Distance(this.transform.position, subject.position);

            if(aDistance < sample)
            {
                sample = aDistance;
                index = productionContainer.IndexOf(test);
            }
        }

        // Go to the building
        agent.SetDestination(productionContainer[index].transform.position);
        animator.SetBool("isMoving", true);
        container = productionContainer[index];
    }

    private void Storage()
    {
        if (container != null)
        {
            // Do the important thing
            if (Vector3.Distance(this.transform.position, container.transform.position) <= 3f)
            {
                //add chair to storage
                resourceList = container.GetComponent<GameResourcesList>();
                animator.SetBool("isMoving", false);

                resourceList.Add(carried, 1);
                animator.SetBool("carrying", false);
                crate.SetActive(false);

                isCarrying = false;
                container = null;
                return;
            }
            return;
        }

        int index = 0;
        // this needs to be ridiculously high
        float sample = 9999999f;
        float aDistance;

        // Find closest building
        foreach (GameObject test in storageContainer)
        {
            Transform subject = test.transform;
            aDistance = Vector3.Distance(this.transform.position, subject.position);

            if(aDistance < sample)
            {
                sample = aDistance;
                index = storageContainer.IndexOf(test);
            }
        }

        // Go to the building
        agent.SetDestination(storageContainer[index].transform.position);
        animator.SetBool("isMoving", true);
        container = storageContainer[index];
    }

    public void resourceUI()
    {
        timer += Time.deltaTime;
        if (timeLimit <= timer)
        {
            resourceText.enabled = false;
        }

        if (Input.GetMouseButtonDown(1) && carried != null  && isCarrying)
        {
            Ray worker = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(worker, out var hitInfo);

            string resource = carried.name;

            if (Vector3.Distance(hitInfo.point, this.transform.position) <= 1f)
            {
                timer = 0f;
                resourceText.text = "Worker's carrying:\r\n " + resource.Replace(" Resource", null);
                resourceText.enabled = true;
                Debug.Log("hit worker");
            }
        }
    }
}
