using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName : int
{
    TitleScene = 0,
    ChatScene,
    VideoScene,
    EndScene
}

public class Main : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    static void FirstInit()
    {
        GameObject src = Resources.Load("SceneManagers/GameManager") as GameObject;
        Main main = Instantiate(src).GetComponent<Main>();
    }

    //簡易シングルトン
    static Main _instance = null;

    public static Main instance
    {
        get
        {
            return _instance ?? (_instance = FindObjectOfType<Main>());
        }
    }

    void Awake()
    {
        if (this != instance)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

    }

    void OnDestroy()
    {
        if (this == instance)
        {
            _instance = null;
        }
    }

    [SerializeField] GameObject[] sceneManagers = new GameObject[4];

    void Start()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        InitializeScene(index);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeScene(scene.buildIndex);
    }

    void InitializeScene(int _sceneIndex)
    {
        ISceneManager instance = Instantiate(sceneManagers[_sceneIndex]).GetComponent<ISceneManager>();
        instance.Initialize();
    }

    /// <summary>
    /// 指定されたシーンへ遷移
    /// </summary>
    /// <param name="_next"></param>
    public void GoNext(SceneName _next)
    {
        SceneManager.LoadScene((int)_next);
    }
}
