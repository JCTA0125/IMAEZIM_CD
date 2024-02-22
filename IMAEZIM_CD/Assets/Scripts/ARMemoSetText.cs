using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ARMemoSetText : MonoBehaviour
{
    //public Transform _textObj;
    public TextMeshPro _text;
    public TextMeshPro _writer;
    public int postId;  //나중에 3d 객체 터치했을 때 알아야 하는 정보는 저장해두기


    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReceiveData(string recvText, string recvWriter)  //geo 코드에서 호출
    {
        //_text.text = data;  //받은 데이터로 3d 오브젝트의 text 바꾸기
        setText(recvText);
        setWriter(recvWriter);
    //3d 오브젝트 터치시 댓글 보이게 하려면 postId로 서버에서 데이터 불러와야함
    }

    //받은 데이터로 3d 오브젝트의 text 바꾸기
    private void setText(string recvText)
    { 
        if (recvText.Length > 40)
        {
            _text.text = String.Concat(recvText.Substring(0, 20), "...");  //텍스트 긴 경우
        }
        else
        {
            _text.text = recvText;
        }
    }

    //3d 오브젝트 글쓴이 띄우기
    private void setWriter(string recvWriter)
    {
        _writer.text = recvWriter;
    }
}
