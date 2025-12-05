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
        player?.SetGrounded(true);

        if (collision.gameObject.CompareTag("Vehicle") && IsOnTop(collision))
        {
            vehicle = collision.transform;
            localOffset = vehicle.InverseTransformPoint(transform.position);
            player?.SetOnVehicle(true);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        player?.SetGrounded(true);

        if (collision.gameObject.CompareTag("Vehicle") && !IsOnTop(collision))
        {
            vehicle = null;
            player?.SetOnVehicle(false);
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
            Vector3 targetPos = vehicle.TransformPoint(localOffset);
            Vector3 moveVector = (targetPos - transform.position) / Time.fixedDeltaTime;
            rigid.velocity = new Vector3(moveVector.x, rigid.velocity.y, moveVector.z);
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