/****************************************************
	文件：SysMsg.cs
	作者：JiahaoWu
	邮箱: jiahaodev@163.ccom
	日期：2020/02/07 23:57   	
	功能：系统消息
*****************************************************/


public class MsgPing : MsgBase
{
    public MsgPing()
    {
        protoName = "MsgPing";
    }
}


public class MsgPong : MsgBase
{
    public MsgPong()
    {
        protoName = "MsgPong";
    }
}