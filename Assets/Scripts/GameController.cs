using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private const int MAIN_PILLAR_PRE_INSTANTIATE_NUMBER = 10;
    private const int FLOOR_PRE_INSTANTIATE_NUMBER = 3;
    public static GameController Instance { get; private set; }
    public bool IsFalling { get => isFalling; set => isFalling = value; }

    public GameObject mainPillar;
    public float distanceBetweenFloors = 5f;
    public List<GameObject> floors;
    public Vector3 firstFloorPosition;

    // Camera control variables
    public Transform player;
    [Tooltip("Camera distance to the player.")]
    public float distance;

    public static Camera mainCamera;

    private bool isFalling = true;

    // Auto-generate map variables
    private LinkedList<Pillar> mainPillars = new LinkedList<Pillar>();
    private Queue<FloorController> floorQueue = new Queue<FloorController>();
    private float lastFloorHeight;
    private Pillar lastQueuedPillar;

    // Mouse drag control variables
    private float offset;
    private Vector3 mouseDownPos;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        //PreInstantiate();
    }

    private void Start()
    {
        AddFloors(5);
        AddMainPillars();
    }

    private void Update()
    {
        RotateAllFloors();
    }

    private void LateUpdate()
    {
        MoveCamera();
    }

    private int GetFloorIndex()
    {
        return Random.Range(0, floors.Count);
    }

    private void AddFloor()
    {
        var newFloor = SimplePool.Spawn(floors[GetFloorIndex()], firstFloorPosition, Quaternion.identity);
        var rotation = newFloor.transform.eulerAngles;
        rotation.y = Random.Range(0, 360f);
        newFloor.transform.eulerAngles = rotation;
        floorQueue.Enqueue(newFloor.GetComponent<FloorController>());
        firstFloorPosition -= Vector3.up * distanceBetweenFloors;
    }


    private void AddFloors(int total)
    {
        for (int i = 0; i < total; i++)
        {
            AddFloor();
        }
    }

    private void AddMainPillar()
    {
        Vector3 pos = Vector3.zero;
        pos.y = player.position.y;
        Pillar pillar;
        if (mainPillars.Count == 0)
        {
            pillar = new Pillar(Instantiate(mainPillar));
        }
        else
        {
            Pillar firstPillar = mainPillars.First.Value;
            if (firstPillar.IsOffScreen)
            {
                pillar = firstPillar;
                mainPillars.RemoveFirst();
            }
            else
            {
                pillar = new Pillar(Instantiate(mainPillar));
            }

            Pillar lastPillar = mainPillars.Last.Value;
            pos = lastPillar.Renderer.bounds.center;
            pos.y -= lastPillar.Renderer.bounds.size.y;
        }
        pillar.GameObject.transform.position = pos;
        pillar.GameObject.transform.SetParent(player);
        mainPillars.AddLast(pillar);

    }
    int temp;
    private void AddMainPillars()
    {
        while (ShouldAddMorePillars() && ++temp < 10) AddMainPillar();
    }
    private bool ShouldAddMorePillars()
    {
        if (mainPillars.Count < 3) return true;
        if (mainPillars.Last.Value.IsOffScreen) return true;
        return false;
    }

    private void RotateAllFloors()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            offset = Input.mousePosition.x - mouseDownPos.x;
            foreach (var floor in floorQueue)
            {
                var angle = floor.transform.eulerAngles;
                angle.y -= offset;
                floor.transform.eulerAngles = angle;
            }
            mouseDownPos = Input.mousePosition;
        }
    }

    public void MoveCamera()
    {
        if (floorQueue.Count == 0) return;
        if (player.transform.position.y > floorQueue.Peek().transform.position.y && !IsFalling) return;
        if (player.transform.position.y < floorQueue.Peek().transform.position.y)
        {
            MakeFall(floorQueue.Dequeue());
            AddFloor();
            AddMainPillars();
        }
        IsFalling = true;

        var pos = mainCamera.transform.position;
        pos.y = player.position.y + distance;
        mainCamera.transform.position = pos;

    }

    private void MakeFall(FloorController floor)
    {
        floor.StartFalling();
        StartCoroutine(DisableFloor(floor.gameObject, floor.fallDuration));
    }

    IEnumerator DisableFloor(GameObject floor, float time)
    {
        yield return new WaitForSeconds(time);
        SimplePool.Despawn(floor);
    }
}

public class Pillar
{
    GameObject gameObject;
    Renderer renderer;

    public Pillar(GameObject gameObject)
    {
        this.GameObject = gameObject;
        this.Renderer = gameObject.GetComponent<Renderer>();
    }

    public Pillar(GameObject gameObject, Renderer renderer)
    {
        this.GameObject = gameObject;
        this.Renderer = renderer;
    }

    public GameObject GameObject { get => gameObject; set => gameObject = value; }
    public Renderer Renderer { get => renderer; set => renderer = value; }

    public bool IsOffScreen => renderer.isVisible;

    public class Floor
    {
        Pillar mapObject;
        List<GameObject> childs;
    }
}