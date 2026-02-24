# НИС UNity HW1 Выполнил Кудрявцев Георгий Александрович БПИ 245
## Краткое описание игры

Игра представляет собой бесконечный `3D` раннер. 
Игрок (капсула) бесконечно бежит вперед по платформе, огороженной с
двух сторон стенами, чтобы игрок не падал. Если игрок подбегает близко к концу платформы,
она перемещается вперед. Уже созданные препятствия при этом остаются на месте.

Препятствия имеют два типа: `стена` и `куб`. Стена сносит игроку `25` единиц здоровья,
куб - `10` единиц. Всего у игрока `100 hp`. При иссечении запаса здоровья, 
в консоль выводится сообщение об окончании игры и через 3 секунды она
перезапускается. 

С течением времени скорость, с которой бежит игрок увеличивается.

## Точки входа и скрипты

Игра имеет только 1 сцену, которая и является главной. 
Эта сцена включает платформу, игрока с камерой, спавнер препятствий, который 
движется с игроком. 

**Скрипты**

- Скрипт игрока. Обрабатывает движение, проверяет возможность прыжка и сам 
процесс прыжка. Содержит HP и метод Die, вызывающийся при достижении отметки в 0 HP. 
```cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 8f;
    [SerializeField] private float sideSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private Rigidbody rb;
    private bool isGrounded;
    private bool isDead;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        CheckGround();

        if (!isDead)
        {
            HandleJump();
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            rb.linearVelocity = new Vector3(
                Input.GetAxis("Horizontal") * sideSpeed,
                rb.linearVelocity.y,
                forwardSpeed
            );
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
            }
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log($"Здоровье: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Игра окончена. Рестарт через 3 секунды.");
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
```

- Спавнер препятствий. Движется вместе с игроком (явлется его ребенком) и спавнит препятствия на одну из 3 линий.
Также содержит Transform игрока и передает его прпятствиям при инициализации.
Это необходимо чтобы препятствия могли самостоятельно отслеживать позицию игрока и удаляться со сцены.
```cs
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
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
```

- Данные препятствий. Отдельный скрипт для статических данных препятствий (Scriptable object)
```cs
using UnityEngine;

[CreateAssetMenu(menuName = "Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    public int damage;
    public float moveSpeed;
}
```

- Препятствия. Обрабатывает позицию игрока и удаляется со сцены после того как игрок пробежал мимо.
Также наносит игроку урок при столкновении. В этом сценарии также удаляется со сцены, чтобы игрок не застревал.
```cs
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleData data;
    private Transform player;

    public int Damage => data.damage;

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void Update()
    {
        if (player != null && transform.position.z < player.position.z - 15f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>()
                .TakeDamage(Damage);
        }

        Destroy(gameObject);
    }
}
```

- Скрипт, передвигающий платформу, по котрой бежит игрок.
```cs
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
```

## Выполнение требований

**Требования для игрока**
- Постоянно двигаться вперёд (автоматически) - Выполнено
- Перемещаться влево и вправо (свободно, или по линиям) - Выполнено
- Перепрыгивать препятствия. Но нельзя прыгать в воздухе более одного раза (т.е. можно
реализовать двойной прыжок) - Выполнено (без двойного прыжка, игрок может прыгать только когда стоит на  земле)
- Иметь максимальное здоровье и текущий запас здоровья - Выполнено
- Получать урон от столкновений с препятствиями и умирать при достижении 0 здоровья - Выполнено

**Требования для препятствий**
- Минимум два типа препятствий. Они должны различаться по форме и по урону, который
наносят игроку при столкновении  - Выполнено
- Периодическую их генерацию на пути перед игроком  - Выполнено
- Тип выбирается случайно (или по описанным правилам) - Выполнено
- Препятствия располагаются случайно: например, в одну из заранее определённых
линий (лево/центр/право) - Выполнено
- Препятствия, которые игрок уже обошёл удаляются со временем - Выполнено

**Доп требования**
- Реализовать ускорение игры с течением времени - Выполнено
- Использовать новую систему ввода - Не выполнено

Требования к доке выполнены.

## Использованные ресурсы
Вспомогательных ресурсов использовано не было.

Работа выполнена и залита на гит 24.02 в 11.45
