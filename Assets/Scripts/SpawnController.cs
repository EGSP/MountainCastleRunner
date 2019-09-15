using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour, GameManager.IGameLooped,PlayerController.IPlayerLooped
{
    public static SpawnController instance;

    [SerializeField] private float QuadSize = 1; // Размер поля спавна
    [SerializeField] private int QuadsCount = 5; // Количество полей
    private Vector3[] quadsPos; // Центры квадов
    [Space]
    [SerializeField] private Obstacle[] PrefabTypes = null; // Типы препятствий
    [SerializeField] private float SpawnInterval = 1; // Интервал в метрах
    private float spawnInterval;
    [Range(0,1)]
    [SerializeField]private float MaxRow = 0;
    private float maxRow;
    
    private Queue<Obstacle> Obstacles = new Queue<Obstacle>(); // Препятствия  


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        
        var range = GroundPlane.instance.GetRange();
        int count = Mathf.RoundToInt(range * QuadsCount / SpawnInterval) + 1 * QuadsCount;
        

        int obsType = 0;
        int catchex = 0;
        while (true)
        {
            catchex++;
            if (catchex > 1000)
                throw new System.Exception("Cathcex");


            int piece = Mathf.Clamp(Random.Range(1, 5), 0, count);
            if ((count -= piece) <= 0)
                break; // Мы сгенерировали
            
            for(int i = 0; i < piece; i++)
            {
                var obs = Instantiate(PrefabTypes[obsType]);

                obs.transform.parent = this.transform;
                obs.gameObject.SetActive(false);
                Obstacles.Enqueue(obs);
            }

            // Меняем префаб
            obsType++;
            if (obsType >= PrefabTypes.Length)
                obsType = 0;
        }

        quadsPos = new Vector3[QuadsCount];
        for(int i = 0; i < QuadsCount; i++)
        {
            quadsPos[i] = transform.position + new Vector3(i * QuadSize + QuadSize/2, 0, 0);
        }

        maxRow = MaxRow;
        spawnInterval = SpawnInterval;

        GameManager.instance.OnGameStartEvent += GameStart;
        GameManager.instance.OnGameStopEvent += GameStop;

        PlayerController.instance.OnRunStartEvent += RunStart;
        PlayerController.instance.OnRunStopEvent += RunStop;

    }

    public void GameStart()
    {
        spawnInterval = SpawnInterval;
        maxRow = MaxRow;
    }

    public void GameStop()
    {

    }

    public void GameUpdate()
    {
        spawnInterval -= PlayerController.instance.GetRunSpeed() * Time.deltaTime;

        if(spawnInterval <= 0)
        {
            spawnInterval = SpawnInterval;

            // Spawn
            SpawnObstacle();

            // AddScorePoint
            GameManager.instance.AddPoint();
        }
    }
    

    public void RunStart()
    {
        GameManager.instance.OnGameUpdate += GameUpdate;
    }

    public void RunStop()
    {
        GameManager.instance.OnGameUpdate -= GameUpdate;
    }

    private void SpawnObstacle()
    {
        // Указывает на первый квад
        int pointer = 0;
        int catchex = 0;
        while(pointer < QuadsCount)
        {
            catchex++;
            if (catchex > 100)
                throw new System.Exception("Catchex Spawn");

            var maxRowInt = (int)(maxRow * QuadsCount); 

            // Пробел
            int space = Random.Range(0, QuadsCount-maxRowInt);
            pointer += space;
            
            if (pointer >= QuadsCount)
                break;

            // Препятствие
            int piece = Mathf.Clamp(Random.Range(1, maxRowInt),0,(int)((QuadsCount-pointer)*0.5f));
            for(int i = 0; i < piece; i++)
            {
                var pos = pointer + i; // Определяем квад
                var halfQuadSize = QuadSize / 2;
                var posV = transform.position + new Vector3(pos * QuadSize +halfQuadSize, halfQuadSize, 0);

                var obs = Obstacles.Dequeue();
                obs.gameObject.SetActive(true);
                obs.transform.position = posV;
                GroundPlane.instance.PlaceOnPlane(obs);
            }
            pointer += piece+1;
        }
        


    }

    public void AddObstacle(Obstacle obs)
    {
        obs.gameObject.SetActive(false);
        Obstacles.Enqueue(obs);
    }

    public void AddMaxRow(float value)
    {
        maxRow += value;
        maxRow = Mathf.Clamp(maxRow, 0, 1);
    }

    public void Set2DefaultMaxRow()
    {
        maxRow = MaxRow;
    }

    // Высчитывает индекс ячейки билже к которой стоит объект и возвращает позицию
    public Vector3 Clamp2Quads(Transform obj)
    {
        float x = obj.position.x;

        float index = (x - (transform.position.x)) / (QuadSize);

        int indexI = Mathf.FloorToInt(Mathf.Clamp(index, 0, QuadsCount - 1));

        return quadsPos[indexI];
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < 2; i++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, QuadSize * i, 0), transform.position + new Vector3(QuadSize * QuadsCount, QuadSize * i, 0));
        }

        for (int i = 0; i <= QuadsCount; i++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(QuadSize * i, 0), transform.position + new Vector3(QuadSize * i, QuadSize));
        }

        Gizmos.color = Color.cyan;

        for(int i= 0; i < QuadsCount; i++)
        {
            Gizmos.DrawSphere(transform.position + new Vector3(i * QuadSize + QuadSize / 2, QuadSize/2, 0), 0.2f);
        }
    }
}
