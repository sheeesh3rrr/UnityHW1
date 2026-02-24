using UnityEngine;

public class DestroyBehindPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float distance = 10f;

    private void Update()
    {
        if (transform.position.z < player.position.z - distance)
        {
            Destroy(gameObject);
        }
    }
}