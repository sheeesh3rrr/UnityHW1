using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    //[SerializeField] private Transform player;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private float[] lanes;

    private float timer;
    private Transform player;

    private void Start()
    {
        player = GetComponentInParent<Transform>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    private void Spawn()
    {
        int lane = Random.Range(0, lanes.Length);
        int index = Random.Range(0, prefabs.Length);

        Vector3 pos = new Vector3(
            lanes[lane],
            1.5f,
            player.position.z + spawnDistance
        );

        GameObject obj = Instantiate(prefabs[index], pos, Quaternion.identity);

        obj.GetComponent<Obstacle>().Initialize(player);
    }
}