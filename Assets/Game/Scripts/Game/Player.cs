using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject coinParticle;

    private Rigidbody2D rb;
    private Transform child;
    private Transform grandChild;
    private SpriteRenderer childSr;
    private SpriteRenderer grandChildSr;
    private TrailRenderer trail;
    private ParticleSystem particles;

    private Vector2 prevPos;
    private Vector2 newPos;

    private Vector2 safePosition;

    private bool isGrounded = false;
    private bool onYellowRing = false;
    private bool onCyanRing = false;
    private PlayerMode playerMode = PlayerMode.CUBE;
    private bool orbJumpBuffer = false;
    private bool isFlip = false;

    private PlayerMode safePosPlayerMode = PlayerMode.CUBE;
    private bool safePosIsFlip = false;

    private bool isFlying = false;

    private float cubeJumpForce = 25f;
    private float ballJumpForce = 9.5f;
    private float ufoJumpForce = 10f;
    private float flyForce = 40f;
    private float speed = 6f;

    private float maxCubeYSpeed = 12.25f;
    private float maxShipYSpeed = 6f;
    private float maxBallYSpeed = 12.25f;
    private float maxUfoYSpeed = 6f;

    private float cubeGravityScale = 3.25f;
    private float ballGravityScale = 2.25f;
    private float shipGravityScale = 1.5f;
    private float ufoGravityScale = 1.5f;
    private float waveGravityScale = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        child = transform.GetChild(0);
        grandChild = child.GetChild(0);
        childSr = child.GetComponent<SpriteRenderer>();
        grandChildSr = grandChild.GetComponent<SpriteRenderer>();
        trail = GetComponent<TrailRenderer>();
        particles = GetComponent<ParticleSystem>();

        prevPos = transform.position;
        newPos = transform.position;

        rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

        gameManager.SetColors(particles, trail, childSr, grandChildSr);

        // this is setting for cube
        childSr.sprite = gameManager.CubeSprite();
        grandChildSr.sprite = gameManager.CubeSprite();
        trail.enabled = false;
        grandChild.gameObject.SetActive(false);
        rb.gravityScale = cubeGravityScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if (collision.gameObject.CompareTag("Block"))
        {
            if (playerMode == PlayerMode.WAVE)
            {
                gameManager.OnRevive();
            }
            else
            {
                Vector2 avgNormal = Vector2.zero;

                foreach (ContactPoint2D contact in collision.contacts)
                {
                    avgNormal += contact.normal;
                }

                avgNormal.Normalize();

                if (Mathf.Abs(avgNormal.x) > Mathf.Abs(avgNormal.y))
                {
                    if (avgNormal.x > 0)
                    {
                        isGrounded = true;
                    }
                    else
                    {
                        gameManager.OnRevive();
                    }
                }
                else
                {
                    isGrounded = true;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
        if (collision.gameObject.CompareTag("Block"))
        {
            isGrounded = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Spike"))
        {
            gameManager.OnRevive();
        }
        if (collision.CompareTag("Yellow Ring"))
        {
            onYellowRing = true;
        }
        if (collision.CompareTag("Cyan Ring"))
        {
            onCyanRing = true;
        }
        if (collision.CompareTag("Normal Gravity Portal"))
        {
            OnNormalGravityPortalEnter();
        }
        if (collision.CompareTag("Flip Gravity Portal"))
        {
            OnFlipGravityPortalEnter();
        }
        if (collision.CompareTag("Cube Portal"))
        {
            OnCubePortalEnter();
        }
        if (collision.CompareTag("Ship Portal"))
        {
            OnShipPortalEnter();
        }
        if (collision.CompareTag("Ball Portal"))
        {
            OnBallPortalEnter();
        }
        if (collision.CompareTag("UFO Portal"))
        {
            OnUFOPortalEnter();
        }
        if (collision.CompareTag("Wave Portal"))
        {
            OnWavePortalEnter();
        }
        if (collision.CompareTag("Coin"))
        {
            GameObject particle = Instantiate(coinParticle, collision.transform.position, Quaternion.identity, collision.transform.parent);

            Destroy(particle, 1f);

            gameManager.CollectCoin(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Yellow Ring"))
        {
            onYellowRing = false;
        }
        if (collision.CompareTag("Cyan Ring"))
        {
            onCyanRing = false;
        }
    }

    public void SetSafePosition(Vector2 position)
    {
        safePosition = position;


    }

    private void Update()
    {
        if (gameManager.GetGameState() == GameState.PLAYING)
        {
            if (transform.position.x > safePosition.x + 21)
            {
                safePosition = new Vector2(safePosition.x + 21, safePosition.y);

                safePosPlayerMode = playerMode;
                safePosIsFlip = isFlip;
            }

            switch (playerMode)
            {
                case PlayerMode.CUBE:

                    if (isGrounded)
                    {
                        child.eulerAngles = new Vector3(0f, 0f, Mathf.Round(child.eulerAngles.z / 90f) * 90f);
                        orbJumpBuffer = false;

                        if (Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                        {
                            Jump(cubeJumpForce);
                            isGrounded = false;
                        }
                    }
                    else
                    {
                        child.Rotate(new Vector3(0f, 0f, (isFlip ? 240 : -240f) * Time.deltaTime));

                        if (Input.touchCount > 0)
                        {
                            Touch touch = Input.GetTouch(0);

                            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            {
                                if (touch.phase == TouchPhase.Began)
                                {
                                    orbJumpBuffer = true;
                                }
                                if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                                {
                                    orbJumpBuffer = false;
                                }
                            }
                        }
                    }

                    if (onYellowRing && orbJumpBuffer)
                    {
                        Jump(cubeJumpForce);
                        onYellowRing = false;
                        orbJumpBuffer = false;
                    }

                    if (onCyanRing && orbJumpBuffer)
                    {
                        FlipGravity(cubeGravityScale);
                        onCyanRing = false;
                        orbJumpBuffer = false;
                    }

                    break;
                case PlayerMode.SHIP:
                    newPos = transform.position;

                    bool touching = Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    isFlying = touching;

                    Vector2 direction = newPos - prevPos;

                    if (direction != Vector2.zero)
                    {
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        child.eulerAngles = new Vector3(0f, 0f, angle);
                    }

                    prevPos = newPos;

                    break;
                case PlayerMode.BALL:

                    child.Rotate(new Vector3(0f, 0f, (isFlip ? 360 : -360f) * Time.deltaTime));

                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            if (touch.phase == TouchPhase.Began)
                            {
                                orbJumpBuffer = true;
                            }
                            if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                            {
                                orbJumpBuffer = false;
                            }
                        }
                    }

                    if (isGrounded && orbJumpBuffer)
                    {
                        FlipGravity(ballGravityScale);
                        isGrounded = false;
                        orbJumpBuffer = false;
                    }

                    if (onYellowRing && orbJumpBuffer)
                    {
                        Jump(ballJumpForce);
                        onYellowRing = false;
                        orbJumpBuffer = false;
                    }

                    if (onCyanRing && orbJumpBuffer)
                    {
                        FlipGravity(ballGravityScale);
                        onCyanRing = false;
                        orbJumpBuffer = false;
                    }

                    break;
                case PlayerMode.UFO:

                    if (Input.touchCount > 0)
                    {
                        Touch touch = Input.GetTouch(0);

                        if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            Jump(ufoJumpForce);
                        }
                    }

                    break;
                case PlayerMode.WAVE:

                    newPos = transform.position;

                    bool touchingWave = Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

                    if (touchingWave && !isFlying)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, isFlip ? -speed : speed);
                    }
                    
                    if (!touchingWave && isFlying)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, isFlip ? speed : -speed);
                    }

                    isFlying = touchingWave;

                    Vector2 direction2 = newPos - prevPos;

                    if (direction2 != Vector2.zero)
                    {
                        float angle = Mathf.Atan2(direction2.y, direction2.x) * Mathf.Rad2Deg;
                        child.eulerAngles = new Vector3(0f, 0f, angle - 90);
                    }

                    prevPos = newPos;

                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.GetGameState() == GameState.PLAYING)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            switch (playerMode)
            {
                case PlayerMode.CUBE:
                    float clampedCubeY = Mathf.Clamp(rb.linearVelocity.y, -maxCubeYSpeed, maxCubeYSpeed);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedCubeY);
                    break;
                case PlayerMode.SHIP:

                    if (isFlying)
                    {
                        rb.AddForce((isFlip ? -1 : 1) * flyForce * Vector2.up);
                    }

                    float clampedShipY = Mathf.Clamp(rb.linearVelocity.y, -maxShipYSpeed, maxShipYSpeed);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedShipY);

                    break;
                case PlayerMode.BALL:
                    float clampedBallY = Mathf.Clamp(rb.linearVelocity.y, -maxBallYSpeed, maxBallYSpeed);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedBallY);
                    break;
                case PlayerMode.UFO:
                    float clampedUfoY = Mathf.Clamp(rb.linearVelocity.y, -maxUfoYSpeed, maxUfoYSpeed);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, clampedUfoY);
                    break;
                case PlayerMode.WAVE:

                    if (isFlying)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, isFlip ? -speed : speed);
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, isFlip ? speed : -speed);
                    }

                    break;
            }
        }
    }

    public void SetPlayerAfterRevive()
    {
        transform.position = safePosition;
        trail.Clear();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        switch (safePosPlayerMode)
        {
            case PlayerMode.CUBE:
                OnCubePortalEnter();
                break;
            case PlayerMode.SHIP:
                OnShipPortalEnter();
                break;
            case PlayerMode.BALL:
                OnBallPortalEnter();
                break;
            case PlayerMode.UFO:
                OnUFOPortalEnter();
                break;
            case PlayerMode.WAVE:
                OnWavePortalEnter();
                break;
        }

        if (safePosIsFlip)
        {
            OnFlipGravityPortalEnter();
        }
        else
        {
            OnNormalGravityPortalEnter();
        }
    }

    private void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce((isFlip ? -1 : 1) * jumpForce * Vector2.up, ForceMode2D.Impulse);
    }

    private void FlipGravity(float gravityScale)
    {
        rb.gravityScale = -rb.gravityScale;
        isFlip = !isFlip;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, isFlip ? gravityScale : -gravityScale);
    }

    private void OnNormalGravityPortalEnter()
    {
        if (isFlip)
        {
            rb.gravityScale = -rb.gravityScale;
            isFlip = !isFlip;

            if (playerMode == PlayerMode.SHIP || playerMode == PlayerMode.UFO)
            {
                childSr.flipY = false;
                grandChildSr.flipY = false;
            }
        }
    }

    private void OnFlipGravityPortalEnter()
    {
        if (!isFlip)
        {
            rb.gravityScale = -rb.gravityScale;
            isFlip = !isFlip;

            if (playerMode == PlayerMode.SHIP || playerMode == PlayerMode.UFO)
            {
                childSr.flipY = true;
                grandChildSr.flipY = true;
            }
        }
    }

    private void OnCubePortalEnter()
    {
        PlayerMode currentMode = playerMode;

        playerMode = PlayerMode.CUBE;

        childSr.sprite = gameManager.CubeSprite();
        orbJumpBuffer = false;
        rb.gravityScale = isFlip ? -cubeGravityScale : cubeGravityScale;

        switch (currentMode)
        {
            case PlayerMode.CUBE:
                break;
            case PlayerMode.SHIP:
                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.BALL:
                break;
            case PlayerMode.UFO:
                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.WAVE:
                trail.enabled = false;
                particles.Play();
                transform.localScale = Vector3.one * 0.95f;
                break;
        }
    }

    private void OnShipPortalEnter()
    {
        PlayerMode currentMode = playerMode;

        playerMode = PlayerMode.SHIP;

        childSr.sprite = gameManager.ShipSprite();
        orbJumpBuffer = false;
        rb.gravityScale = isFlip ? -shipGravityScale : shipGravityScale;

        switch (currentMode)
        {
            case PlayerMode.CUBE:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }
                break;
            case PlayerMode.SHIP:
                break;
            case PlayerMode.BALL:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }
                break;
            case PlayerMode.UFO:
                break;
            case PlayerMode.WAVE:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }

                trail.enabled = false;
                particles.Play();
                transform.localScale = Vector3.one * 0.95f;
                break;
        }
    }

    private void OnBallPortalEnter()
    {
        PlayerMode currentMode = playerMode;

        playerMode = PlayerMode.BALL;

        childSr.sprite = gameManager.BallSprite();
        orbJumpBuffer = false;
        rb.gravityScale = isFlip ? -ballGravityScale : ballGravityScale;

        switch (currentMode)
        {
            case PlayerMode.CUBE:
                break;
            case PlayerMode.SHIP:
                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.BALL:
                break;
            case PlayerMode.UFO:
                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.WAVE:
                trail.enabled = false;
                particles.Play();
                transform.localScale = Vector3.one * 0.95f;
                break;
        }
    }

    private void OnUFOPortalEnter()
    {
        PlayerMode currentMode = playerMode;

        playerMode = PlayerMode.UFO;

        childSr.sprite = gameManager.UFOSprite();
        orbJumpBuffer = false;
        rb.gravityScale = isFlip ? -ufoGravityScale : ufoGravityScale;

        child.eulerAngles = Vector3.zero;

        switch (currentMode)
        {
            case PlayerMode.CUBE:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }
                break;
            case PlayerMode.SHIP:
                break;
            case PlayerMode.BALL:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }
                break;
            case PlayerMode.UFO:
                break;
            case PlayerMode.WAVE:
                grandChild.gameObject.SetActive(true);

                if (isFlip)
                {
                    childSr.flipY = true;
                    grandChildSr.flipY = true;
                }

                trail.enabled = false;
                particles.Play();
                transform.localScale = Vector3.one * 0.95f;
                break;
        }
    }

    private void OnWavePortalEnter()
    {
        PlayerMode currentMode = playerMode;

        playerMode = PlayerMode.WAVE;

        childSr.sprite = gameManager.WaveSprite();
        orbJumpBuffer = false;
        rb.gravityScale = isFlip ? -waveGravityScale : waveGravityScale;

        switch (currentMode)
        {
            case PlayerMode.CUBE:
                trail.enabled = true;
                trail.Clear();
                particles.Stop();
                transform.localScale = Vector3.one * 0.5f;
                break;
            case PlayerMode.SHIP:
                trail.enabled = true;
                trail.Clear();
                particles.Stop();
                transform.localScale = Vector3.one * 0.5f;

                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.BALL:
                trail.enabled = true;
                trail.Clear();
                particles.Stop();
                transform.localScale = Vector3.one * 0.5f;
                break;
            case PlayerMode.UFO:
                trail.enabled = true;
                trail.Clear();
                particles.Stop();
                transform.localScale = Vector3.one * 0.5f;

                childSr.flipY = false;
                grandChildSr.flipY = false;
                grandChild.gameObject.SetActive(false);
                break;
            case PlayerMode.WAVE:
                break;
        }
    }
}
