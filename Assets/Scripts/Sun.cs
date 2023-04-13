using System.Collections;
using System.Linq;
using UnityEngine;

public class Sun : MonoBehaviour
{
    // Fields to control the Sun's movement with arc
    public Transform startSunPosition;
    public Transform endSunPosition;
    public BoxCollider groundPlane;
    public float totalMovementTime;
    public int hoursInDay;
    public float timeBetweenHours = 3f;

    private float currentMovementTime;
    private int currentHour;
    private Quaternion startSunRotation;

    // Fields to control the raycasts
    public int numRaycasts;
    public float maxRaycastDistance;
    public float sunlightPerRaycast = 1f;

    public bool pauseBetweenHours;

    // Store the current ray being cast
    private Ray currentRay;

    // Cached references
    private Collider[] leafColliders;
    private Collider groundCollider;

    private IEnumerator currentCoroutine;

    private void Start()
    {
        leafColliders = GameObject.FindGameObjectsWithTag("Leaf")
            .Select(go => go.GetComponent<Collider>())
            .ToArray();
        groundCollider = GameObject.FindGameObjectWithTag("Ground").GetComponent<Collider>();

        currentMovementTime = 0f;
        currentHour = 0;
        pauseBetweenHours = false;

        startSunRotation = transform.rotation;

        // Start the first movement coroutine
        currentCoroutine = MoveSun();
        StartCoroutine(currentCoroutine);
    }

private IEnumerator MoveSun()
{
    while (true)
    {
        float hourLength = totalMovementTime / hoursInDay;
        float fractionOfDay = (float)currentHour / (float)hoursInDay;

        // Rotate the sun to face the target direction over the course of the hour
        yield return MoveSunTowardsTarget(fractionOfDay);

        // Wait for a short pause between hours
        if (pauseBetweenHours)
        {
            yield return new WaitForSeconds(timeBetweenHours);
        }

        // Perform raycasts onto the ground to simulate sunlight hitting plants
        yield return CastSunlightRays();

        // Increment the hour and reset the movement time
        currentHour++;
        currentMovementTime = 0f;

        // Check if we've finished the day
        if (currentHour >= hoursInDay)
        {
            yield break; // end this coroutine
        }
    }
}

private IEnumerator MoveSunTowardsTarget(float targetFractionOfDay)
{
    Vector3 startPosition = startSunPosition.position;
    Vector3 endPosition = endSunPosition.position;
    Quaternion startRotation = transform.rotation;

    // Calculate the height of the midpoint of the arc based on the distance between the start and end positions
    float arcHeight = Vector3.Distance(startPosition, endPosition) / 2f;

    // Calculate the total arc length and the current position on the arc based on the target hour
    float arcLength = Vector3.Distance(startPosition, endPosition);
    float currentArcPosition = targetFractionOfDay * arcLength;

    float elapsed = 0f;
    while (elapsed < totalMovementTime / hoursInDay)
    {
        elapsed += Time.deltaTime;

        // Calculate the fraction of the hour that has elapsed and use it to determine the current arc position
        float t = elapsed / (totalMovementTime / hoursInDay);
        currentArcPosition = t * arcLength;

        // Calculate the height of the sun above the midpoint of the arc based on its current position
        float height = Mathf.Sin((currentArcPosition / arcLength) * Mathf.PI) * arcHeight;

        // Calculate the new position of the sun based on the current arc position and height
        Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, currentArcPosition / arcLength);
        newPosition += new Vector3(0f, height, 0f);

        // Calculate the new rotation of the sun based on its current position and the target rotation
        Quaternion targetRotation = Quaternion.Euler((targetFractionOfDay * 180.0f) - 90.0f, -90.0f, 0.0f);
        Quaternion newRotation = Quaternion.LookRotation(newPosition - transform.position, Vector3.up);
        Quaternion finalRotation = Quaternion.Slerp(startRotation, targetRotation, t);

        // Move the sun to its new position and rotate it towards the target rotation
        transform.position = newPosition;
        transform.rotation = Quaternion.Lerp(newRotation, finalRotation, t);

        // Perform raycasts onto the ground to simulate sunlight hitting plants
        yield return CastSunlightRays();

        yield return null;
    }
}

private IEnumerator CastSunlightRays()
{
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
                Debug.DrawLine(randomPoint, transform.position, Color.green, 25f);
                Plant plant = hitInfo.collider.transform.parent.GetComponent<Plant>();
                if (plant != null)
                {
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

        yield return null;
    }
}

    private void NextHour()
    {
        Debug.Log($"Starting hour {currentHour}");

        // Perform raycasts randomly onto the ground
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
            Debug.DrawLine(randomPoint, transform.position, Color.red, 2f);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, maxRaycastDistance))
            {
                if (hitInfo.collider.CompareTag("Leaf"))
                {
                    Plant plant = hitInfo.collider.transform.parent.GetComponent<Plant>();
                    if (plant != null)
                    {
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
    }
    
    public void NextDay()
    {
        // Stop the current coroutine
        StopCoroutine(currentCoroutine);
    
        // Reset time and hour counter and start new movement coroutine
        currentMovementTime = 0f;
        currentHour = 0;
        currentCoroutine = MoveSun();
        StartCoroutine(currentCoroutine);
    }
    
}