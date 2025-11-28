using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instancia;

    [Header("Velocidad")]
    [SerializeField] private float velocidadInicial = 6f;
    [SerializeField] private float aceleracionPorSegundo = 0.45f;   
    [SerializeField] private float velocidadMax = 24f;               
    public float VelocidadActual { get; private set; }

    [Header("Puntaje")]
    [SerializeField] private float puntosPorSegundo = 10f;
    [SerializeField] TextMeshProUGUI puntajeText;
    [SerializeField] GameObject EndPanel;
    [SerializeField] TextMeshProUGUI puntajeTextMenu;
    public float puntaje { get; private set; }

    [Header("Dificultad")]
    [SerializeField] private float tiempoHastaDificultadMax = 120f;  
    public float TiempoJugado { get; private set; }
    public float Dificultad01 => Mathf.Clamp01(TiempoJugado / tiempoHastaDificultadMax);

    public bool juegoActivo = true;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start() => ReiniciarJuego();

    void Update()
    {
        if (!juegoActivo) return;

        TiempoJugado += Time.deltaTime;
        VelocidadActual = Mathf.Min(velocidadInicial + aceleracionPorSegundo * TiempoJugado, velocidadMax);

        puntaje += puntosPorSegundo * Time.deltaTime;
        puntajeText.text = "Puntaje: " + Mathf.Round(puntaje);
    }

    public void AddPoints(int n)
    {
        puntaje += n;
    }

    public void ReiniciarJuego()
    {
        TiempoJugado = 0f;
        VelocidadActual = velocidadInicial;
        puntaje = 0f;
        juegoActivo = true;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        juegoActivo = false;
        EndPanel.SetActive(true);
        puntajeTextMenu.text ="" + Mathf.Round(puntaje);
        Time.timeScale = 0f;
        
    }

    public void ResetScene()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
