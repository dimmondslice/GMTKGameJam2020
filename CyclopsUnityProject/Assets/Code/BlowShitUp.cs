using UnityEngine;
using System.Collections;

public class BlowShitUp : MonoBehaviour
{
  public float m_dTimeToMaxDiameter;
  public float m_dMaxDiameter;

  private float m_lifetime;

  private Vector3 m_startScale;
  private Vector3 m_endScale;

  void OnTriggerEnter(Collider collider)
  {
    Rigidbody rColRigid = collider.GetComponent<Rigidbody>();
    CyclopsPlayer rPlayer = collider.GetComponent<CyclopsPlayer>();
    if (rColRigid && !rPlayer)
    {
      rColRigid.AddExplosionForce(500, transform.position, m_dMaxDiameter);
    }
  }

  void Start()
  {
    m_lifetime = 0;
    m_startScale = Vector3.zero;
    m_endScale = Vector3.one * m_dMaxDiameter;
  }

  // Update is called once per frame
  void Update()
  {
    m_lifetime += Time.deltaTime;

    float x = 1 - (m_lifetime / m_dTimeToMaxDiameter);
    float completion = 1 - (x * x * x * x * x * x);
    transform.localScale = Vector3.Lerp(m_startScale, m_endScale, completion);
  }
}
