using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public GameObject trackTraveler;
    public Vector3[] trackNodes;
    public float trackSpeed = 10f;
    public float turnSpeed = 1f;

    private int currentNode = 0;
    private bool isForward = true;

    // Start is called before the first frame update
    void Start()
    {
        trackTraveler.transform.position = trackNodes[0];
    }

    private void OnDrawGizmos()
    {
        int length = trackNodes.GetLength(0);
        for(int i = 0; i < length; i++)
        {
            Gizmos.DrawWireSphere(trackNodes[i], 1);
            if(i < length-1)
            {
                Gizmos.DrawLine(trackNodes[i], trackNodes[i + 1]);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 VectorToPlayer = trackTraveler.transform.position - trackNodes[currentNode];
        float distance = Vector3.SqrMagnitude(VectorToPlayer);
        if (distance > 0.01f)
        {
            Vector3 facing = trackNodes[currentNode] - trackTraveler.transform.position;
            if(!isForward)
            {
                facing = new Vector3(0, 0, 0) - facing;
            }
            
            //trackTraveler.transform.forward = new Vector3(facing.x, facing.y, facing.z);
            trackTraveler.transform.forward = Vector3.RotateTowards(trackTraveler.transform.forward, facing, turnSpeed * Time.deltaTime, 0f);
            trackTraveler.transform.position = Vector3.MoveTowards(trackTraveler.transform.position, trackNodes[currentNode], Time.deltaTime * trackSpeed);           
        }
        else
        {
            trackTraveler.transform.position = trackNodes[currentNode];
            if(isForward)
            {
                if (currentNode < trackNodes.GetLength(0) - 1)
                {
                    currentNode++;
                }
                else
                {
                    isForward = false;
                }
            }
            else
            {
                if (currentNode > 0)
                {
                    currentNode--;
                }
                else
                {
                    isForward = true;
                }
            }
        }
    }
}
