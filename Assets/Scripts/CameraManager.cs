using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera startCamera;
    public Camera topDownCamera;
    public Camera playerCamera;

    private Camera[] allCameras = new Camera[3];
    public enum CameraState
    {
        None,
        Start,
        MazeTopDown,
        Player
    }
    public CameraState state;

    [Space, Header("Other Scripts")]
    [SerializeField] ControllablePlayer player;
    [SerializeField] GenerateMaze mazeGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = CameraState.None;

        allCameras[0] = startCamera;
        allCameras[1] = topDownCamera;
        allCameras[2] = playerCamera;

        for (int i = 0; i < allCameras.Length; i++) allCameras[i].enabled = false;

        startCamera.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case CameraState.Player:
                playerCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
                if (Input.GetKeyDown(KeyCode.M)) SetToMazeTopDown();
                break;
            case CameraState.MazeTopDown:
                if (Input.GetKeyDown(KeyCode.M) && StaticVariables.player.activeSelf) SetToPlayerPosition();
                break;
        }
    }

    public void SetToStartScreen()
    {
        state = CameraState.Start;
        ActivateWantedCamera(startCamera);
    }

    public void SetToMazeTopDown()
    {
        state = CameraState.MazeTopDown;
        int x = mazeGenerator.width;
        int y = mazeGenerator.length;

        topDownCamera.transform.position = new Vector3(x, (x + y) * 2, y);
        ActivateWantedCamera(topDownCamera);

        StaticVariables.uiManager.SetPointMarkerCanvas(true);
    }

    public void SetToPlayerPosition()
    {
        // Have the scroll wheel change the height of the maze;
        state = CameraState.Player;
        Transform camTransform = playerCamera.GetComponent<Transform>();
        camTransform.parent = player.transform;
        camTransform.localPosition = new Vector3(0, 20, 0);
        ActivateWantedCamera(playerCamera);

        StaticVariables.uiManager.SetPointMarkerCanvas(false);
    }

    public void ActivateWantedCamera(Camera camWanted)
    {
        for (int i = 0; i < allCameras.Length; i++)
        {
            allCameras[i].enabled = (allCameras[i] == camWanted);
        }
    }
}
