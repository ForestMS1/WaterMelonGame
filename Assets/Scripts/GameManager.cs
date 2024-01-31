using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("-----------------------[ Core ]")] // [Header] : 인스펙터에 말머리를 추가
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("-----------------------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int poolSize;
    public int poolCursor; //오브젝트풀 관리를 위한 사이즈, 커서 변수 추가
    public Dongle lastDongle;


    [Header("-----------------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip; //다양한 효과음을 저장할 AudioClip 배열 변수 선언
    public enum Sfx {LevelUp, Next, Attach, Button, Over}; //Enum : 상수들의 집합과도 같은 열거형 타입
    int sfxCursor; //다음에 재생할 AudioSource를 가리키는 변수 선언

    [Header("-----------------------[ ETC ]")]
    public GameObject line;
    public GameObject bottom;

    [Header("-----------------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;



    void Awake()
    {
        Application.targetFrameRate = 60; //targetFrameRate : 프레임(FPS) 설정 속성

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for(int index = 0; index < poolSize; index++)
        {
            MakeDongle();
        }

        if(!PlayerPrefs.HasKey("MaxScore")) //HasKey : 저장된 데이터가 있는지 확인하는 함수
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString(); //PlayerPrefs : 데이터 저장을 담당하는 클래스
    }
    public void GameStart()
    {
        //오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        //사운드 플레이
        bgmPlayer.Play(); //Play : AudioSource의 AudioClip을 재생하는 함수
        SfxPlay(Sfx.Button);

        //게임 시작 (동글생성)
        Invoke("NextDongle", 1.5f); //Invoke : 함수 호출에 딜레이를 주고 싶을 때 사용하는 함수
    }

    Dongle MakeDongle()
    {
        //이펙트생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect =  instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //오브젝트풀링 (ObjectPooling) : 미리 생성해둔 오브젝트 재활용 (Instantiate, Destroy를 사용할수록 파편화된 메모리 누적)

        //동글 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup); //Instantiate() -> 오브젝트를 새로 생성해주는 함수
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle =  instantDongleObj.GetComponent<Dongle>(); //Dongle c# 스크립트는 컴포넌트로 등록이 되어있는상태
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
            if(!donglePool[poolCursor].gameObject.activeSelf) //ActiveSelf : bool 형태의 오브젝트 활성화 속성
            {
                return donglePool[poolCursor];
            } 
        }
        return MakeDongle(); //모든 것이 활성화(사용중)이라면 return에 생성함수 반환
    }

    void NextDongle()
    {
        if(isOver)
        {
            return;
        }

        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0,maxLevel); //0~7까지
        lastDongle.gameObject.SetActive(true); //SetActive => 오브젝트 활성화 함수 -> Dongle의 OnEnable사용

        SfxPlay(Sfx.Next);
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

        yield return new WaitForSeconds(1f);

        //최고점수 갱신
        int maxScore = Mathf.Max(score,PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        //게임오버 UI 표시
        subScoreText.text = "점수 : " + scoreText.text;
        endGroup.SetActive(true);


        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");

    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Main"); //LoadScene : 원하는 장면을 새로 불러오는 함수
    }

    //효과음을 재생시켜주는 함수 선언
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

    void Update() //모바일에서 나가는 기능을 위해 Update에서 로직 추가
    {
        if(Input.GetButtonDown("Cancle"))
        {
            Application.Quit();
        }
    }
    
    void LateUpdate() //LateUpdate : Update 종료 후 실행되는 생명주기 함수
    {
        scoreText.text = score.ToString();
    }
}
