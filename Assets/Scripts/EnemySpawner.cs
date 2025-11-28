using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Carriles (debe coincidir con el Jugador)")]
    [SerializeField] private float xCentro = 0f;
    [SerializeField] private float separacionCarril = 2.2f;

    [Header("Spawn por distancia")]
    [SerializeField] private float ySpawn = 8f;
    [SerializeField] private float yDespawn = -7f;

    [Tooltip("Distancia (en unidades de mundo) entre patrones al inicio (fácil).")]
    [SerializeField] private float distanciaMaxFacil = 12f;

    [Tooltip("Distancia mínima cuando la dificultad está al 100%.")]
    [SerializeField] private float distanciaMinDificil = 5.5f;

    [Tooltip("Variación aleatoria +/- en porcentaje (0.2 = ±20%).")]
    [SerializeField] private float jitterPorc = 0.18f;

    [Header("Patrones y dificultad")]
    [Tooltip("Aumenta la chance de patrones complejos al subir la dificultad.")]
    [SerializeField] private bool patronesEscalanConDificultad = true;

    [SerializeField] private GameObject[] prefabsObstaculo;
    [SerializeField] private int prewarmPorPrefab = 6;

    private readonly List<GameObject> pool = new();

    private float distanciaDesdeUltimoSpawn = 0f;
    private float distanciaObjetivoActual = 8f;

    private readonly float[] lanesX = new float[3];

    void Start()
    {
        lanesX[0] = xCentro - separacionCarril;
        lanesX[1] = xCentro;
        lanesX[2] = xCentro + separacionCarril;

        PrecalentarPool();
        distanciaObjetivoActual = CalcularSiguienteDistanciaObjetivo();
    }

    void Update()
    {
        if (!GameManager.Instancia.juegoActivo) return;

        float v = GameManager.Instancia.VelocidadActual;
        float dt = Time.deltaTime;

        for (int i = 0; i < pool.Count; i++)
        {
            var go = pool[i];
            if (!go.activeSelf) continue;
            go.transform.position += Vector3.down * v * dt;
            if (go.transform.position.y < yDespawn) go.SetActive(false);
        }

        distanciaDesdeUltimoSpawn += v * dt;

        if (distanciaDesdeUltimoSpawn >= distanciaObjetivoActual)
        {
            distanciaDesdeUltimoSpawn = 0f;
            distanciaObjetivoActual = CalcularSiguienteDistanciaObjetivo();
            SpawnearPatronSegunDificultad();
        }
    }

    float CalcularSiguienteDistanciaObjetivo()
    {
        float t = GameManager.Instancia.Dificultad01;
        float baseDist = Mathf.Lerp(distanciaMaxFacil, distanciaMinDificil, t);

        float jitter = baseDist * Random.Range(-jitterPorc, jitterPorc);
        float dist = Mathf.Max(2.5f, baseDist + jitter); 
        return dist;
    }

    void SpawnearPatronSegunDificultad()
    {
        if (prefabsObstaculo == null || prefabsObstaculo.Length == 0) return;

        float t = GameManager.Instancia.Dificultad01;

        float wSingle = 1.0f;                          
        float wDouble = patronesEscalanConDificultad ? Mathf.Lerp(0.15f, 0.75f, t) : 0.35f;
        float wMuroConHueco = patronesEscalanConDificultad ? Mathf.Max(0f, (t - 0.35f)) * 0.9f : 0.15f;

        float suma = wSingle + wDouble + wMuroConHueco;
        float r = Random.value * suma;

        if (r < wSingle) PatronSingle();
        else if (r < wSingle + wDouble) PatronDouble();
        else PatronMuroConHueco();
    }

    void PatronSingle()
    {
        int lane = Random.Range(0, 3);
        SpawnEnLane(lane, ySpawn);
    }

    void PatronDouble()
    {
        int libre = Random.Range(0, 3);
        for (int lane = 0; lane < 3; lane++)
            if (lane != libre) SpawnEnLane(lane, ySpawn);
    }

    void PatronMuroConHueco()
    {
        int hueco = Random.Range(0, 3);
        for (int lane = 0; lane < 3; lane++)
            if (lane != hueco) SpawnEnLane(lane, ySpawn);
    }

    void SpawnEnLane(int lane, float y)
    {
        var prefab = prefabsObstaculo[Random.Range(0, prefabsObstaculo.Length)];
        var go = TomarDelPool(prefab);
        go.transform.position = new Vector3(lanesX[lane], y, 0f);
        go.SetActive(true);
    }

    void PrecalentarPool()
    {
        foreach (var p in prefabsObstaculo)
        {
            for (int i = 0; i < prewarmPorPrefab; i++)
            {
                var go = Instantiate(p, Vector3.one * 9999f, Quaternion.identity);
                go.tag = "Obstaculo";
                if (go.TryGetComponent<Collider2D>(out var col)) col.isTrigger = true;
                go.SetActive(false);
                pool.Add(go);
            }
        }
    }

    GameObject TomarDelPool(GameObject prefab)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            var go = pool[i];
            if (!go.activeSelf && go.name.StartsWith(prefab.name)) return go;
        }
        var nuevo = Instantiate(prefab);
        nuevo.tag = "Obstaculo";
        if (nuevo.TryGetComponent<Collider2D>(out var col)) col.isTrigger = true;
        nuevo.SetActive(false);
        pool.Add(nuevo);
        return nuevo;
    }
}
