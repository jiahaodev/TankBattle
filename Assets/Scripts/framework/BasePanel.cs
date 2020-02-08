/****************************************************
    文件：BasePanel.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:05
	功能：Panel基类
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePanel : MonoBehaviour 
{
    public string skinPath;
    public GameObject skin;
    public PanelManager.Layer layer = PanelManager.Layer.Panel;

    public void Init()
    {
        GameObject skinPrefab = ResManager.LoadPrefab(skinPath);
        skin = (GameObject)Instantiate(skinPrefab);
    }

    public void Close() {
        string name = this.GetType().ToString();
        PanelManager.Close(name);
    }

    //初始化时
    public virtual void OnInit() {

    }

    //显示时
    public virtual void OnShow(params object[] para) {

    }

    //关闭时
    public virtual void OnClose() {

    }
}