using UnityEngine;

public class Movimiento3 : MonoBehaviour
{
    public float velocidad = 2f;
    public float cambioDireccionTiempo = 3f;
    public int da�o = 2; // Cantidad de da�o que hace al jugador
    public AudioClip sonidoMuerte; // Clip de sonido para la muerte (opcional)

    private Rigidbody2D rigidBody;
    private bool mirandoAlaDerecha = true;
    private float tiempoUltimoCambio;
    private bool estaMuerto = false;
    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        if (rigidBody == null)
        {
            Debug.LogError("El Rigidbody2D no est� asignado o no est� presente en el objeto.");
            return;
        }

        tiempoUltimoCambio = Time.time;
    }

    void Update()
    {
        if (estaMuerto) return;

        Mover();

        if (Time.time - tiempoUltimoCambio > cambioDireccionTiempo)
        {
            CambiarDireccion();
            tiempoUltimoCambio = Time.time;
        }
    }

    private void Mover()
    {
        float direccion = mirandoAlaDerecha ? 1f : -1f;
        rigidBody.velocity = new Vector2(direccion * velocidad, rigidBody.velocity.y);
    }

    private void CambiarDireccion()
    {
        mirandoAlaDerecha = !mirandoAlaDerecha;
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            NewCharacterController playerScript = collision.gameObject.GetComponent<NewCharacterController>();
            if (playerScript != null)
            {
                // Si el jugador cae sobre la cabeza del enemigo, el enemigo muere
                if (playerScript.EstaSaltandoEncima(transform.position.y))
                {
                    Muerte();
                    playerScript.Rebote(); // Hacer que el jugador rebote hacia arriba
                }
                else
                {
                    // Si el jugador no est� atacando o no est� de frente, el jugador recibe da�o
                    playerScript.RecibeDanio(transform.position, da�o);
                }
            }
        }
    }

    private void Muerte()
    {
        estaMuerto = true;
        rigidBody.velocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetTrigger("muerte");
        }

        // Reproducir sonido de muerte, si est� asignado
        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }
    }

    // Este m�todo ser� llamado desde el Animation Event al final de la animaci�n de muerte
    private void DestruirEnemigo()
    {
        Destroy(gameObject);
    }
}
