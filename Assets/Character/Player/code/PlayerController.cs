using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float dashPower = 10f;
    public float dashDuration = 0.2f;

    [Header("Sprites - Normal")]
    public Sprite normalUp;
    public Sprite normalDown;
    public Sprite normalSide;  

    [Header("Sprites - Attack(Dash)")]
    public Sprite attackUp;
    public Sprite attackDown;
    public Sprite attackSide;

    [Header("Attack Cooldown")]
    public float attackCooldown = 0.5f; 
    private float lastAttackTime = -999f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashTime;

    private Vector2 lastFaceDir = Vector2.down;
    private PlayerStats stats;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (!isDashing)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            if (moveInput != Vector2.zero)
                lastFaceDir = moveInput.normalized;

            UpdateSprite(isAttack: false, dir: lastFaceDir);
            if (Input.GetButtonDown("Fire1") && Time.time >= lastAttackTime + attackCooldown)
            {
                StartDash();
                lastAttackTime = Time.time;
            }

        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = dashDirection * dashPower;
            dashTime -= Time.fixedDeltaTime;

            if (dashTime <= 0f)
                EndDash();
        }
        else
        {
            rb.velocity = moveInput.normalized * stats.moveSpeed;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;

        dashDirection = (moveInput != Vector2.zero) ? moveInput.normalized : lastFaceDir;
        if (dashDirection == Vector2.zero) dashDirection = Vector2.up;

        UpdateSprite(isAttack: true, dir: dashDirection);
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;

        UpdateSprite(isAttack: false, dir: lastFaceDir);
    }

    void UpdateSprite(bool isAttack, Vector2 dir)
    {
        if (sr == null) return;

        bool horizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if (horizontal)
        {
            sr.flipX = dir.x < 0f; 
            sr.sprite = isAttack ? attackSide : normalSide;
        }
        else
        {
            sr.flipX = false;

            if (dir.y >= 0f)
                sr.sprite = isAttack ? attackUp : normalUp;
            else
                sr.sprite = isAttack ? attackDown : normalDown;
        }
    }
    void OnCollisionEnter2D(Collision2D c)
    {
        Debug.Log("HIT: " + c.gameObject.name);
    }

}
