using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor; //������ƮǮ ������ ���� ������, Ŀ�� ���� �߰�
    public Dongle lastDongle;

    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip; //�پ��� ȿ������ ������ AudioClip �迭 ���� ����
    public enum Sfx {LevelUp, Next, Attach, Button, Over}; //Enum : ������� ���հ��� ���� ������ Ÿ��
    int sfxCursor; //������ ����� AudioSource�� ����Ű�� ���� ����
    public int score;
    public int maxLevel;
    public bool isOver;


    void Awake()
    {
        Application.targetFrameRate = 60; //targetFrameRate : ������(FPS) ���� �Ӽ�

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }
    }
    void Start()
    {
        bgmPlayer.Play(); //Play : AudioSource�� AudioClip�� ����ϴ� �Լ�
        NextDongle();
    }

    Dongle MakeDongle()
    {
        //����Ʈ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect =  instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //������ƮǮ�� (ObjectPooling) : �̸� �����ص� ������Ʈ ��Ȱ�� (Instantiate, Destroy�� ����Ҽ��� ����ȭ�� �޸� ����)

        //���� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup); //Instantiate() -> ������Ʈ�� ���� �������ִ� �Լ�
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle =  instantDongleObj.GetComponent<Dongle>(); //Dongle c# ��ũ��Ʈ�� ������Ʈ�� ����� �Ǿ��ִ»���
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }


    Dongle GetDongle()
    {
        for(int index = 0; index < donglePool.Count; index++)
        {
            poolCursor = (poolCursor+1) % donglePool.Count;
            if(!donglePool[poolCursor].gameObject.activeSelf) //ActiveSelf : bool ������ ������Ʈ Ȱ��ȭ �Ӽ�
            {
                return donglePool[poolCursor];
            } 
        }
        return MakeDongle(); //��� ���� Ȱ��ȭ(�����)�̶�� return�� �����Լ� ��ȯ
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0,maxLevel); //0~7����
        lastDongle.gameObject.SetActive(true); //SetActive => ������Ʈ Ȱ��ȭ �Լ� -> Dongle�� OnEnable���

        SfxPlay(Sfx.Next);
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

        yield return new WaitForSeconds(1f);

        SfxPlay(Sfx.Over);
    }

    //ȿ������ ��������ִ� �Լ� ����
    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;

            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;

            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;

            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;

            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
}
