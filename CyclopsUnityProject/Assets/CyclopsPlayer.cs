using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[SelectionBase]
public class CyclopsPlayer : MonoBehaviour
{
  public float m_dJumpHeightMeters;
  public float m_dTimeToJumpPeakSec;

  // Privates
  private Rigidbody rRB;

  private float m_jumpTimer = 0;
  private float m_gravityAcc;
  private float m_initJumpVel;

  void Start()
  {
    rRB = GetComponent<Rigidbody>();
  }

  void FixedUpdate()
  {
    m_gravityAcc = -2 * m_dJumpHeightMeters / (m_dTimeToJumpPeakSec * m_dTimeToJumpPeakSec);
    m_initJumpVel = 2 * m_dJumpHeightMeters / m_dTimeToJumpPeakSec;

    Vector3 newVel = rRB.velocity;

    newVel.y += m_gravityAcc * Time.deltaTime;

    rRB.velocity = newVel;
  }
  public void OnJump(InputAction.CallbackContext context)
  {
    Vector3 newVel = rRB.velocity;
    newVel.y = m_initJumpVel;
    rRB.velocity = newVel;
  }
}
