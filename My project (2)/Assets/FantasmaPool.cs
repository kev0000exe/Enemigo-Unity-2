using System.Collections.Generic;
using UnityEngine;

public class FantasmaPool : MonoBehaviour
{
    public GameObject fantasmaPrefab;  // Prefab del fantasma
    public int cantidadInicial = 5;    // Número inicial de fantasmas en el pool

    private List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        // Crear la cantidad inicial de fantasmas y agregarlos al pool
        for (int i = 0; i < cantidadInicial; i++)
        {
            GameObject fantasma = Instantiate(fantasmaPrefab);
            fantasma.SetActive(false);
            pool.Add(fantasma);
        }
    }

    public GameObject ObtenerFantasma()
    {
        // Buscar un fantasma desactivado en el pool
        foreach (var fantasma in pool)
        {
            if (!fantasma.activeInHierarchy)
            {
                fantasma.SetActive(true);
                return fantasma;
            }
        }

        // Si no hay fantasmas desactivados, crear uno nuevo
        GameObject nuevoFantasma = Instantiate(fantasmaPrefab);
        nuevoFantasma.SetActive(true);
        pool.Add(nuevoFantasma);
        return nuevoFantasma;
    }

    public void DevolverFantasma(GameObject fantasma)
    {
        fantasma.SetActive(false); // Desactiva el fantasma en lugar de destruirlo
    }
}
