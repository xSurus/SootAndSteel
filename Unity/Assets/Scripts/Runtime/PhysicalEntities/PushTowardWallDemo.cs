using UnityEngine;

namespace Gamelab.PhysicalEntities
{
    /// <summary>
    /// Demo-only driver for Assets/Scenes/PhysicsParityDemo.unity: pushes its PhysicalEntity toward the
    /// wall at a constant force, matching PhysicsMovementParityTests' force so pressing Play visually
    /// reproduces what that test asserts numerically.
    /// </summary>
    [RequireComponent(typeof(PhysicalEntity))]
    public class PushTowardWallDemo : MonoBehaviour
    {
        [SerializeField] private Vector2 force = new Vector2(10f, 0f);
        private PhysicalEntity entity;

        private void Awake()
        {
            entity = GetComponent<PhysicalEntity>();
        }

        private void FixedUpdate()
        {
            entity.Body.AddForce(force);
        }
    }
}
