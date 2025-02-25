using UnityEngine;

public class CameraRaycast : MonoBehaviour
{
    private TutorialHandler currentTarget = null; // Tracks the object being looked at
    public float detectionRange = 5f;  // Max distance to detect objects
    public float detectionAngle = 30f; // Cone angle (Field of View)

    void Update()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, detectionRange);
        TutorialHandler newTarget = null;

        foreach (RaycastHit hit in hits)
        {
            TutorialHandler tutorial = hit.collider.gameObject.GetComponent<TutorialHandler>();

            if (tutorial != null)
            {
                // Calculate the angle between the camera's forward direction and the object
                Vector3 directionToTarget = (hit.collider.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToTarget);

                // Check if the object is within the cone
                if (angle <= detectionAngle)
                {
                    newTarget = tutorial;
                    break; // Stop after finding the first valid object within the cone
                }
            }
        }

        // If looking at a new object, update and trigger animations
        if (newTarget != currentTarget)
        {
            // Stop animation on the previous object (if any)
            if (currentTarget != null)
            {
                currentTarget.PlayPauseAnimation(false);
            }

            // Start animation on the new object (if any)
            if (newTarget != null)
            {
                newTarget.PlayPauseAnimation(true);
            }

            currentTarget = newTarget; // Update the current target
        }
        else if (newTarget == null && currentTarget != null)
        {
            // If no object is being looked at, stop the animation of the current target
            currentTarget.PlayPauseAnimation(false);
            currentTarget = null; // Reset the target
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Set the gizmo color

        // Draw forward line (center of the cone)
        Gizmos.DrawRay(transform.position, transform.forward * detectionRange);

        // Calculate the two outer edges of the cone
        Vector3 leftLimit = Quaternion.Euler(0, -detectionAngle, 0) * transform.forward;
        Vector3 rightLimit = Quaternion.Euler(0, detectionAngle, 0) * transform.forward;

        // Draw cone edges
        Gizmos.DrawRay(transform.position, leftLimit * detectionRange);
        Gizmos.DrawRay(transform.position, rightLimit * detectionRange);
    }
}
