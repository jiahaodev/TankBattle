/****************************************************
    文件：PanelManager.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:06
	功能：Panel统一管理
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager
{

    public enum Layer
    {
        Panel,
        Tip,
    }

    //层级列表
    private static Dictionary<Layer, Transform> layers = new Dictionary<Layer, Transform>();
    //面板列表
    public static Dictionary<string, BasePanel> panels = new Dictionary<string, BasePanel>();

    public static Transform root;
    public static Transform canvas;

    //初始化
    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        Transform panel = canvas.Find("Panel");
        Transform tip = canvas.Find("Tip");
        layers.Add(Layer.Panel, panel);
        layers.Add(Layer.Tip, tip);
    }

    //打开面板
    public static void Open<T>(params object[] para) where T : BasePanel
    {
        string name = typeof(T).ToString();
        //已经打开
        if (panels.ContainsKey(name))
        {
            return;
        }
        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.OnInit();
        panel.Init();

        Transform layer = layers[panel.layer];
        panel.skin.transform.SetParent(layer, false);

        panels.Add(name, panel);
        panel.OnShow();
    }



    //关闭面板
    public static void Close(string name) {
        if (!panels.ContainsKey(name))
        {
            return;
        }

        BasePanel panel = panels[name];
        panel.OnClose();
        panels.Remove(name);
        //销毁
        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }



}