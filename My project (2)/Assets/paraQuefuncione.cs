using UnityEngine;

public class SoporteController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Si el enemigo choca con el soporte, evita que se caiga
            collision.rigidbody.velocity = Vector2.zero;
        }
    }
}
