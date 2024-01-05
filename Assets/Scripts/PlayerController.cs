using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum PlayerStates
{
    Idle,
    FindingPath,
    Turning,
    PreparingMovement,
    Moving,
}

public class PlayerController : MonoBehaviour
{


    public Pathfinder Pathfinder;

    public PlayerStates PlayerState;

    public Dictionary<PlayerStates, Action<float>> UpdateStates = new Dictionary<PlayerStates, Action<float>>();

    public Transform TestObject;

    public Vector3 Mousescreenpos;

    public Vector3 Mouseworldpos;

    public Transform Mouseindicator;

    public Vector3 IndicatorOffset;

    bool NewPathWhileMoving = false;


    [SerializeField] float Speed = 1.0f;
    [SerializeField] float RestTime = 1.0f;
    [SerializeField] float TurnTime = 1.0f;
    [SerializeField] float FindPathDelay = 1.0f;
    [SerializeField] float MovementSnapThreshold = 0.1f;
    [SerializeField] Vector3 Destination;
    [SerializeField] Vector2 GridDestination;
    [SerializeField] LineRenderer PathLine;
    [SerializeField] List<Node> CurrentPath = new List<Node>();
    [SerializeField] int _currentDirection = 0; // N 0, E 1, S 2, W 3
    [SerializeField] int _currentNode = -1;
    [SerializeField] int _nextNode = 0;

    private int _currentMovement = 0;
    private float _findPathDelayTimer = 0f;
    private float _pathRestTimer = 0f;
    private float _turnDelayTimer = 0f;
    private bool _movementTriggered = false;
    private Vector3 _currentDest = Vector3.zero;
    private Vector3 _currentStartingPoint = Vector3.zero;





    public void SetDestination(Vector3 pos)
    {
        if (PlayerState != PlayerStates.Idle)
        {
            NewPathWhileMoving = true;
            return;
        }

        Destination = pos;
        SetState(PlayerStates.FindingPath);


    }


    public bool FindPath()
    {
        Vector2 selfPos = Pathfinder.WorldToGridPos(transform.position);
        Vector2Int gridSelf = new Vector2Int((int)selfPos.x, (int)selfPos.y);

        Vector2 destPos = Pathfinder.WorldToGridPos(Destination);
        GridDestination = destPos;
        Vector2Int gridDest = new Vector2Int((int)destPos.x, (int)destPos.y);

        CurrentPath = Pathfinder.GetPath(gridSelf, gridDest);

        

        if (CurrentPath.Count > 0)
        {
            _currentDest = Pathfinder.GridToWorldPos(CurrentPath[0].Position);
            _currentDest.z = transform.position.z;
            PathLine.enabled = true;
            return true;
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateStates[PlayerStates.Idle] = Update_Idle;
        UpdateStates[PlayerStates.FindingPath] = Update_FindingPath;
        UpdateStates[PlayerStates.Turning] = Update_Turning;
        UpdateStates[PlayerStates.PreparingMovement] = Update_PreparingMovement;
        UpdateStates[PlayerStates.Moving] = Update_Moving;




    }


    void SetState(PlayerStates state)
    {
        PlayerState = state;
    }



    // Update is called once per frame
    void Update()
    {
        UpdateStates[PlayerState].Invoke(Time.deltaTime);

        Update_Mouse();
    }

    void Update_Mouse()
    {
        Mousescreenpos = Input.mousePosition;
        Mouseworldpos = Camera.main.ScreenToWorldPoint(Mousescreenpos);
        Mouseworldpos.z = 0;
        Vector3Int cellpos = Pathfinder.Groundmap.WorldToCell(Mouseworldpos);
        Mouseindicator.position = Pathfinder.Groundmap.CellToWorld(cellpos) + IndicatorOffset;

        if (Input.GetMouseButtonDown(0))
        {

            if (Pathfinder.Groundmap.HasTile(cellpos) && !Pathfinder.Treemap.HasTile(cellpos))
            {
                SetDestination(Mouseindicator.position);
            }
        }
    }


    //Player Changing States
    void Update_Idle(float dt)
    {
        _currentMovement = 0;
        if (_movementTriggered)
        {
            _movementTriggered = false;
        }
    }

    void Update_FindingPath(float dt)
    {

        _findPathDelayTimer += dt;
        if (_findPathDelayTimer >= FindPathDelay)
        {
            _findPathDelayTimer = 0f;
            _currentNode = -1;
            _nextNode = 0;
            if (FindPath())
            {
                _currentStartingPoint = transform.position;
                SetState(PlayerStates.Turning);
                return;
            }

            SetState(PlayerStates.Idle);
        }

    }

    void Update_Turning(float dt)
    {

        _currentMovement = 0;
        //transform.rotation = Quaternion.Euler(new Vector3(0, _currentDirection * 90, 0));

        _turnDelayTimer += dt;
        if (_turnDelayTimer > TurnTime)
        {
            SetState(PlayerStates.PreparingMovement);
            _turnDelayTimer = 0f;
        }

        if (_currentNode == -1)
        {
            CheckDirectionToNodeInPath(Pathfinder.WorldToGridPos(transform.position), CurrentPath[0].Position);
            return;
        }

        if (_currentNode >= 0)
            CheckDirectionToNodeInPath(CurrentPath[_currentNode].Position, CurrentPath[_nextNode].Position);

    }

    void Update_PreparingMovement(float dt)
    {

        _pathRestTimer += dt;
        _currentMovement = 0;

        if (CurrentPath.Count > 0)
        {
            Vector3[] pathPositions = new Vector3[CurrentPath.Count + 1];
            pathPositions[0] = _currentStartingPoint;

            for (int i = 1; i < CurrentPath.Count + 1; i++)
            {
                Vector3 worldPos = Pathfinder.GridToWorldPos(CurrentPath[i - 1].Position);
                pathPositions[i] = worldPos;
                //pathPositions[i].y = transform.position.y + 0.5f;
            }
            PathLine.positionCount = CurrentPath.Count + 1;
            PathLine.SetPositions(pathPositions);
        }

        if (_pathRestTimer > RestTime)
        {
            _pathRestTimer = 0f;
            SetState(PlayerStates.Moving);
            return;
        }

    }

    void Update_Moving(float dt)
    {

        if(NewPathWhileMoving)
        {
            _currentDest = Pathfinder.GridToWorldPos(CurrentPath[_nextNode].Position);
            _currentDest.z = transform.position.z;

            if(Vector2.Distance(transform.position, _currentDest) > MovementSnapThreshold)
            {
                transform.position = Vector2.MoveTowards(transform.position, _currentDest, Speed * Time.deltaTime);
                return;
            }

            transform.position = _currentDest;
            PathLine.enabled = false;
            _nextNode = 0;
            SetState(PlayerStates.Idle);
            NewPathWhileMoving = false;
            SetDestination(Mouseindicator.position);
            return;

        }

        if (Vector2.Distance(transform.position, _currentDest) > MovementSnapThreshold)
        {
            _currentMovement = 1;
            if (!_movementTriggered)
            {
                _movementTriggered = true;
            }

            transform.position = Vector2.MoveTowards(transform.position, _currentDest, Speed * Time.deltaTime);
            return;
        }

        if (_nextNode < CurrentPath.Count - 1)
        {
            _currentNode += 1;
            _nextNode += 1;
            transform.position = _currentDest;
            _currentDest = Pathfinder.GridToWorldPos(CurrentPath[_nextNode].Position);
            _currentDest.z = transform.position.z;
            SetState(PlayerStates.Turning);
            return;
        }

        if (_nextNode >= CurrentPath.Count - 1)
        {
            transform.position = _currentDest;
            PathLine.enabled = false;
            _nextNode = 0;
            SetState(PlayerStates.Idle);
        }
    }


    public void CheckDirectionToNodeInPath(Vector2 a, Vector2 b)
    {
        if (a.y < b.y)
        {
            _currentDirection = 0;
            return;
        }
        if (a.y > b.y)
        {
            _currentDirection = 2;
            return;
        }
        if (a.x < b.x)
        {
            _currentDirection = 1;
            return;
        }
        if (a.x > b.x)
        {
            _currentDirection = 3;
            return;
        }
    }


}
