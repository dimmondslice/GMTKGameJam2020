using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class CyclopsPlayer : MonoBehaviour
{
  public bool m_dSHOOTEYEBLASTS = true;

  public float m_dJumpHeightMeters;
  public float m_dTimeToJumpPeakSec;

  public float m_dTopFwdSpeedMPS;
  public float m_dTopStrafeSpeedMPS;
  public float m_dFrictionPercentPerSec;

  public float m_dLookPitchSensitivity;
  public float m_dLookYawSensitivity;

  public float m_dSplosionFalloffDist;
  public float m_dMaxSplosionKnockbackSpeed;

  public float m_dEyeOpenMinSec;
  public float m_dEyeCloseMinSec;

  public GameObject m_dExposionPrefab;
  public GameObject m_dLaserPrefab;
  public float m_dExplosionFreqSec;
  public float m_dExplosionStartUpTime;

  // Reference Gameobjects in prefab
  public Transform m_rCameraTr;
  public Transform m_drBlinkBlock;
  public Transform m_drEar0;
  public Transform m_drEar1;


  // Privates
  private Rigidbody m_rRB;

  private float m_jumpTimer = 0;
  private float m_gravityAcc;
  private float m_initJumpVel;

  private Vector3 m_newVelw;

  // Eyes
  private bool m_bChangeEyeState_CorRunning = false;
  private bool m_bEyesClosed = false;

  // Laser
  private bool m_bFireLaser_CoreRunning = false;

  void Start()
  {
    m_rRB = GetComponent<Rigidbody>();

    Cursor.lockState = CursorLockMode.Locked;

    m_drBlinkBlock.gameObject.SetActive(m_bEyesClosed);
    m_bEyesClosed = false;
    StartCoroutine(FireLaser_Cor());
  }

  private void Update()
  {
    if (Input.GetButtonDown("Jump"))
    {
      OnJump();
    }

    if (!m_bChangeEyeState_CorRunning)
    {
      if (!m_bEyesClosed && Input.GetButton("Blink")) // close eyes
      {
        StartCoroutine(ChangeEyeState_Cor());
      }
      else if (m_bEyesClosed && !Input.GetButton("Blink")) // open eyes
      {
        StartCoroutine(ChangeEyeState_Cor());
        StartCoroutine(FireLaser_Cor());
      }
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
    //Matrix4x4 rot = new Matrix4x4();
    //rot.SetColumn(1, Vector4(Mathf.Cos()))
   // Vector3 maxFwdUp = Vector3. (m_rCameraTr.forward

    float angle = Mathf.Acos(Vector3.Dot(transform.forward, m_rCameraTr.forward)) * Mathf.Rad2Deg; //direction vectors are unit length so only need arccos for angle


    float signedAngle = Vector3.SignedAngle(transform.forward, m_rCameraTr.forward, transform.right);
    //print(signedAngle);
    float pitchDelta = -1 * Input.GetAxis("Mouse Y") * m_dLookPitchSensitivity * Mathf.Deg2Rad;

   // if (Mathf.Sign(signedAngle) < 80.0f || Mathf.Sign(pitchDelta) == Mathf.Sign(signedAngle))
    {
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

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  private IEnumerator ChangeEyeState_Cor()
  {
    m_bChangeEyeState_CorRunning = true;

    m_bEyesClosed = !m_bEyesClosed;

    m_drBlinkBlock.gameObject.SetActive(m_bEyesClosed);
    float wait = m_bEyesClosed ? m_dEyeCloseMinSec : m_dEyeOpenMinSec;
    yield return new WaitForSeconds(wait);

    m_bChangeEyeState_CorRunning = false;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  private IEnumerator FireLaser_Cor()
  {
    if (m_dSHOOTEYEBLASTS)
    {
      m_bFireLaser_CoreRunning = true;

      yield return new WaitForSeconds(m_dExplosionStartUpTime);

      while (!m_bEyesClosed)
      {
        Instantiate(m_dLaserPrefab, m_drEar0.position, m_drEar0.transform.rotation, m_drEar0);
        Instantiate(m_dLaserPrefab, m_drEar1.position, m_drEar1.transform.rotation, m_drEar1);

        Ray ray = new Ray(m_rCameraTr.position, m_rCameraTr.forward);
        int layerMaskNoExplosion = ~(1 << 8);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100.0f, layerMaskNoExplosion))
        {
          Instantiate(m_dExposionPrefab, hitInfo.point, Quaternion.identity);
        }

        yield return new WaitForSeconds(m_dExplosionFreqSec);
      }

      m_bFireLaser_CoreRunning = false;
    }

    yield return null;
  }
}
