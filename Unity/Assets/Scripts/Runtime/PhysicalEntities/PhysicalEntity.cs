using UnityEngine;

namespace Gamelab.PhysicalEntities
{
    /// <summary>
    /// Ported from Src/PhysicalEntities/AbstractPhysicalEntity.cs. Wraps a sibling Rigidbody2D the way the
    /// original wrapped an Aether.Physics2D Body.
    ///
    /// Deliberately drops two things from the original (see docs/superpowers/plans/
    /// 2026-09-18-physics-movement-port-plan.md, "Design decisions"):
    /// - The pixel&lt;-&gt;meter Position conversion the original needed to bridge Aether (meters) with
    ///   XNA's pixel-space SpriteBatch. Unity's Rigidbody2D.position is already in world units; a
    ///   sprite's Pixels-Per-Unit import setting is what keeps world units visually matching the
    ///   original's pixel scale, so no runtime conversion is needed here.
    /// - Draw(SpriteBatch) and the CanHighlight/IsHighlighted/OnHighlight/OnHighlightRemoved highlight
    ///   state. Draw has no Unity equivalent (SpriteRenderer replaces it). Highlighting depends on
    ///   IHighlightable (Src/PhysicalEntities/Interfaces/, owned by agent A2) and a Player parameter
    ///   (Src/Players/, owned by agent A4) — retrofitting IPhysicalEntity/IHighlightable onto this class
    ///   is an integration task for whichever of those agents lands second.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PhysicalEntity : MonoBehaviour
    {
        private Rigidbody2D body;

        public Rigidbody2D Body => body;

        /// <summary>
        /// Alias for <see cref="Body"/> matching Src/PhysicalEntities/Interfaces/IPhysicalEntity.cs's
        /// `PhysicsBody` member name, so a later `: IPhysicalEntity` retrofit (A2) doesn't require
        /// renaming <see cref="Body"/> out from under existing call sites.
        /// </summary>
        public Rigidbody2D PhysicsBody => Body;

        public virtual Vector2 Position
        {
            get => Body.position;
            set => Body.position = value;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Configures this entity's body/collider to match the original's circle-body construction
        /// pattern: World.CreateCircle(radius, density, position, BodyType.Dynamic) + LinearDamping +
        /// FixedRotation (see e.g. Src/Players/Player.cs:76-79). Callers pick radius/density/damping per
        /// entity type, same as the original did per subclass. Gravity scale is forced to 0 to match the
        /// original's zero-gravity World (Src/Map/Train/State/GameplayContext.cs:15) even if a future
        /// change to ProjectSettings/Physics2DSettings.asset reintroduces global gravity.
        /// </summary>
        public CircleCollider2D ConfigureAsDynamicCircle(float radiusMeters, float density, float linearDamping,
            bool fixedRotation)
        {
            if (body == null) body = GetComponent<Rigidbody2D>();

            body.bodyType = RigidbodyType2D.Dynamic;
            body.useAutoMass = true;
            body.linearDamping = linearDamping;
            body.freezeRotation = fixedRotation;
            body.gravityScale = 0f;

            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null) collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = radiusMeters;
            collider.density = density;

            return collider;
        }
    }
}
