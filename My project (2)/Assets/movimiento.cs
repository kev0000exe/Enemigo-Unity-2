using System.Collections;
using UnityEngine;

public class BabosaController : MonoBehaviour
{
    public float velocidad = 2f;              // Velocidad de movimiento
    public float cambioDireccionTiempo = 3f;  // Tiempo entre cambios de direcci�n
    public Animator animator;                 // Referencia al Animator para activar animaciones

    private Rigidbody2D rigidBody;
    private bool mirandoAlaDerecha = true;    // Direcci�n actual
    private float tiempoUltimoCambio;

    private void Start()
    {
        // Asignamos el Rigidbody2D en modo Kinematic y activamos el Trigger
        rigidBody = GetComponent<Rigidbody2D>();

        if (rigidBody == null)
        {
            Debug.LogError("El Rigidbody2D no est� asignado o no est� presente en el objeto.");
            return;
        }

        rigidBody.isKinematic = true;
        GetComponent<BoxCollider2D>().isTrigger = true;

        // Inicializar el temporizador de cambio de direcci�n
        tiempoUltimoCambio = Time.time;
    }

    private void Update()
    {
        Mover();

        // Cambiar de direcci�n cada cierto tiempo
        if (Time.time - tiempoUltimoCambio > cambioDireccionTiempo)
        {
            CambiarDireccion();
            tiempoUltimoCambio = Time.time;
        }
    }

    private void Mover()
    {
        // Movimiento horizontal en la direcci�n actual
        float direccion = mirandoAlaDerecha ? 1f : -1f;
        transform.Translate(Vector2.right * direccion * velocidad * Time.deltaTime);
    }

    private void CambiarDireccion()
    {
        // Cambia la direcci�n del personaje
        mirandoAlaDerecha = !mirandoAlaDerecha;
        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
    }

    // Detecta la colisi�n con el jugador para activar la animaci�n de da�o
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador toc� al enemigo, activar animaci�n de da�o");
            if (animator != null)
            {
                animator.SetTrigger("RecibeDanio");
            }
        }
    }
}
