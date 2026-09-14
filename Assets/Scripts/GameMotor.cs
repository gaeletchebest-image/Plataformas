using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMotor : MonoBehaviour
{

    [SerializeField] List<CheckPoint> checkPoints;
    [SerializeField] int checkpointIndex = 0;

    [SerializeField] float minPosYToDie = -5f;

    PlayerScr player;

    private void Start()
    {
        player = GameController.Instance.GetPlayer();
    }

    private void Update()
    {
        if (player.transform.position.y < minPosYToDie)
            PlayerDied();
    }

    public bool actCheckpoint(CheckPoint check)
    {
        int index = checkPoints.FindIndex(x => x == check);
        if (index > checkpointIndex)
        {
            checkpointIndex = index;
            return true;
        }
        else return false;
    }

    void PlayerDied()
    {
        player.ResetPlayer(checkPoints[checkpointIndex].GetPosSpawn());
    }

}
