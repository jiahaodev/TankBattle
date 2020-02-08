/****************************************************
    文件：RoomPanel.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:09
	功能：Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    //开战按钮
    private Button startButton;
    //退出按钮
    private Button closeButton;
    //列表容器
    private Transform content;
    //玩家信息物体
    private GameObject playerObj;

    //初始化
    public override void OnInit()
    {
        skinPath = "RoomPanel";
        layer = PanelManager.Layer.Panel;
    }

    //显示
    public override void OnShow(params object[] args)
    {
        //寻找组件
        startButton = skin.transform.Find("CtrlPanel/StartButton").GetComponent<Button>();
        closeButton = skin.transform.Find("CtrlPanel/CloseButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        playerObj = skin.transform.Find("Player").gameObject;
        //不激活玩家信息
        playerObj.SetActive(false);
        //按钮事件
        startButton.onClick.AddListener(OnStartClick);
        closeButton.onClick.AddListener(OnCloseClick);
        //协议监听
        NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);
        //发送查询
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        NetManager.Send(msg);
    }

    //关闭
    public override void OnClose()
    {
        //协议监听
        NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMsgListener("MsgStartBattle", OnMsgStartBattle);
    }

    //收到玩家列表协议
    public void OnMsgGetRoomInfo(MsgBase msgBase)
    {
        MsgGetRoomInfo msg = (MsgGetRoomInfo)msgBase;
        //清除玩家列表
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        //重新生成列表
        if (msg.players == null)
        {
            return;
        }
        for (int i = 0; i < msg.players.Length; i++)
        {
            GeneratePlayerInfo(msg.players[i]);
        }
    }

    //创建一个玩家信息单元
    public void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        //创建物体
        GameObject o = Instantiate(playerObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;
        //获取组件
        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text campText = trans.Find("CampText").GetComponent<Text>();
        Text scoreText = trans.Find("ScoreText").GetComponent<Text>();
        //填充信息
        idText.text = playerInfo.id;
        if (playerInfo.camp == 1)
        {
            campText.text = "红";
        }
        else
        {
            campText.text = "蓝";
        }
        if (playerInfo.isOwner == 1)
        {
            campText.text = campText.text + " ！";
        }
        scoreText.text = playerInfo.win + "胜 " + playerInfo.lost + "负";
    }

    //点击退出按钮
    public void OnCloseClick()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    //收到退出房间协议
    public void OnMsgLeaveRoom(MsgBase msgBase)
    {
        MsgLeaveRoom msg = (MsgLeaveRoom)msgBase;
        //成功退出房间
        if (msg.result == 0)
        {
            PanelManager.Open<TipPanel>("退出房间");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        //退出房间失败
        else
        {
            PanelManager.Open<TipPanel>("退出房间失败");
        }
    }

    //点击开战按钮
    public void OnStartClick()
    {
        MsgStartBattle msg = new MsgStartBattle();
        NetManager.Send(msg);
    }

    //收到开战返回
    public void OnMsgStartBattle(MsgBase msgBase)
    {
        MsgStartBattle msg = (MsgStartBattle)msgBase;
        //开战
        if (msg.result == 0)
        {
            //关闭界面
            Close();
        }
        //开战失败
        else
        {
            PanelManager.Open<TipPanel>("开战失败！两队至少都需要一名玩家，只有队长可以开始战斗！");
        }
    }

}