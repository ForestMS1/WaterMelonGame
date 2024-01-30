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
    public bool isMerge; //��ĥ �� �ٸ� ������ �������� �ʵ��� ��ݿ��� ���ִ� ���� �߰�
    Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;

    public void Awake()
    {
        rigid = GetComponent<Rigidbody2D>(); //������Ʈ ������
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
    }

    private void OnEnable() //OnEnable : ��ũ��Ʈ�� Ȱ��ȭ �� �� ����Ǵ� �̺�Ʈ�Լ� //������Ʈ�� �����̵ǰų� Ȱ��ȭ�ɶ� �ڵ����� �����̵Ǵ� �̺�Ʈ�Լ�
    {
        anim.SetInteger("Level", level);
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

    void OnCollisionStay2D(Collision2D collision) //OnCollisionStay2D : ������ �浹 ���� �� ��� ����Ǵ� �Լ�
    {
        if(collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //���� ��ġ�� ����
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ����� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //1.���� �Ʒ��� ���� ��
                //2.������ ������ ��, ���� �����ʿ� ���� ��
                if(meY < otherY || (meY == otherY && meX > otherX))
                {
                    //������ �����
                    other.Hide(transform.position);
                    //���� ������
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

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos) //�̵��� ���� �ڷ�ƾ ���� //�����ϴ� ��뿡�� �̵��ϹǷ� Vector3 �Ű����� �߰�
    {
        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return null;
        }

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());//���� ���� �ִϸ��̼��� ���� �ڷ�ƾ �߰�
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level+1);
        EffectPlay();

        yield return new WaitForSeconds(0.3f);

        level++; //���� ���� ����� �ʰ� �ϴ� ������ �ִϸ��̼� �ð� ����!

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void EffectPlay() //��ƼŬ ��ġ�� ũ�⸦ �������ִ� �Լ� ����
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}

