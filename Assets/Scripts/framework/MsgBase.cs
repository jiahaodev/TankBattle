/****************************************************
	文件：MsgBase.cs
	作者：JiahaoWu
	邮箱: jiahaodev@163.ccom
	日期：2020/02/07 22:37   	
	功能：消息基类
*****************************************************/
using System;
using System.Web.Script.Serialization;


public class MsgBase
{
    //协议名称
    public string protoName = "null";

    static JavaScriptSerializer Js = new JavaScriptSerializer();

    /// <summary>
    /// 编码
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = Js.Serialize(msgBase);
        return StringToBytes(s);
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName"></param>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = BytesToString(bytes, offset, count);
        MsgBase msgBase = (MsgBase)Js.Deserialize(s, Type.GetType(protoName));
        return msgBase;
    }

    /// <summary>
    /// 编码协议名（2字节长度 + 字符串）
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = StringToBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[2 + len];
        //组装两字节的长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //组装协议名
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    /// <summary>
    /// 解码协议名（2字节长度 + 字符串）
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        //必须大于2字节（记录协议名称长度信息占用了两字节）
        if (offset + 2 > bytes.Length)
        {
            return "";
        }
        //读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | (bytes[offset]));
        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }
        count = 2 + len;
        string name = BytesToString(bytes, offset + 2, len);
        return name;
    }

    //字符串转字节数组
    private static byte[] StringToBytes(string s)
    {
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    //字节数组转字符串
    private static string BytesToString(byte[] bytes, int offset, int count)
    {
        return System.Text.Encoding.UTF8.GetString(bytes, offset, count);
    }
}

