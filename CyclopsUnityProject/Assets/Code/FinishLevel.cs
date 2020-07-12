using System.Collections.Generic;
using UnityEngine;

public class FinishLevel : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    CyclopsPlayer rPlayer = other.GetComponentInParent<CyclopsPlayer>();
    if (rPlayer)
    {
      rPlayer.m_dSHOOTEYEBLASTS = false;

      rPlayer.SetFinishedLevel();
      Destroy(transform.root.gameObject);
    }
  }
}
