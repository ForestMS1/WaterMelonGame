using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public bool isDrag;
    Rigidbody2D rigid;

    public void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //������Ʈ ������
    }
    void Update()
    {
        if(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //������ǥ�� != ��ũ����ǥ�� //��ũ����ǥ�� -> ������ǥ��� �ٲ���

            //���콺 �¿� ��輳��
            float leftBorder = -4.2f + transform.localScale.x / 2;
            float rightBorder = 4.2f - transform.localScale.x / 2;

            if(mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if(mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }


            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f); //��ǥ�������� �ε巴�� �̵�(������ġ,��ǥ��ġ,����)
        }
        
    }

    public void Drag() //ĵ���� event trigger ������Ʈ���� ȣ��
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true; //������������ -> ��������
    }
}
