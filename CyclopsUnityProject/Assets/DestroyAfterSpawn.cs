using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSpawn : MonoBehaviour
{
  public float m_dDurationSec;

  private float m_secSinceSpawn = 0.0f;

  private void Update()
  {
    m_secSinceSpawn += Time.deltaTime;

    if (m_secSinceSpawn > m_dDurationSec)
    {
      Destroy(gameObject);
    }
  }
}
