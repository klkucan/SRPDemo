using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonAdd : MonoBehaviour
{
    public Button[] Buttons;
    private Dictionary<string, Text> Texts = new Dictionary<string, Text>();
    public GameObject Parent;
    private int calCount = 1000;
    private int objCount = 10;
    private int newObjCount = 0;
    private static int calRate = 10000; // 计算的每帧变化数量
    private static int objRate = 1000; // 绘制对象的每帧变化数量
    private float radius = 100f; // 计算位置时用到的圆形半径


    private List<GameObject> _gameObjects = new List<GameObject>();

    public void AddCal()
    {
        calCount += calRate;
        Texts["AddCal"].text = string.Format("每帧增加10000次计数\n 当前计算数量 {0}", calCount);
    }

    public void SubCal()
    {
        if (calCount >= calRate)
        {
            calRate -= calRate;
        }

        Texts["SubObj"].text = string.Format("每帧减少10000次计数\n 当前计算数量 {0}", calCount);
    }

    public void AddObj()
    {
        Debug.Log("AddObj");
        newObjCount = objCount + objRate;
        Texts["AddObj"].text = string.Format("每帧增加1000个对象\n 当前对象数量 {0}", newObjCount);
    }

    public void SubObj()
    {
        if (newObjCount >= objRate)
        {
            newObjCount -= objRate;
        }

        Texts["SubObj"].text = string.Format("每帧减少1000个对象\n 当前对象数量 {0}", newObjCount);
    }

    private Vector3 getNewPosition()
    {
        return Random.insideUnitSphere * radius;
    }

    private void DrawObj(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _gameObjects.Add(Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere), getNewPosition(),
                Quaternion.identity,
                Parent.transform));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        string name = string.Empty;
        for (int i = 0; i < Buttons.Length; i++)
        {
            switch (Buttons[i].name)
            {
                case "AddCal":
                    Buttons[i].onClick.AddListener(AddCal);
                    name = "AddCal";
                    break;
                case "AddObj":
                    Buttons[i].onClick.AddListener(AddObj);
                    name = "AddObj";
                    break;
                case "SubObj":
                    Buttons[i].onClick.AddListener(SubObj);
                    name = "SubObj";
                    break;
                case "SubCal":
                    Buttons[i].onClick.AddListener(SubCal);
                    name = "SubCal";
                    break;
            }

            Texts.Add(name, Buttons[i].GetComponentInChildren<Text>());
        }

        newObjCount = objCount;
        DrawObj(newObjCount);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < calCount; i++)
        {
            i += 1;
        }

        if (newObjCount > objCount)
        {
            objCount = newObjCount;
            DrawObj(objRate);
        }
        else if (newObjCount < objCount)
        {
            if (objCount >= objRate)
            {
                for (int i = 0; i < objRate; i--)
                {
                    int index = _gameObjects.Count;
                    _gameObjects.RemoveAt(index);
                    GameObject.Destroy(_gameObjects[index]);
                }
            }

            objCount = newObjCount;
        }
    }
}