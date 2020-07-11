using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
  private bool m_bCollidedWithPlayer = false;

  public void SetCollidedWithPlayer()
  {
    m_bCollidedWithPlayer = true;
  }
  public bool HasCollidedWithPlayer()
  {
    return m_bCollidedWithPlayer;
  }
}
