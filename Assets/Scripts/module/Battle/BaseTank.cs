/****************************************************
    文件：BaseTank.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:09
	功能：坦克基类
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTank : MonoBehaviour
{
    //坦克模型
    private GameObject skin;
    //转向速度
    public float steer = 30f;
    //移动速度
    public float speed = 6f;
    //炮塔旋转速度
    public float turretSpeed = 30f;
    //炮塔
    public Transform turret;
    //炮管
    public Transform gun;
    //发射点
    public Transform firePoint;
    //炮弹cd时间
    public float fireCd = 0.5f;
    //上一次发射炮弹的时间
    public float lastFireTime = 0;
    //物理
    protected new Rigidbody rigidbody;
    //生命值
    public float hp = 100;
    //属于哪一名玩家
    public string id = "";
    //阵营
    public int camp = 0;


    protected void Start()
    {
    }

    protected void Update()
    {
    }

    //初始化
    public void Init(string skinPath)
    {
        GameObject skinRes = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;

        rigidbody = gameObject.AddComponent<Rigidbody>();
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = new Vector3(0, 2.5f, 1.47f);
        boxCollider.size = new Vector3(7, 5, 12);

        turret = skin.transform.Find("Turret");
        gun = skin.transform.Find("Gun");
        firePoint = skin.transform.Find("FirePoint");
    }


    //发射炮弹
    public Bullet Fire() {
        if (IsDie())
        {
            return null;
        }
        //产生炮弹
        GameObject bulletObj = new GameObject("bullet");
        Bullet bullet = bulletObj.AddComponent<Bullet>();
        bullet.Init();
        bullet.tank = this;

        //位置
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        //更新时间
        lastFireTime = Time.time;
        return bullet;
    }



    //被攻击
    public void Attacked(float att) {
        if (IsDie())
        {
            return;
        }
        
        hp -= att;
        //如果死亡，则显示焚烧效果
        if (IsDie())
        {
            GameObject obj = ResManager.LoadPrefab("explosion");
            GameObject explosion = Instantiate(obj, transform.position, transform.rotation);
            explosion.transform.SetParent(transform);
        }
    }



    //是否死亡
    public bool IsDie()
    {
        return hp <= 0;
    }


}