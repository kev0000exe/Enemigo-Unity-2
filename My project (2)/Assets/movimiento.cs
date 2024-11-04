using System.Collections;
using UnityEngine;

public class BabosaController : MonoBehaviour
{
    public float velocidad = 2f;              // Velocidad de movimiento
    public float cambioDireccionTiempo = 3f;  // Tiempo entre cambios de dirección
    public Animator animator;                 // Referencia al Animator para activar animaciones

    private Rigidbody2D rigidBody;
    private bool mirandoAlaDerecha = true;    // Dirección actual
    private float tiempoUltimoCambio;

    private void Start()
    {
        // Asignamos el Rigidbody2D en modo Kinematic y activamos el Trigger
        rigidBody = GetComponent<Rigidbody2D>();

        if (rigidBody == null)
        {
            Debug.LogError("El Rigidbody2D no está asignado o no está presente en el objeto.");
            return;
        }

        rigidBody.isKinematic = true;
        GetComponent<BoxCollider2D>().isTrigger = true;

        // Inicializar el temporizador de cambio de dirección
        tiempoUltimoCambio = Time.time;
    }

    private void Update()
    {
        Mover();

        // Cambiar de dirección cada cierto tiempo
        if (Time.time - tiempoUltimoCambio > cambioDireccionTiempo)
        {
            CambiarDireccion();
            tiempoUltimoCambio = Time.time;
        }
    }

    private void Mover()
    {
        // Movimiento horizontal en la dirección actual
        float direccion = mirandoAlaDerecha ? 1f : -1f;
        transform.Translate(Vector2.right * direccion * velocidad * Time.deltaTime);
    }

    private void CambiarDireccion()
    {
        // Cambia la dirección del personaje
        mirandoAlaDerecha = !mirandoAlaDerecha;
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
    }

    // Detecta la colisión con el jugador para activar la animación de daño
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador tocó al enemigo, activar animación de daño");
            if (animator != null)
            {
                animator.SetTrigger("RecibeDanio");
            }
        }
    }
}
