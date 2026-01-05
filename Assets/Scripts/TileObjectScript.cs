using Unity.VisualScripting;
using UnityEngine;

public class TileObjectScript : MonoBehaviour
{
    public GameObject[] walls = new GameObject[4];
    [SerializeField] private GameObject wallsParent;

    public void SetWalls(int dir) => walls[dir].SetActive(false);
    public void EnableWalls() => wallsParent.SetActive(true);
    public void ResetWalls() { foreach (GameObject wall in walls) { wall.SetActive(true); } }
}
