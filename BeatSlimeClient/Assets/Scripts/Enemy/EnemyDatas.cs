using System.Collections;
using System.Collections.Generic;

//�̵�
public class EMoving
{
    //����
    public Beat when;
    //�ڷ���Ʈ�ΰ� �������� �̵��ΰ� blah...
    public int which;
    //����
    public HexCoordinates where;
}

//���Ǳ�
public class EAttack
{
    //����
    public Beat when;
    //� ��ų��
    public int which;
    //���ٰ�
    public HexCoordinates where;
    //��� ��������
    public HexDirection direction;
}

//����ȭ��
public class ENote
{
    //����
    public Beat when;
    //� ��ų��
    public int which;
    //��������
    public int who;
}