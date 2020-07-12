using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

[SelectionBase]
public class CyclopsPlayer : MonoBehaviour
{
  public bool m_dSHOOTEYEBLASTS = true;

  public float m_dJumpHeightMeters;
  public float m_dTimeToJumpPeakSec;

  public float m_dTopFwdSpeedMPS;
  public float m_dTopStrafeSpeedMPS;
  public float m_dGroundFrictionPerFrame;
  public float m_dYVelocityClamp;

  public float m_dLookPitchSensitivity;
  public float m_dLookYawSensitivity;

  public float m_dMinSplosionKnockbackMPS;
  public float m_dMaxHorizontalSplosionKnockbackMPS;
  public float m_dMaxVerticalSplosionKnockbackMPS;

  public float m_dEyeOpenMinSec;
  public float m_dEyeCloseMinSec;

  // Splode
  public GameObject m_dExposionPrefab;
  public GameObject m_dLaserPrefab;
  public float m_dExplosionBaseFreqSec;
  public float m_dExplosionMaxFreqSec;
  public int m_dExpRampUpShots;
  public float m_dExplosionStartUpTime;

  public float m_dMaxHealth;
  public float m_dHealthRegenPerSec;

  // Reference Gameobjects in prefab
  public Transform m_rCameraTr;
  public Transform m_drBlinkBlock;
  public Transform m_drEar0;
  public Transform m_drEar1;
  public PostProcessVolume m_drVolume;


  // Privates
  private Rigidbody m_rRB;

  private float m_jumpTimer = 0;
  private float m_gravityAcc;
  private float m_initJumpVel;

  private Vector3 m_newVelw;

  // Eyes
  private bool m_bChangeEyeState_CorRunning = false;
  private bool m_bEyesClosed = false;

  private Vignette m_vignette;

  private bool m_bFireLaser_CoreRunning = false;
  private float m_currentExplosionFreq;

  //health
  private float m_currentHealth;

  // level
  private bool m_levelFinishedScreen = false;
  private bool m_levelFailedScreen = false;
  private int m_currentLevelNum = 1;


  ////////////////////////////////////////////////////////////////////////////////////////////////////
  void Start()
  {
    m_rRB = GetComponent<Rigidbody>();

    Cursor.lockState = CursorLockMode.Locked;

    m_drBlinkBlock.gameObject.SetActive(m_bEyesClosed);
    m_bEyesClosed = false;
    StartCoroutine(FireLaser_Cor());

    m_currentHealth = m_dMaxHealth;

    m_levelFinishedScreen = false;

    // init eyes to off
    m_dSHOOTEYEBLASTS = false;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  private void Update()
  {
    bool onGround = IsOnGround();
    if (Input.GetButtonDown("Jump") && onGround)
    {
      OnJump();
    }

    // Change Eye State
    if (!m_bChangeEyeState_CorRunning)
    {
      if (!m_bEyesClosed && Input.GetButton("Blink")) // close eyes
      {
        StartCoroutine(ChangeEyeState_Cor());

        if (m_levelFinishedScreen)
        {
          string[] strs = SceneManager.GetActiveScene().name.Split('0');
          string nextLevelName = "Level0" + (int.Parse(strs[1]) + 1);
          SceneManager.LoadScene(nextLevelName);
          m_levelFinishedScreen = false;
          //turn off canvas
        }
        else if (m_levelFailedScreen)
        {
          SceneManager.LoadScene(SceneManager.GetActiveScene().name);
          m_levelFailedScreen = false;
          //turn off canvas
        }
      }
      else if (m_bEyesClosed && !Input.GetButton("Blink")) // open eyes
      {
        StartCoroutine(ChangeEyeState_Cor());
        StartCoroutine(FireLaser_Cor());
      }
    }

    //health
    if (m_currentHealth < m_dMaxHealth)
    {
      m_currentHealth += m_dHealthRegenPerSec * Time.deltaTime;
    }
    else
    {
      m_currentHealth = m_dMaxHealth;
    }
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  void FixedUpdate()
  {
    bool onGround = IsOnGround();

    m_newVelw = m_rRB.velocity;

    // X Z Plane Movement

    Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));

    if (onGround)
    {
      m_newVelw *= 1 - m_dGroundFrictionPerFrame;

      moveInput.z *= m_dTopFwdSpeedMPS;
      moveInput.x *= m_dTopStrafeSpeedMPS;
    }
    else
    {

    }

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

    m_newVelw.y = Mathf.Min(m_newVelw.y, m_dYVelocityClamp); // clamp upper y velocities

    // Slide Along Walls
    Vector3 flatVel = m_newVelw;
    flatVel.y = 0.0f;
    Ray ray = new Ray(transform.position + new Vector3(0.0f, 0.2f, 0.0f), flatVel.normalized); //raycast a bit above the ground
    float rayDist = 0.5f + flatVel.magnitude * Time.fixedDeltaTime;
    int layerMaskNoExplosion = ~((1 << 8) | (1 << 2));
    Debug.DrawLine(ray.origin, ray.origin + (ray.direction * rayDist), Color.blue);

    if (Physics.Raycast(ray, out RaycastHit hitInfo, rayDist, layerMaskNoExplosion))
    {
      if (!hitInfo.collider.isTrigger)
      {
        Vector3 wallRelativeUp = Vector3.Cross(flatVel.normalized, hitInfo.normal);
        Vector3 displacementDir = Vector3.Cross(wallRelativeUp, hitInfo.normal);

        displacementDir *= Vector3.Dot(flatVel, displacementDir); //scale by projecting intended vel onto dir perpendicular to wall normal
        displacementDir.y = m_newVelw.y;

        m_newVelw = displacementDir;

        Debug.DrawLine(transform.position, transform.position + m_newVelw, Color.red);
        //Debug.Break();
      }
    }

    // Set Velocity
    m_rRB.velocity = m_newVelw;
    m_rRB.angularVelocity = Vector3.zero;

    // Y Look Rotation
    {
      float yawDelta = Input.GetAxis("Mouse X") * m_dLookYawSensitivity * Mathf.Deg2Rad;
      transform.RotateAround(transform.position, transform.up, yawDelta);
    }

    // Camera Look X Rot
    //Matrix4x4 rot = new Matrix4x4();
    //rot.SetColumn(1, Vector4(Mathf.Cos()))
    // Vector3 maxFwdUp = Vector3. (m_rCameraTr.forward


    float signedAngle = Vector3.SignedAngle(transform.forward, m_rCameraTr.forward, transform.right);
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
    if (rExp && !rExp.HasCollidedWithPlayer())
    {
      rExp.SetCollidedWithPlayer();

      Vector3 playerhead = transform.position + new Vector3(.0f, 2.0f, .0f);
      Vector3 splosionVec = playerhead - rExp.transform.position;

      float dist2Epicenter = splosionVec.magnitude;
      float fallOff = 1 - (dist2Epicenter / rExp.m_dMaxDiameter);
      fallOff = 1 - (fallOff * fallOff * fallOff);

      //negate negative splosion force, apply to horizontal
      if (splosionVec.y < 0.0f)
      {
        float mag = splosionVec.magnitude;
        splosionVec.y = 0.0f;
        splosionVec = splosionVec.normalized * mag;
      }
      splosionVec.Normalize();

      //vert and horizontal components of knockback
      Vector3 hSplosionVec = new Vector3(splosionVec.x, 0.0f, splosionVec.z);
      hSplosionVec *= Mathf.Lerp(m_dMinSplosionKnockbackMPS, m_dMaxHorizontalSplosionKnockbackMPS, fallOff);
      Vector3 vSplotionVec = new Vector3(0.0f, splosionVec.y, 0.0f);
      vSplotionVec *= Mathf.Lerp(m_dMinSplosionKnockbackMPS, m_dMaxVerticalSplosionKnockbackMPS, fallOff);

      splosionVec += hSplosionVec + vSplotionVec;

      // new vel
      m_newVelw = Vector3.zero;
      m_newVelw = splosionVec;
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

    m_drVolume.profile.TryGetSettings(out m_vignette);

    m_vignette.active = true;
    m_vignette.intensity.value = 1.0f;

    m_bChangeEyeState_CorRunning = false;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  private IEnumerator FireLaser_Cor()
  {
    if (m_dSHOOTEYEBLASTS)
    {
      m_bFireLaser_CoreRunning = true;

      yield return new WaitForSeconds(m_dExplosionStartUpTime);

      int shotsSinceStarted = 0;
      while (!m_bEyesClosed && m_dSHOOTEYEBLASTS)
      {
        Instantiate(m_dLaserPrefab, m_drEar0.position, m_drEar0.transform.rotation, m_drEar0);
        Instantiate(m_dLaserPrefab, m_drEar1.position, m_drEar1.transform.rotation, m_drEar1);

        m_currentExplosionFreq = Mathf.Lerp(m_dExplosionBaseFreqSec, m_dExplosionMaxFreqSec, (float)(shotsSinceStarted / m_dExpRampUpShots));

        Ray ray = new Ray(m_rCameraTr.position, m_rCameraTr.forward);
        int layerMaskNoExplosion = ~(1 << 8);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100.0f, layerMaskNoExplosion))
        {
          Instantiate(m_dExposionPrefab, hitInfo.point, Quaternion.identity);
        }

        yield return new WaitForSeconds(m_currentExplosionFreq);
      }
      m_currentExplosionFreq = m_dExplosionBaseFreqSec;

      m_bFireLaser_CoreRunning = false;
    }

    yield return null;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  public bool IsOnGround()
  {
    Vector3 slightFootOffset = transform.position + new Vector3(0.0f, 0.1f, 0.0f);
    Ray ray = new Ray(slightFootOffset, -Vector3.up);
    int layerMaskNoExplosion = ~((1 << 8) | (1 << 2));
    if (Physics.Raycast(ray, out RaycastHit hitInfo, 0.2f, layerMaskNoExplosion))
    {
      return true;
    }
    else return false;
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  public void SetFinishedLevel()
  {
    m_levelFinishedScreen = true;

    m_currentLevelNum++;
    //set canvas words here
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  public void StartEyeBlast()
  {
    m_dSHOOTEYEBLASTS = true;
    StartCoroutine(FireLaser_Cor());
  }

  ////////////////////////////////////////////////////////////////////////////////////////////////////
  public void ResetLevelOnNextBlink()
  {
    m_levelFailedScreen = true;
  }
}
