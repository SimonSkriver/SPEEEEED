using UnityEngine;

public class CentOfMass : MonoBehaviour
{
    public Transform a;
    public Transform b;


    void Awake()
    {
        Vector3 localA = transform.InverseTransformPoint(a.position);
        Vector3 localB = transform.InverseTransformPoint(b.position);
        print((localA + localB) * 0.5f);
    }
}
