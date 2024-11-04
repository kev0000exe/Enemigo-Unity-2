using System.Collections;
using UnityEngine;

public class NewCharacterController : MonoBehaviour
{
    public float fuerzaRebote = 10f;
    public float velocidad = 5f;
    public float velocidadSprint = 8f;
    public float longitudRaycast = 3f;
    public float fuerzaSalto;
    public LayerMask capasuelo;
    public AudioManager audioManager;
    public AudioClip sonidoSalto; // Clip de sonido para el salto
    public AudioClip sonidoAtaque; // Clip de sonido para el ataque
    public AudioClip sonidoDaño; // Clip de sonido para recibir daño
    public int vida = 3;
    public float duracionAtaque = 0.5f;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    public Animator animator;
    private AudioSource audioSource;
    private bool recibiendoDanio;
    private bool enSuelo;
    private bool isSprinting;
    private bool atacando;
    private bool muerto;
    private bool esInvulnerable;
    private int contadorSaltos = 0; // Contador para el doble salto
    private int maxSaltos = 2; // Número máximo de saltos (doble salto)

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();

        if (rigidBody == null) Debug.LogWarning("Rigidbody2D no asignado.");
        if (boxCollider == null) Debug.LogWarning("BoxCollider2D no asignado.");
        if (animator == null) Debug.LogWarning("Animator no asignado en el Inspector.");
        if (audioSource == null) Debug.LogWarning("AudioSource no asignado en el Inspector.");
    }

    private void Update()
    {
        if (muerto) return;

        // Movimiento y ataque
        if (!atacando)
        {
            Movimiento();

            // Salto y doble salto
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IntentarSaltar();
            }
        }

        // Iniciar ataque si se presiona "Z"
        if (Input.GetKeyDown(KeyCode.Z) && !atacando)
        {
            StartCoroutine(Atacar());
        }

        // Actualizar animaciones
        animator.SetBool("recibiendoDanio", recibiendoDanio);
    }

    private void Movimiento()
    {
        float velocidadActual = isSprinting ? velocidadSprint : velocidad;
        float velocidadX = Input.GetAxis("Horizontal") * velocidadActual;

        rigidBody.velocity = new Vector2(velocidadX, rigidBody.velocity.y);

        animator.SetFloat("movement", Mathf.Abs(velocidadX));

        if (velocidadX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (velocidadX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isSprinting = true;
            animator.SetBool("isRunning", true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
            animator.SetBool("isRunning", false);
        }
    }

    private void IntentarSaltar()
    {
        if (contadorSaltos < maxSaltos)
        {
            Salto();
            contadorSaltos++;
        }
    }

    private void Salto()
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
        rigidBody.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

        // Reproduce el sonido de salto si está asignado
        if (sonidoSalto != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoSalto);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si colisiona con un objeto en la capa "suelo"
        if (collision.gameObject.layer == LayerMask.NameToLayer("suelo"))
        {
            contadorSaltos = 0; // Reiniciar el contador de saltos al tocar el suelo
        }
    }

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && vida > 0 && !esInvulnerable)
        {
            recibiendoDanio = true;
            vida -= cantDanio;

            if (sonidoDaño != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoDaño);
            }

            if (vida <= 0)
            {
                animator.SetBool("muerto", true);
                muerto = true;
            }
            else
            {
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                rigidBody.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
                animator.SetTrigger("daño");
            }
        }
    }

    public void DesactivaDanio()
    {
        recibiendoDanio = false;
        rigidBody.velocity = Vector2.zero;
    }

    private IEnumerator Atacar()
    {
        atacando = true;
        esInvulnerable = true;
        animator.SetBool("Atacando", true);
        animator.SetBool("isRunning", false); // Detener la animación de correr

        if (sonidoAtaque != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoAtaque);
        }

        yield return new WaitForSeconds(duracionAtaque);

        DesactivaAtaque();
        esInvulnerable = false;
    }

    private void DesactivaAtaque()
    {
        atacando = false;
        animator.SetBool("Atacando", false);
        if (isSprinting)
        {
            animator.SetBool("isRunning", true); // Restaurar la animación de correr si está sprinting
        }
    }

    public void FinAnimacionMuerte()
    {
        GetComponent<Animator>().enabled = false;
    }

    private bool EstaEnElSuelo()
    {
        RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, 0.2f, capasuelo);
        return hit.collider != null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (atacando && other.CompareTag("Enemy"))
        {
            var enemigo = other.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                float directionToEnemy = other.transform.position.x - transform.position.x;
                bool isEnemyInFront = (directionToEnemy > 0 && transform.localScale.x > 0) || (directionToEnemy < 0 && transform.localScale.x < 0);

                if (isEnemyInFront)
                {
                    enemigo.RecibeGolpe();
                }
            }
        }
    }

    public bool EstaVivo()
    {
        return vida > 0;
    }

    public bool EstaSaltandoEncima(float posicionYEnemigo)
    {
        return !enSuelo && rigidBody.velocity.y < 0 && transform.position.y > posicionYEnemigo;
    }

    public void ReboteTrasGolpe()
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0); // Reinicia la velocidad en Y
        rigidBody.AddForce(Vector2.up * fuerzaRebote, ForceMode2D.Impulse);
    }

    public void Rebote()
    {
        ReboteTrasGolpe();
    }
}
