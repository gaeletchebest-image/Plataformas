using UnityEngine;

public class Plataform : MonoBehaviour
{

    [SerializeField] Transform PointA, PointB;
    [SerializeField] float speed;
    [SerializeField] Rigidbody plataformRb;

    [SerializeField] bool goingToA = false;

    Vector3 lastPos;

    private void Start()
    {
        lastPos = plataformRb.position;
    }

    private void FixedUpdate()
    {
        Vector3 pointToGo = Vector3.zero;
        if (goingToA) pointToGo = PointA.position;
        else pointToGo = PointB.position;

        Vector3 dir = (pointToGo - plataformRb.position).normalized;
        lastPos = plataformRb.position;

        plataformRb.MovePosition(plataformRb.position + dir * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(pointToGo, plataformRb.position) < .2f)
            goingToA = !goingToA;
    }

    public Vector3 GetDelta() => plataformRb.position - lastPos ;

}
