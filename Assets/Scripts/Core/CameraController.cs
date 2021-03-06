﻿using System;
using UnityEngine;

public class CameraController : MonoBehaviour, ISave
{
    [SerializeField]
    int objectID = 10000;
    public Transform Target = null;
    public float EffectTime = 1;
    public GameObject Map;
    public float CameraZoom = 1;    //увеличение камеры
    public float traction = 0.0f;   //расстояние (в юнитах), через которое камера начинает двигаться за целью
    public float SlideFactor = 0.5f;//параметр функции lerp для скольжения камеры

    Camera Camera;
    GameObject PanelBeforeCamera;
    Animator Animator;
    float fieldWidth, fieldHeight;      //размеры карты
    float cameraWidth, cameraHeight;    //половинные размеры камеры
    float UnitsPerPixel = 1f;
    private Vector2 UpLeft;

    bool ConstX, ConstY;    //Камера вмещает карту целиком и не двигается по Х и/или У.
    Vector3 prevTargetPos;    //Предыдущее положение цели камеры

    public void Awake()
    {
        if (objectID == 0)
            Debug.LogError("Объекту не назначен идентификатор.", gameObject);

        SaveManager.Subscribe(this);
    }

    private void Start ()
    {
        Application.targetFrameRate = 30;
        //DV{
        //transform.parent = Target;
        Camera = GetComponent<Camera>();

        Camera.orthographicSize = Screen.height * 0.5f * UnitsPerPixel / CameraZoom;
        cameraHeight = Camera.orthographicSize;
        cameraWidth = Screen.width * 0.5f * UnitsPerPixel / CameraZoom;
        Camera.aspect = cameraWidth / cameraHeight;

        TuneMap(Map);

        //Animator = GetComponentInChildren<Animator>();
        Animator = transform.Find("ShadowEffect").GetComponent<Animator>();
        Animator.SetFloat("Speed", 0.5f/EffectTime);
        PanelBeforeCamera = GameObject.Find("PanelBeforeCamera");
        PanelBeforeCamera.SetActive(false);
        //DV}

        prevTargetPos = Target.position;
    }

    void LateUpdate()
    {
        if (!Target) return;
        if (ConstX && ConstY) return;

        float newX = transform.position.x, newY = transform.position.y, newZ = transform.position.z;
        //Усреднение движения камеры
        Vector3 destination = (Target.position + prevTargetPos) * 0.5f;

        //Если Ширина камеры больше ширины карты, камеру по горизонтали не двигаем.
        if (!ConstX)
        {
            if (destination.x - transform.position.x > traction)
                newX = destination.x - traction;
            if (destination.x - transform.position.x < -traction)
                newX = destination.x + traction;

            //Проверка границ карты по Х
            if (newX + cameraWidth > UpLeft.x + fieldWidth)
                newX = UpLeft.x + fieldWidth - cameraWidth;
            if (newX - cameraWidth < UpLeft.x)
                newX = UpLeft.x + cameraWidth;
        }

        //Если Высота камеры больше высоты карты, камеру по вертикали не двигаем
        if (!ConstY)
        {
            if (destination.y - transform.position.y > traction)
                newY = destination.y - traction;
            if (destination.y - transform.position.y < -traction)
                newY = destination.y + traction;

            //Проверка границ карты по Y
            if (newY + cameraHeight > UpLeft.y)
                newY = UpLeft.y - cameraHeight;
            if (newY - cameraHeight < UpLeft.y - fieldHeight)
                newY = UpLeft.y - fieldHeight + cameraHeight;
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(newX, newY, newZ), SlideFactor);

        prevTargetPos = Target.position;
    }


  public void StartEffect(Color Color = new Color(), float EfTime = 0)
  {
        if (EfTime == 0)
            EfTime = EffectTime;

        Animator.SetFloat("Speed", 1f / EfTime);
        PanelBeforeCamera.GetComponent<UnityEngine.UI.Image>().color = Color;

        Animator.SetTrigger("Play");
  }

    public void TuneMap(GameObject map)//Boris Map -> map
    {
        Map = map;//Boris будем хранить фон в ссылке Map
        Sprite Spr = map.GetComponent<SpriteRenderer>().sprite;

        //Ширину и длину поля считаем в юнитах
        fieldWidth = Spr.rect.width * UnitsPerPixel;
        fieldHeight = Spr.rect.height * UnitsPerPixel;

        //Находим координаты левого верхнего угла карты
        UpLeft = (Vector2)map.transform.position + new Vector2(-Spr.pivot.x, Spr.rect.height - Spr.pivot.y) * UnitsPerPixel;

        ConstY = (2 * Camera.orthographicSize >= fieldHeight);
        ConstX = (2 * Camera.orthographicSize * Camera.aspect >= fieldWidth);

        //Если размер камеры больше размера карты, устанавливаем камеру в центр карты. Если камера меньше устанавливаем её на цель.
        Vector3 NewCamPosition = new Vector3(Target.position.x, Target.position.y, transform.position.z);
        if (ConstX)
            NewCamPosition.x = map.transform.position.x + fieldWidth * 0.5f;
        if(ConstY)
            NewCamPosition.y = map.transform.position.y - fieldHeight * 0.5f;

        transform.position = NewCamPosition;

        GameManager.GM.SetPartyWind(map.tag == "Wind");
    }

    public void OnDestroy()
    {
        SaveManager.Unsubscribe(this);
    }

    public SavedData GetDataToSave()
    {
        return new SavedData(GetObjID(), Map.name);
    }

    public void SetSavedData(string str)
    {
        Map = GameObject.Find(str);
        if (Map == null)
            Debug.LogError("В сцене не найдена требуемая локация: " + str, gameObject);
    }

    public int GetObjID()
    {
        if (objectID == 0)
            Debug.LogError("Объекту не назначен идентификатор.", gameObject);

        return objectID;
    }
}
