/****************************************************
    文件：GameMain.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 16:59
	功能：客户端主逻辑入口
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour 
{
    public static string id = "";

    private void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick",OnMsgKick);
        //初始化
        PanelManager.Init();
        BattleManager.Init();
        //打开登陆面板
        PanelManager.Open<LoginPanel>();
    }


    private void Update()
    {
        NetManager.Update();
    }

    //关闭连接
    private void OnConnectClose(string err) {
        Debug.Log("断开连接");
    }


    //被踢下线
    void OnMsgKick(MsgBase msgBase)
    {
        PanelManager.Open<TipPanel>("被踢下线");
    }

}