using UnityEngine;
using System.Collections;

public class BlowShitUp : MonoBehaviour
{
  public float duration;
  float lifetime;
  public float size;

  Vector3 startScale;
  Vector3 endScale;

  void OnTriggerEnter(Collider collider)
  {
    Rigidbody colRigid = collider.GetComponent<Rigidbody>();
    if (colRigid != null)
    {
      colRigid.AddExplosionForce(1, transform.position, size);
    }
  }

  void Start()
  {
    lifetime = 0;
    startScale = Vector3.zero;
    endScale = Vector3.one * size;
  }

  // Update is called once per frame
  void Update()
  {
    lifetime += Time.deltaTime;

    float x = 1 - (lifetime / duration);
    float completion = 1 - (x * x * x * x * x * x);
    transform.localScale = Vector3.Lerp(startScale, endScale, completion);
  }
}
