using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge; //합칠 때 다른 동글이 개입하지 않도록 잠금역할 해주는 변수 추가
    public bool isAttach;
    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;

    public void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //컴포넌트 가져옴
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() //OnEnable : 스크립트가 활성화 될 때 실행되는 이벤트함수 //오브젝트가 생성이되거나 활성화될때 자동으로 실행이되는 이벤트함수
    {
        anim.SetInteger("Level", level);
    }

    private void OnDisable() //OnDisable : 비활성화 시 호출되는 이벤트함수
    { 
        //동글 속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero; //DongleGroup안에 들어가있기때문에 localPosition
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }
    void Update()
    {
        if(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); //월드좌표계 != 스크린좌표계 //스크린좌표계 -> 월드좌표계로 바꿔줌

            //마우스 좌우 경계설정
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
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f); //목표지점으로 부드럽게 이동(현재위치,목표위치,정도)
        }
        
    }

    public void Drag() //캔버스 event trigger 컴포넌트에서 호출
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true; //물리현상적용 -> 자유낙하
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag != "SideWall")
        {
            StartCoroutine("AttachRoutine");
        }
    }

    IEnumerator AttachRoutine()
    {
        if(isAttach)
        {
            yield break; //코루틴을 탈출하는 키워드는 yield break
        }
        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision) //OnCollisionStay2D : 물리적 충돌 중일 때 계속 실행되는 함수
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //동글 합치기 로직
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //1.내가 아래에 있을 때
                //2.동일한 높이일 때, 내가 오른쪽에 있을 때
                if(meY < otherY || (meY == otherY && meX > otherX))
                {
                    //상대방은 숨기기
                    other.Hide(transform.position);
                    //나는 레벨업
                    LevelUp();
                }

            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;
        rigid.simulated = false;
        circle.enabled = false;

        if(targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos) //이동을 위한 코루틴 생성 //성장하는 상대에게 이동하므로 Vector3 매개변수 추가
    {
        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;
            if(targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.6f);
                yield return null;
            }
            else if(targetPos == Vector3.up * 100) //게임오버될때 호출
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            
        }

        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());//동글 성장 애니메이션을 위한 코루틴 추가
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level+1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f);

        level++; //실제 레벨 상승을 늦게 하는 이유는 애니메이션 시간 때문!

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime > 2)
            {
                spriteRenderer.color = new Color(0.7f, 0.2f, 0.2f);
            }
            if(deadTime > 5)
            {
                manager.GmaeOver();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    void EffectPlay() //파티클 위치와 크기를 보정해주는 함수 생성
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}

