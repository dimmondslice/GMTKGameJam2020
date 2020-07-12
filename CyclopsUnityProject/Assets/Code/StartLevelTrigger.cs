using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLevelTrigger : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    CyclopsPlayer rPlayer = other.GetComponentInParent<CyclopsPlayer>();
    if (rPlayer)
    {
      rPlayer.StartEyeBlast();
      //Destroy(gameObject);
    }
  }
}
