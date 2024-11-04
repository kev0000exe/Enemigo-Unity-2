using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float velocidad;
    public float fuerzaSalto;
    public LayerMask capasuelo;
    public int SaltosMaximos;
    public float fuerzaRebote = 15f; // Fuerza aumentada para que el rebote sea más notorio
    public AudioManager audioManager;
    public AudioClip sonidoSalto;

    private Rigidbody2D rigidBody;
    private BoxCollider2D boxCollider;
    private bool mirandoAlaDerecha = true;
    private int SaltosRestantes;
    private Animator animator;
    private bool recibiendoDanio = false; // Para saber si está recibiendo daño

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        SaltosRestantes = SaltosMaximos;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!recibiendoDanio) // Solo puede moverse si no está recibiendo daño
        {
            procesarmovimiento();
            ProcesarSalto();
        }

        animator.SetBool("recibiendoDanio", recibiendoDanio);
    }

    // Verifica si el personaje está en el suelo
    bool EstaEnElsuelo()
    {
        RaycastHit2D hit = Physics2D.CircleCast(boxCollider.bounds.center, 0.1f, Vector2.down, 0.1f, capasuelo);
        Debug.Log(hit.collider != null ? "En el suelo" : "No en el suelo"); // Depuración para ver si se detecta el suelo
        return hit.collider != null;
    }

    // Maneja el salto del personaje
    void ProcesarSalto()
    {
        if (EstaEnElsuelo())
        {
            SaltosRestantes = SaltosMaximos;
        }

        if (Input.GetKeyDown(KeyCode.Space) && SaltosRestantes > 0)
        {
            SaltosRestantes--;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f);
            rigidBody.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

            // Reproduce sonido de salto si se ha asignado
            if (audioManager != null && sonidoSalto != null)
            {
                audioManager.ReproducirSonido(sonidoSalto);
            }
        }
    }

    // Método para recibir daño, con dirección y cantidad de daño
    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio)
        {
            recibiendoDanio = true;

            // Calcula la dirección de rebote dependiendo de la posición del golpe
            Vector2 direccionRebote = (transform.position.x > direccion.x) ? Vector2.right : Vector2.left;

            // Aplicar la fuerza de rebote hacia atrás y hacia arriba
            Vector2 fuerzaReboteAplicada = new Vector2(direccionRebote.x * fuerzaRebote, fuerzaSalto);
            rigidBody.AddForce(fuerzaReboteAplicada, ForceMode2D.Impulse);

            // Temporizador para desactivar el estado de daño
            Invoke("DesactivarDanio", 2f);
        }
    }

    // Método para desactivar el estado de daño
    public void DesactivarDanio()
    {
        recibiendoDanio = false;
    }

    // Maneja el movimiento del personaje
    void procesarmovimiento()
    {
        float inputmovimiento = Input.GetAxis("Horizontal");
        rigidBody.velocity = new Vector2(inputmovimiento * velocidad, rigidBody.velocity.y);
        GestionarOrientacion(inputmovimiento);

        if (inputmovimiento != 0f)
        {
            animator.SetBool("isRunning", true); // Asegúrate de que el parámetro se llame "isRunning"
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    // Cambia la orientación del personaje según el movimiento
    void GestionarOrientacion(float inputmovimiento)
    {
        if ((mirandoAlaDerecha && inputmovimiento < 0) || (!mirandoAlaDerecha && inputmovimiento > 0))
        {
            mirandoAlaDerecha = !mirandoAlaDerecha;
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }

    // Detectar la colisión con el enemigo
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("enemigo")) // Verifica si el objeto que tocó al personaje tiene la etiqueta "Enemigo"
        {
            RecibeDanio(collision.transform.position, 1); // Llama al método de recibir daño
        }
    }
}
