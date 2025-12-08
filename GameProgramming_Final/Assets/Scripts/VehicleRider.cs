using UnityEngine;

public class VehicleRider : MonoBehaviour
{
    [Header("References")]
    Player player;
    Rigidbody rigid;
    
    Transform vehicle;
    Vector3 localOffset;

    void Awake()
    {
        player = GetComponent<Player>();
        rigid = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Transform vehicleTransform = GetVehicleTransform(collision.gameObject);
        if (vehicleTransform == null) return;

        if (IsOnTop(collision, vehicleTransform))
        {
            vehicle = vehicleTransform;
            localOffset = vehicle.InverseTransformPoint(transform.position);
            player?.SetOnVehicle(true);
            player?.SetGrounded(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        Transform vehicleTransform = GetVehicleTransform(collision.gameObject);
        if (vehicleTransform == null) return;

        if (IsOnTop(collision, vehicleTransform))
        {
            if (vehicle == null)
            {
                vehicle = vehicleTransform;
                localOffset = vehicle.InverseTransformPoint(transform.position);
                player?.SetOnVehicle(true);
            }
            if (rigid.velocity.y <= 0.5f)
            {
                player?.SetGrounded(true);
            }
        }
        else if (vehicle == vehicleTransform)
        {
            vehicle = null;
            player?.SetOnVehicle(false);
        }
    }

    void Update()
    {
        if (vehicle != null)
        {
            Bounds vehicleBounds = GetVehicleBounds(vehicle);
            float vehicleTop = vehicleBounds.max.y;
            Bounds playerBounds = GetPlayerBounds();
            float playerBottom = playerBounds.min.y;
            
            if (playerBottom > vehicleTop - 0.3f && playerBottom < vehicleTop + 1f && rigid.velocity.y <= 0.5f)
            {
                player?.SetOnVehicle(true);
                player?.SetGrounded(true);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Transform vehicleTransform = GetVehicleTransform(collision.gameObject);
        if (vehicleTransform != null && vehicle == vehicleTransform)
        {
            vehicle = null;
            player?.SetOnVehicle(false);
        }
    }

    Transform GetVehicleTransform(GameObject collisionObject)
    {
        if (collisionObject.CompareTag("Vehicle"))
            return collisionObject.transform;

        Transform parent = collisionObject.transform.parent;
        while (parent != null)
        {
            if (parent.CompareTag("Vehicle"))
                return parent;
            parent = parent.parent;
        }
        return null;
    }

    void FixedUpdate()
    {
        if (vehicle == null || rigid == null) return;

        Rigidbody vehicleRigid = vehicle.GetComponent<Rigidbody>();
        if (vehicleRigid == null) return;

        Vector3 vehicleVelocity = vehicleRigid.velocity;
        Vector3 playerInput = player?.GetPlayerInput() ?? Vector3.zero;
        Vector3 targetVelocity = new Vector3(
            vehicleVelocity.x + playerInput.x,
            rigid.velocity.y,
            vehicleVelocity.z + playerInput.z
        );
        
        rigid.velocity = Vector3.Lerp(rigid.velocity, targetVelocity, Time.fixedDeltaTime * 20f);
    }

    bool IsOnTop(Collision collision, Transform vehicleTransform)
    {
        float playerY = transform.position.y;
        Bounds vehicleBounds = GetVehicleBounds(vehicleTransform);
        float vehicleTop = vehicleBounds.max.y;
        
        Bounds playerBounds = GetPlayerBounds();
        float playerBottom = playerBounds.min.y;
        
        if (playerBottom < vehicleTop - 0.5f) return false;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.1f && contact.point.y > vehicleTop - 0.5f)
            {
                return true;
            }
        }
        
        if (playerBottom > vehicleTop - 0.3f && playerBottom < vehicleTop + 1f)
        {
            return true;
        }
        
        return false;
    }

    Bounds GetPlayerBounds()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) return col.bounds;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) return renderer.bounds;
        
        return new Bounds(transform.position, Vector3.one);
    }

    Bounds GetVehicleBounds(Transform vehicleTransform)
    {
        Collider col = vehicleTransform.GetComponent<Collider>();
        if (col != null) return col.bounds;

        Renderer renderer = vehicleTransform.GetComponent<Renderer>();
        if (renderer != null) return renderer.bounds;

        return new Bounds(vehicleTransform.position, Vector3.one);
    }
}


