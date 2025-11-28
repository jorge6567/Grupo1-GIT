using UnityEngine;

public class LoopingFondo : MonoBehaviour
{
    [SerializeField] private float altoSprite = 10f;
    [SerializeField] private Transform[] piezas;
    [SerializeField] private float yReciclar = -10f;
    [SerializeField] private float yArriba = 10f;

    void Update()
    {
        if (!GameManager.Instancia.juegoActivo) return;

        float v = GameManager.Instancia.VelocidadActual;
        foreach (var t in piezas)
        {
            t.position += Vector3.down * v * Time.deltaTime;
            if (t.position.y <= yReciclar)
            {
                t.position = new Vector3(t.position.x, t.position.y + altoSprite * piezas.Length, t.position.z);
            }
        }
    }
}
