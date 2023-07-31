using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public Canvas buildingCanvas;
    public GameObject workersCont;

    void Start()
    {
        workersCont = GameObject.Find("Worker Container");

        foreach (Transform childTransform in workersCont.transform)
        {
            childTransform.GetComponent<Worker>().TargetsUpdate(this.gameObject);
        }
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.gameObject.name);
            if (hit.transform.gameObject == gameObject)
            {
                buildingCanvas.gameObject.SetActive(true);
                this.enabled = false;
            }
            else
            {
                buildingCanvas.gameObject.SetActive(false);
            }
        }
        else
        {
            buildingCanvas.gameObject.SetActive(false);
        }
    }
}
