using UnityEngine;

public class VehicleRider : MonoBehaviour
{
    Transform originalParent;

    void Awake()
    {
        originalParent = transform.parent;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle") && IsOnTop(collision))
        {
            transform.SetParent(collision.transform);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            if (!IsOnTop(collision) && transform.parent == collision.transform)
            {
                transform.SetParent(originalParent);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Vehicle"))
        {
            transform.SetParent(originalParent);
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

