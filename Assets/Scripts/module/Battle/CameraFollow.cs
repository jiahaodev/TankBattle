/****************************************************
    文件：CameraFollow.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:11
	功能：相机跟随
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Camera camera;
    //距离
    public Vector3 distance = new Vector3(0, 8, -18);
    //偏移值
    public Vector3 offset = new Vector3(0, 5f, 0);
    //相机速度
    public float speed = 6f;

    private void Start()
    {
        camera = Camera.main;
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 initPos = pos - 30 * forward + Vector3.up * 10;
    }

    //所有组件update之后调用
    private void LateUpdate()
    {
        //坦克位置
        Vector3 pos = transform.position;
        //坦克方向
        Vector3 forward = transform.forward;
        //相机目标位置
        Vector3 targetPos = pos;
        targetPos += forward * distance.z;
        targetPos.y += distance.y;
        //相机位置
        Vector3 cameraPos = camera.transform.position;
        cameraPos = Vector3.MoveTowards(cameraPos,targetPos,Time.deltaTime*speed);
        camera.transform.position = cameraPos;
        //对准坦克
        camera.transform.LookAt(pos + offset);
    }

}