using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public float destroyTime;

    Collider[] toIgnore;

    public GameObject particles;

    [HideInInspector]
    public bool instantiateParticles = false;

    void Start()
    {
        StartCoroutine(AutoDestroy(6));
        toIgnore = GetComponentsInChildren<Collider>();
    }

    void OnCollisionEnter(Collision other)
    {
        for (int i = 0; i < toIgnore.Length; i++)
        {
            if(other.collider != toIgnore[i])
            {
                if (instantiateParticles && particles != null)
                {
                    Instantiate(particles, other.contacts[0].point, new Quaternion(0, 0, 0, 0));
                    AutoDestroy(0);
                }

                else
                {
                    AutoDestroy(1);
                }

                return;
            }
        }
    }

    IEnumerator ExpiryDate(float deathTime )
    {
        yield return new WaitForSeconds(deathTime);
        Destroy(this.gameObject);
    }

    IEnumerator AutoDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}
