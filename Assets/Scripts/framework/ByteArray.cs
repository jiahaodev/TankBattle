/****************************************************
	文件：ByteArray.cs
	作者：JiahaoWu
	邮箱: jiahaodev@163.ccom
	日期：2020/02/07 21:33   	
	功能：读写缓冲区封装类
*****************************************************/
using System;


public class ByteArray
{
    //默认大小
    const int DEFAULT_SIZE = 1024;
    //初始大小
    int initSize = 0;
    //缓冲区
    public byte[] bytes;
    //读写位置
    public int readIdx = 0;
    public int writeIdx = 0;
    //容量
    private int capacity = 0;
    //剩余空间
    public int remain { get { return capacity - writeIdx; } }
    //数据长度
    public int length { get { return writeIdx - readIdx; } }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    /// <summary>
    /// 重置缓冲区大小
    /// 扩容时使用
    /// </summary>
    /// <param name="size"></param>
    public void Resize(int size)
    {
        if (size < length) return;
        if (size < initSize) return;
        int n = 1;
        while (n < size)
            n *= 2;
        capacity = n;
        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);
        bytes = newBytes;
        writeIdx = length;
        readIdx = 0;
    }


    #region 移动缓冲数据
    /// <summary>
    /// 如果剩余空间不足，则往前移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            MoveBytes();
        }
    }

    /// <summary>
    /// 移动数据
    /// </summary>
    public void MoveBytes()
    {
        Array.Copy(bytes, readIdx, bytes, 0, length);
        writeIdx = length;
        readIdx = 0;
    }
    #endregion


    #region 读写数据
    /// <summary>
    ///写入数据
    /// </summary>
    /// <param name="bs"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int Write(byte[] bs, int offset, int count)
    {
        if (remain < count)
        {
            Resize(length + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="bs"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, length);
        Array.Copy(bytes, 0, bs, offset, count);
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    /// <summary>
    /// 读取Int16
    /// </summary>
    /// <returns></returns>
    public Int16 ReadInt16()
    {
        if (length < 2) return 0;
        Int16 ret = BitConverter.ToInt16(bytes, readIdx);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    /// <summary>
    /// 读取Int32
    /// </summary>
    /// <returns></returns>
    public Int32 ReadInt32()
    {
        if (length < 4) return 0;
        Int32 ret = BitConverter.ToInt32(bytes, readIdx);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }
    #endregion


    #region 调试使用
    //打印缓冲区
    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, length);
    }

    //打印调试信息
    public string Debug()
    {
        return string.Format("readIdx({0}), writeIdx{1}, bytes({2})", readIdx, writeIdx, BitConverter.ToString(bytes, 0, capacity));
    }
    #endregion


}