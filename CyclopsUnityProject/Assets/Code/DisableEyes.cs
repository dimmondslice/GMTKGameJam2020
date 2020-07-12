using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEyes : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        CyclopsPlayer rPlayer = other.GetComponentInParent<CyclopsPlayer>();
        if (rPlayer)
        {
            Debug.Log("THERE IS A PLAYER");
            rPlayer.m_dSHOOTEYEBLASTS = false;
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
