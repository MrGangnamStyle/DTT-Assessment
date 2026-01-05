using UnityEngine;

public class StaticVariables : MonoBehaviour
{
    [SerializeField] GameObject _player;
    public static GameObject player;

    [SerializeField] GameObject _victoryParticles;
    public static GameObject victoryParticles;

    [SerializeField] UIManager _uimanager;
    public static UIManager uiManager;

    [SerializeField] CameraManager _cammanager;
    public static CameraManager camManager;
    private void Start()
    {
        player = _player;
        victoryParticles = _victoryParticles;
        uiManager = _uimanager;
        camManager = _cammanager;
    }
}
