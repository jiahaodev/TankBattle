/****************************************************
	文件：LoginMsg.cs
	作者：JiahaoWu
	邮箱: jiahaodev@163.ccom
	日期：2020/02/08 0:27   	
	功能：注册、登陆 相关消息
*****************************************************/

//注册
public class MsgRegister : MsgBase {
    public MsgRegister() {
        protoName = "MsgRegister";
    }
    //客户端发
    public string id = "";
    public string pw = "";
    //服务器会（0-成功，1-失败）
    public int result = 0;
}

//登陆
public class MsgLogin : MsgBase
{
    public MsgLogin() { protoName = "MsgLogin"; }
    //客户端发
    public string id = "";
    public string pw = "";
    //服务端回（0-成功，1-失败）
    public int result = 0;
}


//踢下线（服务端推送）
public class MsgKick : MsgBase
{
    public MsgKick() { protoName = "MsgKick"; }
    //原因（0-其他人登陆同一账号）
    public int reason = 0;
}