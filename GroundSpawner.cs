using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> activeObjects = new List<GameObject>();
    [SerializeField]
    private List<GameObject> queueToDeactive = new List<GameObject>();
    public int activeGroundAmount;
    public int queueBeforeDeactivateAmount;
    private ObjectPooler pooler;
    private int combinedPoolCount;

    //to avoid colliding grounds
    private int groundCountSinceLastDirectionGround = 0;

    public Vector3 groundPosition;
    public Vector3 groundDirection;

    //direction grounds changing rotation and position of the way for next grounds
    public enum WayRotation
    {
        Left, Right, Back, Straight
    }
    public WayRotation wayRotation = WayRotation.Straight;

    public Vector3 leftPositonIncrease;
    public Vector3 rightPositonIncrease;
    public Vector3 backPositonIncrease;
    public Vector3 straightPositonIncrease;

    public Vector3 leftDirection;
    public Vector3 rightDirection;
    public Vector3 backDirection;
    public Vector3 straightDirection;

    private void Start()
    {
        pooler = GetComponent<ObjectPooler>();
        combinedPoolCount = pooler.pooledGrounds.Count + pooler.pooledDirectionGrounds.Count;
    }

    private void Update()
    {
        //placing grounds
        if(activeObjects.Count < activeGroundAmount)
        {
            int randomInt = Random.Range(0, combinedPoolCount);
            if(pooler.pooledGrounds.Count >= randomInt + 1)
            {
                int random = Random.Range(0, pooler.pooledGrounds.Count);
                GameObject go = pooler.GetPooledGround(random);
                PlaceGround(go);
                UpdatePositionAndRotationForNextGround();


                activeObjects.Add(go);
                groundCountSinceLastDirectionGround++;
            }
            else if(groundCountSinceLastDirectionGround < 4)
            {
                int random = Random.Range(0, pooler.pooledGrounds.Count);
                GameObject go = pooler.GetPooledGround(random);
                PlaceGround(go);
                UpdatePositionAndRotationForNextGround();
                activeObjects.Add(go);
                groundCountSinceLastDirectionGround++;
                

            }
            else
            {
                int random = Random.Range(0, pooler.pooledDirectionGrounds.Count);
                GameObject go = pooler.GetPooledDirectionGround(random);
                PlaceGround(go);
                //directionGrounds 2x bigger than others but they pivot points act like normal grounds 
                //so i need uptade position third time
                // one for increase position on x, one for on z, one for next ground
                UpdatePositionAndRotationForNextGround();
                UpdateDirection(go);
                UpdatePositionAndRotationForNextGround();
                UpdatePositionAndRotationForNextGround();
                activeObjects.Add(go);
                groundCountSinceLastDirectionGround = 0;
            }
        }
        //removing grounds
        if(queueToDeactive.Count > queueBeforeDeactivateAmount)
        {
            for(int i = 0; i < activeObjects.Count; i++)
            {
                if(queueToDeactive[0] == activeObjects[i])
                {
                    pooler.GroundToPool(activeObjects[i]);
                    activeObjects.Remove(activeObjects[i]);
                    queueToDeactive.RemoveAt(0);
                    break;
                }
            }
            
        }
    }

    public void RemoveGroundFromList(GameObject ground)
    {
        activeObjects.Remove(ground);
        pooler.GroundToPool(ground);
    }
    public void PlaceGround(GameObject ground)
    {
        ground.transform.position = groundPosition;
        ground.transform.rotation = Quaternion.Euler(groundDirection);       
    }
    public void UpdatePositionAndRotationForNextGround()
    {
        switch (wayRotation)
        {
            case WayRotation.Straight:
                groundPosition += straightPositonIncrease;
                groundDirection = straightDirection;
                break;
            case WayRotation.Left:
                groundPosition += leftPositonIncrease;
                groundDirection = leftDirection;
                break;
            case WayRotation.Right:
                groundPosition += rightPositonIncrease;
                groundDirection = rightDirection;
                break;
            case WayRotation.Back:
                groundPosition += backPositonIncrease;
                groundDirection = backDirection;
                break;
        }
    }
    public void UpdateDirection(GameObject directionGround)
    {
        if(directionGround.transform.GetChild(0).name == "Left")
        {
            switch (wayRotation)
            {
                case WayRotation.Straight:
                    wayRotation = WayRotation.Left;
                    break;
                case WayRotation.Left:
                    wayRotation = WayRotation.Back;
                    break;
                case WayRotation.Right:
                    wayRotation = WayRotation.Straight;
                    break;
                case WayRotation.Back:
                    wayRotation = WayRotation.Right;
                    break;
            }
        }
        else if (directionGround.transform.GetChild(0).name == "Right")
        {
            switch (wayRotation)
            {
                case WayRotation.Straight:
                    wayRotation = WayRotation.Right;
                    break;
                case WayRotation.Left:
                    wayRotation = WayRotation.Straight;
                    break;
                case WayRotation.Right:
                    wayRotation = WayRotation.Back;
                    break;
                case WayRotation.Back:
                    wayRotation = WayRotation.Left;
                    break;
            }
        }
        else
        {
            throw new System.Exception("Naming Error In DIRECTION GROUND CHILD!!!");
        }
    }
    public void AddToDeactivateQueue(GameObject go)
    {
        queueToDeactive.Add(go);
    }
}
