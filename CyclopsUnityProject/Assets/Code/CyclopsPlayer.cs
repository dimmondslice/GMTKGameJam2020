using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class CyclopsPlayer : MonoBehaviour
{
  public float m_dJumpHeightMeters;
  public float m_dTimeToJumpPeakSec;

  public float m_dTopFwdSpeedMPS;
  public float m_dTopStrafeSpeedMPS;
  public float m_dFrictionPercentPerSec;

  public float m_dLookPitchSensitivity;
  public float m_dLookYawSensitivity;

  public float m_dSplosionFalloffDist;
  public float m_dMaxSplosionKnockbackSpeed;

  public Transform m_rCameraTr;

  // Privates
  private Rigidbody m_rRB;

  private float m_jumpTimer = 0;
  private float m_gravityAcc;
  private float m_initJumpVel;

  private Vector3 m_newVelw;

  void Start()
  {
    m_rRB = GetComponent<Rigidbody>();

    Cursor.lockState = CursorLockMode.Locked;
  }

  private void Update()
  {
    if (Input.GetButtonDown("Jump"))
    {
      OnJump();
    }
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  void FixedUpdate()
  {
    m_newVelw = m_rRB.velocity;

    // X Z Plane Movement

    Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

    // apply slowdown friction when not inputting direction AND ON THE GROUND FUTURE
    //if (moveInput.magnitude < .01)
    {
      m_newVelw *= 1 - m_dFrictionPercentPerSec;
    }

    moveInput.z *= m_dTopFwdSpeedMPS;
    moveInput.x *= m_dTopStrafeSpeedMPS;
    moveInput = transform.TransformVector(moveInput); //to worldspace

    m_newVelw.x += moveInput.x;
    m_newVelw.z += moveInput.z;

    // cap speed in each direction independently
    Vector3 newVelPlayerSpace = transform.InverseTransformVector(m_newVelw);
    newVelPlayerSpace.x = Mathf.Sign(newVelPlayerSpace.x) * Mathf.Min(Mathf.Abs(newVelPlayerSpace.x), m_dTopStrafeSpeedMPS);
    newVelPlayerSpace.z = Mathf.Sign(newVelPlayerSpace.z) * Mathf.Min(Mathf.Abs(newVelPlayerSpace.z), m_dTopFwdSpeedMPS);
    m_newVelw = transform.TransformVector(newVelPlayerSpace);

    // Y Gravity
    m_gravityAcc = -2 * m_dJumpHeightMeters / (m_dTimeToJumpPeakSec * m_dTimeToJumpPeakSec);
    m_initJumpVel = 2 * m_dJumpHeightMeters / m_dTimeToJumpPeakSec;
    m_newVelw.y = m_rRB.velocity.y + m_gravityAcc * Time.deltaTime;

    // Set Velocity
    m_rRB.velocity = m_newVelw;

    // Y Look Rotation
    {
      float yawDelta = Input.GetAxis("Mouse X") * m_dLookYawSensitivity * Mathf.Deg2Rad;
      transform.RotateAround(transform.position, transform.up, yawDelta);
    }

    // Camera Look X Rot
    //if (Mathf.Abs(m_rCameraTr.localRotation.eulerAngles.x) < 80.0f)
    {
      float pitchDelta = -1 * Input.GetAxis("Mouse Y") * m_dLookPitchSensitivity * Mathf.Deg2Rad;
      m_rCameraTr.RotateAround(m_rCameraTr.position, m_rCameraTr.right, pitchDelta);
    }
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  public void OnJump()
  {
    Vector3 newVel = m_rRB.velocity;
    newVel.y = m_initJumpVel;
    m_rRB.velocity = newVel;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  private void OnTriggerEnter(Collider other)
  {
    Explosion rExp = other.GetComponent<Explosion>();
    if (rExp /* && !rExp.HasCollidedWithPlayer()*/)
    {

      rExp.SetCollidedWithPlayer();

      Vector3 splosionVec = transform.position - rExp.transform.position;
      float dist2Epicenter = splosionVec.magnitude;
      splosionVec.Normalize();
      splosionVec *= m_dMaxSplosionKnockbackSpeed * (m_dSplosionFalloffDist - dist2Epicenter);

      //splosionVec.y *= 2;
      //splosionVec.y += 4;
      m_newVelw += splosionVec;
      m_rRB.velocity = m_newVelw;

    }
  }
}
