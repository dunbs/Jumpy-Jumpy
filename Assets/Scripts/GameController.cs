using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private const int preMainPillarsCount = 10;
    public GameObject mainPillar;
    public float distanceBetweenFloors = 5f;
    private const int preFloorsCount = 3;
    public List<GameObject> floors;
    public Vector3 firstFloorPosition;

    // Camera control variables
    public Transform player;
    [Tooltip("Camera distance to the player.")]
    public float distance;

    [HideInInspector]
    public bool isFalling = true;

    private int score;

    private new Camera camera;

    // Auto-generate map variables
    private Queue<MapObject> mainPillarStack = new Queue<MapObject>();
    private Queue<GameObject> floorStack = new Queue<GameObject>();
    private float lastFloorHeight;

    // Mouse drag control variables
    private float offset;
    private Vector3 mouseDownPos;

    private void Awake()
    {
        Instance = this;
        camera = Camera.main;
        PreInstantiate();
    }

    private void Start()
    {
        AddFloors(100);
    }

    private void Update()
    {
        RotateAllFloors();
    }

    private void LateUpdate()
    {
        MoveCamera();
    }

    private void PreInstantiate()
    {
        // Main Pillar;
        var lastPillar = new MapObject(Instantiate(mainPillar));
        mainPillarStack.Enqueue(lastPillar);
        for (int i = 0; i < preMainPillarsCount; i++)
        {
            var y = lastPillar.Renderer.bounds.size.y;
            var newPos = lastPillar.GameObject.transform.position - Vector3.up * y;
            var newPillar = new MapObject(Instantiate(mainPillar, newPos, Quaternion.identity));
            lastPillar = newPillar;
            mainPillarStack.Enqueue(newPillar);
        }

        // Floors
        /*
        int originalCount = floors.Count;
        floors.Capacity = originalCount * 3;
        for (int i = 0; i < originalCount; i++)
        {
            for (int j = 0; j < preFloorsCount - 1; j++)
            {
                floors.Add(Instantiate(floors[i]));
            }
            floors[i] = Instantiate(floors[i]);
        }
        */
    }

    private int GetFloorIndex()
    {
        return Random.Range(0, floors.Count);
    }

    private void AddFloors(int total)
    {
        if (total <= 0) return;

        if (floorStack.Count == 0)
        {
            --total;
            var newFloor = Instantiate(floors[GetFloorIndex()], firstFloorPosition, Quaternion.identity);
            floorStack.Enqueue(newFloor);
        }

        for (int i = 0; i < total; i++)
        {
            firstFloorPosition -= Vector3.up * distanceBetweenFloors;
            var newFloor = Instantiate(floors[GetFloorIndex()], firstFloorPosition, Quaternion.identity);
            floorStack.Enqueue(newFloor);
        }
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
            foreach (var floor in floorStack)
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


        if ((floorStack.Count > 0 && player.transform.position.y > floorStack.Peek().transform.position.y) && !isFalling) return;
        while (floorStack.Count > 0 && player.transform.position.y < floorStack.Peek().transform.position.y)
        {
            MakeFall(floorStack.Dequeue());
        }
        //if (!isFalling)
        //{
        //    while (player.transform.position.y > floorStack.Peek().transform.position.y)
        //        floorStack.Dequeue();
        //}
        isFalling = true;

        var pos = camera.transform.position;
        pos.y = player.position.y + distance;
        camera.transform.position = pos;

    }


    private void MakeFall(GameObject gameObject)
    {
        Debug.Log(floorStack.Count);
        foreach (Transform childTranform in gameObject.transform)
        {
            var child = childTranform.gameObject;
            Rigidbody r = child.GetComponent<Rigidbody>();
            if (!r) continue;
            r.isKinematic = false;
            r.useGravity = true;
            r.AddForce(Vector3.one);
        }
    }
}

public class MapObject
{
    GameObject gameObject;
    Renderer renderer;

    public MapObject(GameObject gameObject)
    {
        this.GameObject = gameObject;
        this.Renderer = gameObject.GetComponent<Renderer>();
    }

    public MapObject(GameObject gameObject, Renderer renderer)
    {
        this.GameObject = gameObject;
        this.Renderer = renderer;
    }

    public GameObject GameObject { get => gameObject; set => gameObject = value; }
    public Renderer Renderer { get => renderer; set => renderer = value; }
}

public class Floor
{
    MapObject mapObject;
    List<GameObject> childs;
}
