using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public int score;
    public int maxLevel;
    public bool isOver;


    void Awake()
    {
        Application.targetFrameRate = 60; //targetFrameRate : ������(FPS) ���� �Ӽ�
    }
    void Start()
    {
        NextDongle();
    }


    Dongle GetDongle()
    {
        //����Ʈ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect =  instantEffectObj.GetComponent<ParticleSystem>();

        //���� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup); //Instantiate() -> ������Ʈ�� ���� �������ִ� �Լ�
        Dongle instantDongle =  instantDongleObj.GetComponent<Dongle>(); //Dongle c# ��ũ��Ʈ�� ������Ʈ�� ����� �Ǿ��ִ»���
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0,maxLevel); //0~7����
        lastDongle.gameObject.SetActive(true); //SetActive => ������Ʈ Ȱ��ȭ �Լ� -> Dongle�� OnEnable���

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

    public void GmaeOver() //���ӸŴ������� ���ӿ��� �Լ� ���� �� ���ۿ��� ȣ��
    {
        if(isOver)
        {
            return;
        }
        isOver = true;

        StartCoroutine("GameOverRoutine");
        
    }

    IEnumerator GameOverRoutine()
    {
        //1.��� �ȿ� Ȱ��ȭ �Ǿ��ִ� ��� ���� ��������
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();//FindObjectsOfType<T> : ��鿡 �ö�� T ������Ʈ���� Ž�� //Dongle ��ũ��Ʈ�� ������Ʈ�� �������ִ°͵��� ���� ã��

        //2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for(int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }

        //3. 1���� ����� �ϳ��� �����ؼ� �����
        for(int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 100); //�����÷��� �߿��� ���� �� ���� ū ���� �����Ͽ� �����
            yield return new WaitForSeconds(0.1f);
        }
    }
}
