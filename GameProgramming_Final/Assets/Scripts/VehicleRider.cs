using UnityEngine;

public class VehicleRider : MonoBehaviour
{
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
        if (collision.gameObject.CompareTag("Vehicle") && IsOnTop(collision))
        {
            vehicle = collision.transform;
            localOffset = vehicle.InverseTransformPoint(transform.position);
            player?.SetOnVehicle(true);
            player?.SetGrounded(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            if (IsOnTop(collision))
            {
                if (vehicle == null)
                {
                    vehicle = collision.transform;
                    localOffset = vehicle.InverseTransformPoint(transform.position);
                    player?.SetOnVehicle(true);
                }
                player?.SetGrounded(true);
            }
            else
            {
                if (vehicle == collision.transform)
                {
                    vehicle = null;
                    player?.SetOnVehicle(false);
                }
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            vehicle = null;
            player?.SetOnVehicle(false);
        }
    }

    void FixedUpdate()
    {
        if (vehicle != null && rigid != null)
        {
            Rigidbody vehicleRigid = vehicle.GetComponent<Rigidbody>();
            if (vehicleRigid != null)
            {
                Vector3 vehicleVelocity = vehicleRigid.velocity;
                Vector3 playerInput = player?.GetPlayerInput() ?? Vector3.zero;
                
                rigid.velocity = new Vector3(
                    vehicleVelocity.x + playerInput.x,
                    rigid.velocity.y,
                    vehicleVelocity.z + playerInput.z
                );
            }
        }
    }

    bool IsOnTop(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) return true;
        }
        return false;
    }
}