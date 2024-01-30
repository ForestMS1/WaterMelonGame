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
    public int maxLevel;


    void Awake()
    {
        Application.targetFrameRate = 60; //targetFrameRate : 프레임(FPS) 설정 속성
    }
    void Start()
    {
        NextDongle();
    }


    Dongle GetDongle()
    {
        //이펙트생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect =  instantEffectObj.GetComponent<ParticleSystem>();

        //동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup); //Instantiate() -> 오브젝트를 새로 생성해주는 함수
        Dongle instantDongle =  instantDongleObj.GetComponent<Dongle>(); //Dongle c# 스크립트는 컴포넌트로 등록이 되어있는상태
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    void NextDongle()
    {
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0,maxLevel); //0~7까지
        lastDongle.gameObject.SetActive(true); //SetActive => 오브젝트 활성화 함수 -> Dongle의 OnEnable사용

        StartCoroutine("WaitNext"); //코루틴을 호출하는법 StartCoroutine(WaitNext())도 가능
    }

    //코루틴(Coroutine) : 로직 제어를 유니티에게 맡기는 함수
    //IEnumerator -> 열겨헝 인터페이스
    //yield => 유니티가 코루틴을 제어하기 위한 키워드
    IEnumerator WaitNext()
    {

        while(lastDongle != null) //동글이 비워지지 않았다 == 선 위에서 놀고있다
        {
            yield return null;
        }
        //yield return null; //한프레임을 쉬는 코드
        yield return new WaitForSeconds(2.5f); //2.5초 쉬는 시간

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
        lastDongle = null; //터치를 끝내면 손에서 벗어남
    }
}
