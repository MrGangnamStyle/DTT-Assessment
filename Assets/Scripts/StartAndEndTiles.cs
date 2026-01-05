using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class StartAndEndTiles : MonoBehaviour
{
    private void Start()
    {
        if (this.GetComponent<Vector3>() == null)
        Debug.Log("Boogerx Big MAc");
    }
}

public class StartTile : StartAndEndTiles
{
    public void Start()
    {
        Debug.Log("Start tile instantiated");
        this.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
}

public class EndTile : StartAndEndTiles
{
    public void Start()
    {
        Debug.Log("End tile instantiated");
        this.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        BoxCollider trigger = this.gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size *= 2.25f;

        Transform t = this.GetComponent<Transform>();
        StaticVariables.victoryParticles.transform.position = new Vector3(t.position.x, t.position.y, t.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ControllablePlayer>() != null) StartCoroutine(VictorySequence());
    }

    public IEnumerator VictorySequence()
    {
        StaticVariables.victoryParticles.GetComponent<ParticleSystem>().Play();
        ControllablePlayer _cp = StaticVariables.player.GetComponent<ControllablePlayer>();
        _cp.enabled = false;
        yield return new WaitForSeconds(1.5f);

        StaticVariables.uiManager.SetVictoryCanvas(true);
        StaticVariables.camManager.SetToStartScreen();
        _cp.gameObject.SetActive(false);

        yield return null;
    }
}
