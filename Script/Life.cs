using System.Collections;
using UnityEngine;

public class Life : MonoBehaviour
{
    public int hp;
    public float iframe;

    private SpriteRenderer spriteRenderer;
    public Material WhiteMaterial;
    private Material originalMaterial;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        if (iframe > 0)
        {
            iframe -= Time.deltaTime;
        }

        if (hp <= 0)
        {
            Death();
        }
    }
    public void Death()
    {
        if (GetComponent<FloatingHeadBoss>() == null) { Destroy(gameObject); }
    }
    public void TakeDamage(int damage)
    {
        if (iframe > 0) { return; }

        hp -= damage;

        if (GetComponent<FloatingHeadBoss>() != null) { GetComponent<FloatingHeadBoss>().TakeDamage(); }


        // flash sprite
        StartCoroutine(WhiteFlash());
    }
    private IEnumerator WhiteFlash()
    {
        spriteRenderer.material = WhiteMaterial;
        yield return new WaitForSeconds(0.07f);
        spriteRenderer.material = originalMaterial;
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.CompareTag("Player"))
        {
            if (collision.GetComponent<Projectile>() != null)
            {
                TakeDamage(collision.GetComponent<Projectile>().damage);
                Destroy(collision.gameObject);
            }
        }
    }
}
