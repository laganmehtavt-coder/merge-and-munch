using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class JellyCollision : MonoBehaviour {
    private Material mat;
    private Rigidbody2D rb;
    private CircleCollider2D circleCol;
    private PolygonCollider2D polyCol;

    // Base shape
    private Vector2[] basePoints;
    private Vector2[] deformedPoints;
    private float baseRadius;

    // Squeeze state
    private float currentSqueezeX = 1f;
    private float currentSqueezeY = 1f;
    private float targetSqueezeX = 1f;
    private float targetSqueezeY = 1f;

    // Wobble state
    private float wobbleVelX = 0f;
    private float wobbleVelY = 0f;
    private float wobbleX = 0f;
    private float wobbleY = 0f;

    private Vector2 lastContactNormal = Vector2.up;
    private Vector2 lastContactPoint;
    private bool inContact = false;
    private float staySqueezeTarget = 0f;

    [Header("=== BLOB SQUEEZE ===")]
    [Range(0.3f, 0.95f)] public float maxSquishX = 0.65f;
    [Range(1.0f, 1.8f)] public float maxStretchY = 1.35f;
    [Range(2f, 20f)] public float restoreSpeed = 10f;

    [Header("=== WOBBLE ===")]
    [Range(10f, 50f)] public float wobbleFreq = 25f;
    [Range(1f, 20f)] public float wobbleDamping = 8f;
    [Range(0f, 1f)] public float wobbleStrength = 0.4f;

    [Header("=== IMPACT ===")]
    [Range(0.1f, 3f)] public float impactMult = 1.8f;
    [Range(0f, 2f)] public float minForce = 0.4f;
    [Range(0f, 1f)] public float massFactor = 0.15f;

    [Header("=== SHADER ===")]
    [Range(0f, 0.5f)] public float vibration = 0.05f;
    [Range(10f, 100f)] public float stiffness = 60f;
    [Range(0.5f, 3f)] public float squeezeRadius = 1.5f;
    [Range(0f, 1f)] public float bulgePower = 0.5f;

    // ─────────────────────────────────────────────────────────
    void Start() {
        mat = GetComponent<SpriteRenderer>().material;
        rb = GetComponent<Rigidbody2D>();

        circleCol = GetComponent<CircleCollider2D>();
        polyCol = GetComponent<PolygonCollider2D>();

        // Circle → Polygon convert karo
        if (circleCol != null) {
            baseRadius = circleCol.radius;
            BuildPolygonFromCircle(baseRadius, circleCol.offset);
            Destroy(circleCol);
        } else if (polyCol != null) {
            basePoints = (Vector2[])polyCol.points.Clone();
            deformedPoints = new Vector2[basePoints.Length];
        }

        currentSqueezeX = 1f;
        currentSqueezeY = 1f;
        targetSqueezeX = 1f;
        targetSqueezeY = 1f;
    }

    // ── Circle se 24-point polygon ────────────────────────────
    void BuildPolygonFromCircle(float r, Vector2 offset) {
        int segs = 24;
        basePoints = new Vector2[segs];
        deformedPoints = new Vector2[segs];

        for (int i = 0; i < segs; i++) {
            float a = (float)i / segs * Mathf.PI * 2f;
            basePoints[i] = new Vector2(
                offset.x + Mathf.Cos(a) * r,
                offset.y + Mathf.Sin(a) * r
            );
        }

        polyCol = gameObject.AddComponent<PolygonCollider2D>();
        polyCol.points = basePoints;
    }

    // ── COLLISION ENTER ───────────────────────────────────────
    void OnCollisionEnter2D(Collision2D col) {
        float force = col.relativeVelocity.magnitude;
        if (force < minForce)
            return;

        lastContactNormal = col.contacts[0].normal;
        lastContactPoint = col.contacts[0].point;

        float squeeze = Mathf.Clamp01(force * impactMult * 0.1f);
        squeeze = AdjustForMass(col, squeeze);

        SetSqueezeTarget(squeeze, lastContactNormal);
        AddWobbleImpulse(squeeze, lastContactNormal);

        inContact = false;
    }

    // ── COLLISION STAY ────────────────────────────────────────
    void OnCollisionStay2D(Collision2D col) {
        if (col.rigidbody == null)
            return;

        lastContactNormal = col.contacts[0].normal;
        lastContactPoint = col.contacts[0].point;
        inContact = true;

        float squeeze = AdjustForMass(col, 0f);
        staySqueezeTarget = squeeze;

        SetSqueezeTarget(squeeze, lastContactNormal);
    }

    void OnCollisionExit2D(Collision2D col) {
        inContact = false;
        staySqueezeTarget = 0f;

        // Sirf dusre ko push — khud nahi
        if (col.rigidbody != null)
            col.rigidbody.AddForce(
                -lastContactNormal * rb.mass * 1.5f,
                ForceMode2D.Impulse);
    }

    // ── MASS LOGIC ────────────────────────────────────────────
    float AdjustForMass(Collision2D col, float baseSqueeze) {
        if (col.rigidbody == null)
            return baseSqueeze;

        float myMass = rb.mass;
        float otherMass = col.rigidbody.mass;
        float ratio = otherMass / Mathf.Max(myMass, 0.001f);

        if (otherMass > myMass * 1.1f) {
            // Dusra bhaari — main zyada squeeze
            return Mathf.Clamp01(baseSqueeze + ratio * massFactor);
        } else if (myMass > otherMass * 1.1f) {
            // Main bhaari — thoda squeeze
            return Mathf.Clamp01(baseSqueeze * 0.35f);
        } else {
            // Same mass
            return Mathf.Clamp01(baseSqueeze + 0.05f);
        }
    }

    // ── SQUEEZE TARGET SET ────────────────────────────────────
    void SetSqueezeTarget(float amount, Vector2 normal) {
        float absNX = Mathf.Abs(normal.x);
        float absNY = Mathf.Abs(normal.y);

        // Normal direction mein compress, perpendicular mein stretch
        if (absNY >= absNX) {
            // Upar/neeche — Y compress, X stretch
            targetSqueezeY = Mathf.Min(targetSqueezeY,
                             Mathf.Lerp(1f, maxSquishX, amount));
            targetSqueezeX = Mathf.Max(targetSqueezeX,
                             Mathf.Lerp(1f, maxStretchY, amount));
        } else {
            // Side — X compress, Y stretch
            targetSqueezeX = Mathf.Min(targetSqueezeX,
                             Mathf.Lerp(1f, maxSquishX, amount));
            targetSqueezeY = Mathf.Max(targetSqueezeY,
                             Mathf.Lerp(1f, maxStretchY, amount));
        }
    }

    // ── WOBBLE IMPULSE ────────────────────────────────────────
    void AddWobbleImpulse(float amount, Vector2 normal) {
        float str = amount * wobbleStrength;
        wobbleVelX += normal.y * str * wobbleFreq;
        wobbleVelY += normal.x * str * wobbleFreq;
    }

    // ── UPDATE ────────────────────────────────────────────────
    void Update() {
        if (!inContact) {
            targetSqueezeX = Mathf.MoveTowards(targetSqueezeX, 1f,
                             Time.deltaTime * restoreSpeed * 0.5f);
            targetSqueezeY = Mathf.MoveTowards(targetSqueezeY, 1f,
                             Time.deltaTime * restoreSpeed * 0.5f);
        }

        // Smooth scale
        currentSqueezeX = Mathf.Lerp(currentSqueezeX, targetSqueezeX,
                          Time.deltaTime * restoreSpeed);
        currentSqueezeY = Mathf.Lerp(currentSqueezeY, targetSqueezeY,
                          Time.deltaTime * restoreSpeed);

        // Spring wobble
        float springX = -wobbleX * wobbleFreq * wobbleFreq;
        float dampX = -wobbleVelX * wobbleDamping;
        wobbleVelX += (springX + dampX) * Time.deltaTime;
        wobbleX += wobbleVelX * Time.deltaTime;

        float springY = -wobbleY * wobbleFreq * wobbleFreq;
        float dampY = -wobbleVelY * wobbleDamping;
        wobbleVelY += (springY + dampY) * Time.deltaTime;
        wobbleY += wobbleVelY * Time.deltaTime;

        wobbleX = Mathf.Clamp(wobbleX, -0.3f, 0.3f);
        wobbleY = Mathf.Clamp(wobbleY, -0.3f, 0.3f);

        // Scale apply — wobble bhi saath
        float finalX = currentSqueezeX + wobbleX * 0.15f;
        float finalY = currentSqueezeY + wobbleY * 0.15f;

        transform.localScale = new Vector3(
            transform.localScale.x > 0
                ? Mathf.Abs(finalX) * Mathf.Sign(transform.localScale.x)
                : finalX,
            transform.localScale.y > 0
                ? Mathf.Abs(finalY) * Mathf.Sign(transform.localScale.y)
                : finalY,
            transform.localScale.z
        );

        // Shader update
        if (mat != null) {
            float shaderSqz = Mathf.Max(0f,
                1f - Mathf.Min(currentSqueezeX, currentSqueezeY)) * 2f;

            mat.SetFloat("_SqueezeAmount", shaderSqz);
            mat.SetFloat("_WobbleSpeed", stiffness);
            mat.SetFloat("_SqueezeRadius", squeezeRadius);
            mat.SetFloat("_BulgePower", bulgePower);

            mat.SetVector("_ContactPoint",
                new Vector4(lastContactPoint.x, lastContactPoint.y, 0, 0));
        }

        // Collider sync
        SyncCollider();
    }

    // ── COLLIDER SYNC ─────────────────────────────────────────
    void SyncCollider() {
        if (polyCol == null || basePoints == null)
            return;

        float sqzX = currentSqueezeX + wobbleX * 0.1f;
        float sqzY = currentSqueezeY + wobbleY * 0.1f;

        bool needsUpdate = Mathf.Abs(sqzX - 1f) > 0.005f
                        || Mathf.Abs(sqzY - 1f) > 0.005f;

        if (!needsUpdate) {
            polyCol.SetPath(0, basePoints);
            return;
        }

        for (int i = 0; i < basePoints.Length; i++) {
            deformedPoints[i] = new Vector2(
                basePoints[i].x * sqzX,
                basePoints[i].y * sqzY
            );
        }

        polyCol.SetPath(0, deformedPoints);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(lastContactPoint, 0.1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay((Vector2)transform.position, lastContactNormal * 0.5f);
    }
}