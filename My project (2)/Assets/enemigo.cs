using System.Collections;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    public Transform player;                // Referencia al jugador
    public float detectionRadius = 5.0f;    // Radio de detección para perseguir al jugador
    public float speed = 2.0f;              // Velocidad de movimiento
    public float damageInterval = 1f;       // Tiempo entre cada daño al jugador
    public AudioClip sonidoMuerte;          // Clip de sonido para la muerte
    public int vida = 1;                    // Vida del enemigo, 1 para que muera con un golpe

    private float nextDamageTime = 0f;
    private bool enMovimiento;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (animator != null)
        {
            animator.enabled = false;
            StartCoroutine(ActivateAnimator());
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // Agrega un AudioSource si no existe
        }
    }

    IEnumerator ActivateAnimator()
    {
        yield return new WaitForSeconds(0.1f);
        animator.enabled = true;
    }

    void Update()
    {
        PerseguirJugador();
    }

    private void PerseguirJugador()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            movement = new Vector2(direction.x, 0);  // Mover solo en el eje X
            enMovimiento = true;

            if (direction.x < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else if (direction.x > 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            movement = Vector2.zero;
            enMovimiento = false;
        }

        rb.MovePosition(rb.position + movement * speed * Time.deltaTime);

        if (animator != null && animator.enabled)
        {
            animator.SetBool("enMovimiento", enMovimiento);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            NewCharacterController playerScript = other.GetComponent<NewCharacterController>();
            if (playerScript != null && playerScript.EstaVivo())
            {
                playerScript.RecibeDanio(transform.position, 1);
                nextDamageTime = Time.time + damageInterval;
            }
        }
    }

    public void RecibeGolpe()
    {
        vida--;
        if (vida <= 0)
        {
            Muerte();
        }
    }

    private void Muerte()
    {
        if (animator != null)
        {
            animator.SetTrigger("muerto"); // Asegúrate de que el parámetro sea "muerto" en el Animator
        }

        // Reproduce el sonido de muerte y espera a que termine antes de destruir el objeto
        if (sonidoMuerte != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
            Invoke("DestruirEnemigo", sonidoMuerte.length); // Espera la duración del sonido antes de destruir
        }
        else
        {
            DestruirEnemigo(); // Si no hay sonido, destruye de inmediato
        }
    }

    private void DestruirEnemigo()
    {
        Destroy(gameObject);
    }
}
