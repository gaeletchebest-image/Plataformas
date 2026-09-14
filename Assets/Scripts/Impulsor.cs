using System.Linq;
using UnityEngine;

public class Impulsor : MonoBehaviour
{

    [SerializeField] MeshRenderer rend;
    [SerializeField] float speed = 1f;

    [SerializeField] Vector2 offset;

    void Update()
    {
        offset += Vector2.down * speed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }

}
