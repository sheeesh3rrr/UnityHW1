using UnityEngine;

public class GroundMover : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float moveDistance = 20f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (player.position.z > transform.position.z)
        {
            transform.position += Vector3.forward * moveDistance;
        }
    }
}