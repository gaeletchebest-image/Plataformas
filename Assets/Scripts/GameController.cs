using UnityEngine;

public class GameController : MonoBehaviour
{
    
    public static GameController Instance;

    [SerializeField] ControlsController controls;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else DestroyImmediate(this);
    }



    public ControlsController GetControlsController() => controls;


}
