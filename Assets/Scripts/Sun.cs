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

    // Store the current hour of the day
    private int currentHour;

    private void Start()
    {
        MoveSunToStart();
    }

    private void MoveSunToStart()
    {
        currentHour = 0;
        transform.position = startSunPosition.position;
        transform.rotation = startSunPosition.rotation;
    }

    public void NextHour()
    {
        //Debug.Log($"Starting hour {currentHour}");

        // Calculate the fraction of the day based on the current hour
        float fractionOfDay = (float)currentHour / (float)hoursInDay;

        // Calculate the new position of the sun based on the fraction of the day
        Vector3 newPosition = Vector3.Lerp(startSunPosition.position, endSunPosition.position, fractionOfDay);

        // Calculate the new rotation of the sun based on the fraction of the day
        Quaternion targetRotation = Quaternion.Euler((fractionOfDay * 180.0f) - 90.0f, -90.0f, 0.0f);
        Quaternion newRotation = Quaternion.LookRotation(newPosition - transform.position, Vector3.up);
        Quaternion finalRotation = Quaternion.Slerp(transform.rotation, targetRotation, fractionOfDay);

        // Move the sun to its new position and rotate it towards the target rotation
        transform.position = newPosition;
        transform.rotation = Quaternion.Lerp(newRotation, finalRotation, fractionOfDay);

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

            Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.yellow, 1f);

            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxRaycastDistance))
            {
                if (hitInfo.collider.CompareTag("Leaf"))
                {
                    Plant plant = hitInfo.collider.transform.parent.GetComponent<Plant>();
                    if (plant != null)
                    {
                        Debug.DrawLine(randomPoint, transform.position, Color.green, 10f);
                        plant.AddSunlight(sunlightPerRaycast);
                        Debug.Log($"Added {sunlightPerRaycast} sunlight to plant {plant.name}");
                    }
                }
            }
            else
            {
                RaycastHit groundHitInfo;
                if (Physics.Raycast(ray, out groundHitInfo, maxRaycastDistance, LayerMask.GetMask("Ground")))
                {
                    Debug.Log("Hit ground");
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
}