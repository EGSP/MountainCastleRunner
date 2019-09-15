using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPlane : MonoBehaviour, PlayerController.IPlayerLooped
{
    public static GroundPlane instance;

    [SerializeField] private SpriteRenderer[] ScrollableSprites = new SpriteRenderer[3];
    [SerializeField] private float[] factors = new float[3] { 1, 1, 1 }; // Интенсивность скролспрайтов

    private List<Obstacle> Obstacles = new List<Obstacle>(); // Все препятствия 

    [SerializeField] private float Range = 1;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        PlayerController.instance.OnRunStartEvent += RunStart;
        PlayerController.instance.OnRunStopEvent += RunStop;
    }

    public void GameUpdate()
    {
        var speed = PlayerController.instance.GetRunSpeed();
        Obstacle[] deleteObs = new Obstacle[Obstacles.Count];

        // Перемещение препятствий
        for(int i = 0; i < Obstacles.Count; i++)
        {
            var obs = Obstacles[i];

            // Прибавляем пройденный путь и перемещаем на этот пройденный путь
            obs.Distance += speed * Time.deltaTime;
            obs.transform.position += new Vector3(0, speed * Time.deltaTime, 0);

            // Удаляем объект со сцены
            if (obs.Distance > Range * 2)
            {
                deleteObs[i] = obs;
            }
        }

        // Оффсет стен и пола
        for(int i = 0; i < ScrollableSprites.Length; i++)
        {
            var ofs = ScrollableSprites[i].material.mainTextureOffset;
            ofs.y -= speed * Time.deltaTime*factors[i];
            ScrollableSprites[i].material.mainTextureOffset = ofs;
            
        }
        // Удаляем объекты со сцены

        for (int i = 0; i < deleteObs.Length; i++)
        {
            var obs = deleteObs[i];

            if (obs == null)
                continue;
            
            if (obs.Distance > Range * 2)
            {
                obs.Distance = 0;
                Obstacles.Remove(obs);
                SpawnController.instance.AddObstacle(obs);
            }
        }
    }

    public void RunStart()
    {
        GameManager.instance.OnGameUpdate += GameUpdate;
    }

    public void RunStop()
    {
        GameManager.instance.OnGameUpdate -= GameUpdate;

        for(int i = 0; i < Obstacles.Count; i++)
        {
            var obs = Obstacles[i];
            obs.Distance = 0;
            SpawnController.instance.AddObstacle(obs);
        }

        Obstacles.Clear();
    }

    // Возвращает длинну отрезка от спавна и до конца
    public float GetRange()
    {
        return Range * 2;
    }

    public void PlaceOnPlane(Obstacle obs)
    {
        Obstacles.Add(obs);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(transform.position - new Vector3(0, Range, 0), transform.position + new Vector3(0, Range, 0));

        Gizmos.DrawSphere(transform.position - new Vector3(0, Range, 0), 0.2f);
        Gizmos.DrawSphere(transform.position + new Vector3(0, Range, 0), 0.2f);
    }
}
