using TMPro;
using UnityEngine;

public class TextOnObject : MonoBehaviour
{
    public GameObject cube;
    //private TextMeshPro textMeshPro; //여기서만 사용
    void Start()
    {
        //TextMeshPro textMeshPro = cube.AddComponent<TextMeshPro>(); //textmeshpro가 만들어짐
        //TextMeshPro textMeshPro = new GameObject("NameText").AddComponent<TextMeshPro>(); // 따로 text object 생성
        //textMeshPro.text = "that's gonna be your name";

        //textMeshPro.fontSize = 10;
       // textMeshPro.color = Color.black;

       // textMeshPro.transform.parent = cube.transform; //text의 부모를 cube로 지정

        //UpdateFontSize();

        //float cubeSize = Mathf.Max(cube.transform.localScale.x, cube.transform.localScale.y, cube.transform.localScale.z);//cube의 x,y,z 중 한 길이
       // textMeshPro.fontSize = Mathf.RoundToInt(cubeSize * 0.2f);// 그 길이의 1/5가 폰트 사이즈가 됨.

      //  textMeshPro.transform.localPosition = Vector3.up * cube.transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
       // UpdateFontSize();
    }
    /*
    void UpdateFontSize()
    {
        float cubeSize = Mathf.Max(cube.transform.localScale.x, cube.transform.localScale.y, cube.transform.localScale.z);//cube의 x,y,z 중 한 길이
        this.textMeshPro.fontSize = Mathf.RoundToInt(cubeSize * 0.2f);// 그 길이의 1/5가 폰트 사이즈가 됨.
    }*/
}
