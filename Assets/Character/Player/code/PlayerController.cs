using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashPower = 10f;
    public float dashDuration = 0.2f;

    [Header("Sprites - Normal")]
    public Sprite normalUp;
    public Sprite normalDown;
    public Sprite normalSide;   // 오른쪽 방향 기준 1장만(좌우는 flipX)

    [Header("Sprites - Attack(Dash)")]
    public Sprite attackUp;
    public Sprite attackDown;
    public Sprite attackSide;   // 오른쪽 기준 1장만(좌우는 flipX)

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveInput;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashTime;

    // 마지막으로 바라본 방향(정지 중에도 방향 유지)
    private Vector2 lastFaceDir = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (!isDashing)
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");

            // 이동 입력이 있으면 마지막 방향 갱신
            if (moveInput != Vector2.zero)
                lastFaceDir = moveInput.normalized;

            // 일반 상태 스프라이트 갱신
            UpdateSprite(isAttack: false, dir: lastFaceDir);

            if (Input.GetButtonDown("Fire1"))
                StartDash();
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
            rb.velocity = moveInput.normalized * moveSpeed;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = dashDuration;

        dashDirection = (moveInput != Vector2.zero) ? moveInput.normalized : lastFaceDir;
        if (dashDirection == Vector2.zero) dashDirection = Vector2.up;

        // 대시 시작 시 공격 스프라이트
        UpdateSprite(isAttack: true, dir: dashDirection);
    }

    void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;

        // 대시 종료 후 일반 스프라이트로 복귀
        UpdateSprite(isAttack: false, dir: lastFaceDir);
    }

    void UpdateSprite(bool isAttack, Vector2 dir)
    {
        if (sr == null) return;

        // 가로/세로 우선 판단(대각선은 더 큰 축 기준으로 처리)
        bool horizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

        if (horizontal)
        {
            sr.flipX = dir.x < 0f; // 왼쪽이면 반전
            sr.sprite = isAttack ? attackSide : normalSide;
        }
        else
        {
            sr.flipX = false; // 위/아래는 반전 해제(원하면 유지해도 됨)

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
