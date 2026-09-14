using System;
using UnityEngine;

public class Collectible : MonoBehaviour
{

    [SerializeField] TriggerDetect td;

    GameMotor gm;

    private void Start()
    {
        gm = GameController.Instance.GetGameMotor();
        td.OnDetect += (object o, EventArgs e) =>
        {
            gm.AddCollectable();
            Destroy(gameObject);
        };
    }

}
