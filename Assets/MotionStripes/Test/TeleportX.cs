using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class                    TeleportX : MonoBehaviour
{
    [SerializeField]
    float                       maxWaitBeforeTP = 5;

    bool                        didTp = false;
    float                       nextTp;
    Vector3                     displacment = Vector3.zero;

    void                        Start()
    {
        nextTp = Time.time + Random.Range(0.0f, maxWaitBeforeTP);
    }

    void                        Update()
    {
        if (nextTp > Time.time) return;
        nextTp = Time.time + Random.Range(0.0f, maxWaitBeforeTP);
        if (!didTp)
        {
            displacment.x = Random.Range(5, 15);
            this.transform.Translate(displacment);
            didTp = true;
        }
        else
        {
            this.transform.Translate(-displacment);
            didTp = false;
        }
    }
}
