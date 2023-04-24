using UnityEngine;

public class Sun : MonoBehaviour
{
    // Fields to control the Sun's movement with arc
    public Transform startSunPosition;
    public Transform endSunPosition;
    public BoxCollider groundPlane;
    public float totalMovementTime;
    public int hoursInDay;

    // Fields to control the raycasts
    public int numRaycasts;
    public float maxRaycastDistance;
    public float sunlightPerRaycast = 1f;
    
    public bool drawRays = true;

    // Store the current hour of the day
    private int currentHour;
    private Vector3[] sunPositions;

    private void Start()
    {
        MoveSunToStart();
        sunPositions = GetArcPositions(startSunPosition.position, endSunPosition.position, hoursInDay);
    }

    private void MoveSunToStart()
    {
        currentHour = 0;
        transform.position = startSunPosition.position;
    }

    public void NextHour()
    {
        transform.position = sunPositions[currentHour];
        transform.rotation = Quaternion.LookRotation(groundPlane.center - transform.position, transform.up);

        // Perform raycasts onto the ground to simulate sunlight hitting plants
        for (int i = 0; i < numRaycasts; i++)
        {
            // Pick random point on ground which is a plane with a box collider attached 
            Vector3 randomPoint = new Vector3(
                Random.Range(groundPlane.bounds.min.x, groundPlane.bounds.max.x),
                groundPlane.transform.position.y,
                Random.Range(groundPlane.bounds.min.z, groundPlane.bounds.max.z)
            );
        
            // Cast ray from the sun to that point
            Vector3 direction = randomPoint - transform.position;
            Ray ray = new Ray(transform.position, direction.normalized);

            if(drawRays)Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.yellow, 1f);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxRaycastDistance))
            {
                if (hitInfo.collider.CompareTag("Leaf"))
                {
                    Plant plant = hitInfo.collider.transform.parent.GetComponent<Plant>();
                    if (plant != null)
                    {
                        if(drawRays)Debug.DrawLine(randomPoint, transform.position, Color.green, 10f);
                        plant.AddSunlight(sunlightPerRaycast);
                        //Debug.Log($"Added {sunlightPerRaycast} sunlight to plant {plant.name}");
                    }
                }
            }
        }

        // Increment the hour counter and check if we've finished the day
        currentHour++;
        if (currentHour >= hoursInDay)
        {
            // Move the sun back to its start position and reset the hour counter
            MoveSunToStart();
        }
    }
    
    // Returns an array of positions that make up a semi circle arc of numPositions between start and end
    public Vector3[] GetArcPositions(Vector3 start, Vector3 end, int numPositions)
    {
        Vector3[] positions = new Vector3[numPositions];
        // get circle center and radius
        var radius = Vector3.Distance(start, end) / 2f;
        var centerPos = (start + end) / 2f;

        // get a rotation that looks in the direction
        // start -> end
        var centerDirection = Quaternion.LookRotation((start - end).normalized);

        for (var i = 0; i < numPositions; i++)
        {
            var angle = Mathf.PI * (i) / (numPositions);
            var y = Mathf.Sin(angle) * radius;
            var z = Mathf.Cos(angle) * radius;
            var pos = new Vector3(0, y, z);
            // Rotate the pos vector according to the centerDirection
            pos = centerDirection * pos;

            positions[i] = centerPos + pos;
            
        }

        return positions;
    }
}