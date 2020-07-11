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
        startScale = transform.localScale;
        endScale = startScale * size;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime > duration)
        {
            Destroy(gameObject);
        }
        lifetime += Time.deltaTime;

        transform.localScale = Vector3.Lerp(startScale, endScale, (lifetime / duration));
    }
}
