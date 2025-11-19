using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Collider2D _col;
    public LayerMask wallLayer;

    public int damage = 1;

    public float Lifetime = 8f;
    private float elapsedTime = 0f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= Lifetime) { Destroy(gameObject); }
    }

    public void Fire(float speed, Vector2 direction)
    {
        //Debug.Log("Projectile Fired!");
        _rb.AddForceX(speed, ForceMode2D.Impulse);
        _rb.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & wallLayer) != 0)
        {
            Destroy(gameObject);
        }
    }

}
