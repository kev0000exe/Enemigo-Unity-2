using System.Collections;
using UnityEngine;

public class Gigante : MonoBehaviour
{
    public float velocidad = 3f;
    public Transform jugador;
    public GameObject fantasmaPrefab;
    public Transform puntoDeLanzamiento;
    public float tiempoEntreLanzamientos = 2f;
    public float distanciaLanzamientoMaxima = 10f;
    public float distanciaDeteccion = 5f;
    public int vida = 3;
    public float saltoRebote = 5f;
    public AudioClip sonidoLanzamiento; // Clip de sonido para el lanzamiento
    public AudioClip sonidoMuerte; // Clip de sonido para la muerte
    public AudioClip sonidoDaño; // Clip de sonido para recibir daño

    private float tiempoProximoLanzamiento;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D boxCollider;
    private AudioSource audioSource; // Referencia al AudioSource
    private bool enMovimiento;
    private bool muerto;
    private bool recibiendoDanio;
    private bool sonidoMuerteReproducido = false; // Variable para asegurar que el sonido de muerte solo se reproduzca una vez

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>(); // Obtener el AudioSource

        if (jugador == null)
        {
            jugador = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (puntoDeLanzamiento == null)
        {
            puntoDeLanzamiento = this.transform;
        }

        tiempoProximoLanzamiento = Time.time + tiempoEntreLanzamientos;
    }

    void Update()
    {
        if (muerto)
        {
            animator.SetBool("caminar", false);
            return;
        }

        float distancia = Vector2.Distance(jugador.position, transform.position);

        if (distancia <= distanciaDeteccion)
        {
            float direccionX = Mathf.Sign(jugador.position.x - transform.position.x);
            transform.Translate(new Vector2(direccionX * velocidad * Time.deltaTime, 0));
            spriteRenderer.flipX = direccionX < 0;
            enMovimiento = true;

            boxCollider.offset = new Vector2(direccionX < 0 ? -Mathf.Abs(boxCollider.offset.x) : Mathf.Abs(boxCollider.offset.x), boxCollider.offset.y);
        }
        else
        {
            enMovimiento = false;
        }

        if (animator != null)
        {
            animator.SetBool("caminar", enMovimiento);
        }

        // Comprobar condiciones de lanzamiento de fantasmas
        if (distancia <= distanciaLanzamientoMaxima && Time.time >= tiempoProximoLanzamiento)
        {
            Debug.Log("Condiciones de lanzamiento cumplidas, lanzando fantasma.");
            LanzarFantasma();
            tiempoProximoLanzamiento = Time.time + tiempoEntreLanzamientos;
        }
    }

    void LanzarFantasma()
    {
        if (fantasmaPrefab != null)
        {
            Debug.Log("Lanzando un fantasma desde el punto de lanzamiento.");
            GameObject nuevoFantasma = Instantiate(fantasmaPrefab, puntoDeLanzamiento.position, Quaternion.identity);
            nuevoFantasma.transform.SetParent(null);
            Destroy(nuevoFantasma, 10f); // Destruye el fantasma después de 10 segundos

            // Reproducir el sonido de lanzamiento
            if (audioSource != null && sonidoLanzamiento != null)
            {
                audioSource.PlayOneShot(sonidoLanzamiento);
            }
            else
            {
                Debug.LogWarning("AudioSource o sonidoLanzamiento no asignado en el inspector.");
            }
        }
        else
        {
            Debug.LogWarning("FantasmaPrefab no asignado en el inspector.");
        }
    }

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !muerto)
        {
            vida -= cantDanio;
            recibiendoDanio = true;

            // Reproducir el sonido de daño
            if (audioSource != null && sonidoDaño != null)
            {
                audioSource.PlayOneShot(sonidoDaño);
            }
            else
            {
                Debug.LogWarning("AudioSource o sonidoDaño no asignado en el inspector.");
            }

            if (vida <= 0)
            {
                muerto = true;
                animator.SetBool("caminar", false);
                animator.SetTrigger("murio");
                StartCoroutine(DestruirGigante());
            }
            else
            {
                animator.SetTrigger("recibirDaño");
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                GetComponent<Rigidbody2D>().AddForce(rebote * 10f, ForceMode2D.Impulse);
                StartCoroutine(DesactivaDanio());
            }
        }
    }

    private IEnumerator DesactivaDanio()
    {
        yield return new WaitForSeconds(0.5f);
        recibiendoDanio = false;
        animator.ResetTrigger("recibirDaño");
    }

    private IEnumerator DestruirGigante()
    {
        // Reproducir el sonido de muerte antes de destruir el objeto solo si no se ha reproducido antes
        if (audioSource != null && sonidoMuerte != null && !sonidoMuerteReproducido)
        {
            audioSource.PlayOneShot(sonidoMuerte);
            sonidoMuerteReproducido = true; // Marcar el sonido como reproducido para evitar repeticiones
        }
        else if (sonidoMuerte == null)
        {
            Debug.LogWarning("AudioSource o sonidoMuerte no asignado en el inspector.");
        }

        yield return new WaitForSeconds(2f); // Espera para que el sonido se reproduzca completamente
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.position.y > transform.position.y + 0.5f)
            {
                Vector2 direccionDanio = new Vector2(other.transform.position.x, transform.position.y);
                RecibeDanio(direccionDanio, 1);

                Rigidbody2D rbJugador = other.GetComponent<Rigidbody2D>();
                if (rbJugador != null)
                {
                    Vector2 rebote = new Vector2(other.transform.position.x > transform.position.x ? saltoRebote : -saltoRebote, saltoRebote);
                    rbJugador.velocity = new Vector2(rbJugador.velocity.x, 0);
                    rbJugador.AddForce(rebote, ForceMode2D.Impulse);
                }
            }
        }
    }
}
