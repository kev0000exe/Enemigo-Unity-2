using System.Collections;
using UnityEngine;

public class Fantasma : MonoBehaviour
{
    public float velocidad = 5f;   // Velocidad del fantasma
    public int daño = 1;           // Daño que inflige al jugador
    private Transform objetivo;    // Referencia al jugador
    private SpriteRenderer spriteRenderer;
    private Animator animator;     // Referencia al Animator
    private bool estaMuerto = false; // Variable para controlar si el fantasma está muerto

    void Start()
    {
        objetivo = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (spriteRenderer == null)
        {
            Debug.LogError("No se encontró SpriteRenderer en el fantasma.");
        }
        if (animator == null)
        {
            Debug.LogError("No se encontró Animator en el fantasma.");
        }
    }

    void Update()
    {
        if (!estaMuerto && objetivo != null)
        {
            // Mover el fantasma hacia el jugador
            Vector2 direccion = (objetivo.position - transform.position).normalized;
            transform.Translate(direccion * velocidad * Time.deltaTime);

            // Voltear el sprite según la dirección hacia el jugador
            if (direccion.x < 0)
                spriteRenderer.flipX = true;
            else if (direccion.x > 0)
                spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta si el jugador cae sobre el fantasma (con etiqueta "JumpKill")
        if (collision.CompareTag("JumpKill") && !estaMuerto)
        {
            Debug.Log("El personaje cayó sobre el fantasma.");
            Muerte();

            // Impulso hacia arriba al personaje cuando cae sobre el fantasma
            Rigidbody2D playerRb = collision.GetComponentInParent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, 5f); // Cambia el valor para ajustar el impulso
            }
        }
        // Detecta si el fantasma colisiona directamente con el jugador
        else if (collision.CompareTag("Player") && !estaMuerto)
        {
            Debug.Log("El fantasma colisionó con el jugador.");
            NewCharacterController playerScript = collision.GetComponent<NewCharacterController>();
            if (playerScript != null)
            {
                playerScript.RecibeDanio(transform.position, daño);
            }
        }
    }

    private void Muerte()
    {
        estaMuerto = true;  // Marcar como muerto para que deje de moverse
        animator.SetTrigger("muerte"); // Activar la animación de muerte
        Debug.Log("Animación de muerte activada");

        // Después de 1 segundo (tiempo que dura la animación), destruir el objeto
        Destroy(gameObject, 1f);
    }
}
