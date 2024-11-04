using System.Collections;
using UnityEngine;

public class ArbolMovimiento : MonoBehaviour
{
    public Transform jugador;
    public float velocidadMovimiento = 2f;
    public float fuerzaSalto = 5f;
    public float rangoDeteccion = 10f;
    public float tiempoEntreSaltos = 2f;
    public int daño = 1;
    public AudioClip sonidoMuerte;
    public AudioClip sonidoSalto;

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;
    private BoxCollider2D boxCollider;
    private bool muerto = false;
    private float proximoSalto;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();

        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player").transform;
        }
        proximoSalto = Time.time + tiempoEntreSaltos;
    }

    void Update()
    {
        if (muerto) return;

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        if (distanciaAlJugador <= rangoDeteccion)
        {
            PerseguirJugador();
        }

        if (Time.time >= proximoSalto)
        {
            Saltar();
            proximoSalto = Time.time + tiempoEntreSaltos;
        }

        VoltearHaciaJugador();
    }

    void PerseguirJugador()
    {
        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.velocity = new Vector2(direccion.x * velocidadMovimiento, rb.velocity.y);
    }

    void Saltar()
    {
        if (Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            if (sonidoSalto != null)
            {
                audioSource.PlayOneShot(sonidoSalto);
            }
            animator.SetTrigger("salto");
        }
    }

    void VoltearHaciaJugador()
    {
        if (jugador.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (muerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            NewCharacterController jugadorScript = collision.gameObject.GetComponent<NewCharacterController>();
            if (jugadorScript != null)
            {
                // Si el jugador toca al enemigo desde los lados o desde abajo, recibe daño
                jugadorScript.RecibeDanio(transform.position, daño);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (muerto) return;

        if (other.CompareTag("Player"))
        {
            NewCharacterController jugadorScript = other.GetComponent<NewCharacterController>();
            if (jugadorScript != null && jugadorScript.EstaSaltandoEncima(transform.position.y))
            {
                // Si el jugador cae estrictamente en la zona de golpe desde arriba, el enemigo muere
                Muerte();
                jugadorScript.Rebote();
            }
        }
    }

    void Muerte()
    {
        muerto = true;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Static;

        if (animator != null)
        {
            animator.SetTrigger("muerte");
        }

        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }
    }
}
