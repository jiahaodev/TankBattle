/****************************************************
    文件：Bullet.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:10
	功能：炮弹
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour 
{
    //移动速度
    public float speed = 120f;
    //发射者
    public BaseTank tank;
    //炮弹模型
    private GameObject skin;
    //物理
    Rigidbody rigidBody;

    //初始化
    public void Init()
    {
        //皮肤
        GameObject skinRes = ResManager.LoadPrefab("bulletPrefab");
        skin = (GameObject)Instantiate(skinRes);
        skin.transform.parent = this.transform;
        skin.transform.localPosition = Vector3.zero;
        skin.transform.localEulerAngles = Vector3.zero;
        //物理
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
    }

    private void Update()
    {
        //向前运动
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    //碰撞回调
    private void OnCollisionEnter(Collision collision)
    {
        GameObject collObj = collision.gameObject;
        BaseTank hitTank = collObj.GetComponent<BaseTank>();
        //不能打自己
        if (hitTank == tank)
        {
            return; 
        }
        if (hitTank != null)
        {
            SendMsgHit(tank,hitTank);
        }
        //显示爆炸效果
        GameObject explode = ResManager.LoadPrefab("fire");
        Instantiate(explode, transform.position, transform.rotation);
        //摧毁自身
        Destroy(gameObject);
    }

    //发送伤害协议
    private void SendMsgHit(BaseTank tank, BaseTank hitTank)
    {
        if (hitTank == null || tank == null)
        {
            return;
        }
        //不是自己发出的炮弹
        //避免不同客户端，发送同一内容的消息
        if (tank.id != GameMain.id)
        {
            return;
        }
        MsgHit msg = new MsgHit();
        msg.targetId = hitTank.id;
        msg.id = tank.id;
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        NetManager.Send(msg);
    }
}