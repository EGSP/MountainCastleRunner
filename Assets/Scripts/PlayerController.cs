using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, GameManager.IGameLooped
{
    public static PlayerController instance;

    public delegate void PlayerLoopDelegate();
    public event PlayerLoopDelegate OnRunStartEvent = delegate { };
    public event PlayerLoopDelegate OnRunStopEvent = delegate { };

    public interface IPlayerLooped
    {
        void RunStart();
        void RunStop();
    }

    [SerializeField] private float HorizontalRange = 1;
    [SerializeField] private float PreDistance = 1; // Дистанция перед началом забега (пока персонаж не добежит до нужной точки Y, игра не начнётся)
                     private float preDistance = 0;
 
    [Space]
    [SerializeField] private float RunSpeed = 1; // Скорость бега
                     private float runSpeed;
    [SerializeField] private float HorizontalSpeed = 1; // Горизонтальная скорость бега (max value of acceleration)
    [SerializeField] private float AccelerationFactor = 1; // How fast do the horizontal acceleration rise?
    [SerializeField] private float AccelerationDownFactor = 2; // Как быстро будет падать ускорение
                     private float acceleration = 0;
    [Range(0,1)]
    [SerializeField] private float ClampFactor = 0.5f; // Насколько быстро будет смещаться игрок

    private bool IsRun; // Начался ли основной забег

    [Space]
    public Transform PlayerSprite = null; // Спрайт, который мы двигаем вместо контроллера
    private int direction = 0; // left(-1) ot right(+1)


    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        GameManager.instance.OnGameStartEvent += GameStart;
        GameManager.instance.OnGameStopEvent += GameStop;

        runSpeed = RunSpeed;
    }

    public void GameStart()
    {
        GameManager.instance.OnGameUpdate += GameUpdate;

        acceleration = 0;
        runSpeed = RunSpeed;

        PlayerSprite.position = transform.position;
        preDistance = 0;

        IsRun = false;

        direction = 0;
    }

    public void GameStop()
    {
        GameManager.instance.OnGameUpdate -= GameUpdate;
    }

    public void GameUpdate()
    {
        
        if(IsRun == false)
        {
            if (preDistance >= PreDistance)
            {
                IsRun = true;
                OnRunStartEvent?.Invoke();
            }

            preDistance += RunSpeed * Time.deltaTime;
            PlayerSprite.position -= new Vector3(0, RunSpeed * Time.deltaTime, 0);
        }
        else
        {
            var input = Input.GetKey(KeyCode.Mouse0);

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                direction *= (-1);
                acceleration = 2;
            }

            // Самый первый клик
            if (direction == 0 && input == true)
            {
                var x = Input.mousePosition;
                x.x /= Screen.width;

                direction = x.x <= 0.5f ? -1 : 1;
                
            }

            // Перемещение спрайта
            if(input == true)
            {
                print("input == true");
                PlayerSprite.position += new Vector3(acceleration * Time.deltaTime*direction, 0, 0);
                var posx = Mathf.Clamp(PlayerSprite.position.x,transform.position.x -HorizontalRange,transform.position.x+HorizontalRange);
                PlayerSprite.position = new Vector3(posx, PlayerSprite.position.y, PlayerSprite.position.z);


                acceleration += AccelerationFactor * Time.deltaTime;
                if (acceleration > HorizontalSpeed)
                    acceleration = HorizontalSpeed;
            }
            else // Игрок отпустил input
            {
                acceleration -= AccelerationDownFactor * Time.deltaTime;

                if (acceleration < 2)
                    acceleration = 2;

                Clamp2Quads();
            }


        }

        var cols = Physics2D.OverlapCircleAll(PlayerSprite.position, 0.2f);

        for(int i = 0; i < cols.Length; i++)
        {
            print("Work");
            if(cols[i].tag == "Obs")
            {
                GameManager.instance.StopGame();

                OnRunStopEvent?.Invoke();

            }
        }
        
        
    }

    public float GetRunSpeed()
    {
        return runSpeed;
    }

    // Округляем позицию до ближайшей ячейки (Х)
    private void Clamp2Quads()
    {
        var pos = Vector3.Lerp(PlayerSprite.position, SpawnController.instance.Clamp2Quads(PlayerSprite), ClampFactor);

        PlayerSprite.position = new Vector3(pos.x, PlayerSprite.position.y, 0);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -PreDistance, 0));
        Gizmos.DrawLine(transform.position + new Vector3(-HorizontalRange, 0, 0), transform.position + new Vector3(HorizontalRange, 0, 0));

       

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.position + new Vector3(0, -PreDistance, 0), 0.2f);

        Gizmos.DrawSphere(transform.position + new Vector3(-HorizontalRange, 0, 0), 0.2f);
        Gizmos.DrawSphere(transform.position + new Vector3(HorizontalRange, 0, 0), 0.2f);


    }

}
