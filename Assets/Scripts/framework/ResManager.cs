/****************************************************
    文件：ResManager.cs
	作者：JiahaoWu
    邮箱: jiahaodev@163.com
    日期：2020/02/08 17:06
	功能：资源管理者
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResManager : MonoBehaviour 
{

    //加载预设
    public static GameObject LoadPrefab(string path) {
        return Resources.Load<GameObject>(path);
    }

}