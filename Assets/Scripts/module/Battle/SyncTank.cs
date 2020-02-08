/****************************************************
    文件：SyncTank.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:10
	功能：其他玩家的坦克（同步网络消息）
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank 
{
    private Vector3 lastPos;
    private Vector3 lastRot;
    private Vector3 forecastPos;
    private Vector3 forecastRot;
    private float forecastTime;


    public new void Init(string skinPath) {
        base.Init(skinPath);
        //不受物理影响
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        rigidbody.useGravity = false;
        //初始化预测信息
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        forecastPos = transform.position;
        forecastRot = transform.eulerAngles;
        forecastTime = Time.time;
    }

    private new void Update() {
        base.Update();
        //更新位置
        ForecastUpdate();
    }


    //移动同步
    public void SyncPos(MsgSyncTank msg) {
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex,msg.ey,msg.ez);
        //预测位置
        forecastPos = pos + 2 * (pos - lastPos);
        forecastPos = rot + 2 * (rot - lastRot);
        //更新
        lastPos = pos;
        lastRot = rot;
        forecastTime = Time.time;
        //炮塔
        Vector3 le = turret.localEulerAngles;
        le.y = msg.turretY;
        turret.localEulerAngles = le;
    }

    //更新位置
    public void ForecastUpdate() {
        float t = (Time.time - forecastTime) / CtrlTank.syncInterval;
        t = Mathf.Clamp(t,0f,1f);
        //位置
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(pos,forecastPos,t);
        transform.position = pos;
        //旋转
        Quaternion quat = transform.rotation;
        Quaternion forecastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat,forecastQuat,t);
        transform.rotation = quat;
    }


    //开火
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();
        //更新坐标
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}