using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateWheel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lp = GetComponentInChildren<MeshRenderer>().transform.localPosition;
        lp = new Vector3(lp.x, GetComponentInChildren<WheelSuspension>().contactDepth, lp.z);
        GetComponentInChildren<MeshRenderer>().transform.localPosition = lp;
    }
}
