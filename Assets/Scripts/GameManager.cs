using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup;


    void Start()
    {
        NextDongle();
    }


    Dongle GetDongle()
    {
        GameObject instant = Instantiate(donglePrefab, dongleGroup); //Instantiate() -> ������Ʈ�� ���� �������ִ� �Լ�
        Dongle instantDongle =  instant.GetComponent<Dongle>(); //Dongle c# ��ũ��Ʈ�� ������Ʈ�� ����� �Ǿ��ִ»���
        return instantDongle;
    }

    void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;

        StartCoroutine("WaitNext"); //�ڷ�ƾ�� ȣ���ϴ¹� StartCoroutine(WaitNext())�� ����
    }

    //�ڷ�ƾ(Coroutine) : ���� ��� ����Ƽ���� �ñ�� �Լ�
    //IEnumerator -> ������ �������̽�
    //yield => ����Ƽ�� �ڷ�ƾ�� �����ϱ� ���� Ű����
    IEnumerator WaitNext()
    {

        while(lastDongle != null) //������ ������� �ʾҴ� == �� ������ ����ִ�
        {
            yield return null;
        }
        //yield return null; //���������� ���� �ڵ�
        yield return new WaitForSeconds(2.5f); //2.5�� ���� �ð�

        NextDongle();
    }

    public void TouchDown()
    {
        if(lastDongle == null)
        {
            return;
        }
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if(lastDongle == null)
        {
            return;
        }
        lastDongle.Drop();
        lastDongle = null; //��ġ�� ������ �տ��� ���
    }
}
