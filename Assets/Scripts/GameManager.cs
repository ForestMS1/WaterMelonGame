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
        if(isOver)
        {
            return;
        }

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

    public void GmaeOver() //게임매니저에서 게임오버 함수 선언 후 동글에서 호출
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
        //1.장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();//FindObjectsOfType<T> : 장면에 올라온 T 컴포넌트들을 탐색 //Dongle 스크립트를 컴포넌트로 가지고있는것들을 전부 찾음

        //2. 지우기 전에 모든 동글의 물리효과 비활성화
        for(int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }

        //3. 1번의 목록을 하나씩 접근해서 지우기
        for(int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 100); //게임플레이 중에는 나올 수 없는 큰 값을 전달하여 숨기기
            yield return new WaitForSeconds(0.1f);
        }
    }
}
