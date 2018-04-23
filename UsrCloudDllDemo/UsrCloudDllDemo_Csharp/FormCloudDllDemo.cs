using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UsrCloudDllDemo_Csharp
{
    using System.Runtime.InteropServices;

    public partial class FormCloudDllDemo : Form
    {
        TUSR_ConnAckEvent FConnAck_CBF;
        TUSR_SubscribeAckEvent FSubscribeAck_CBF;
        TUSR_UnSubscribeAckEvent FUnSubscribeAck_CBF;
        TUSR_PubAckEvent FPubAck_CBF;
        TUSR_RcvParsedEvent FRcvParsedDataPointPush_CBF;
        TUSR_RcvParsedEvent FRcvParsedDevStatusPush_CBF;
        TUSR_RcvParsedEvent FRcvParsedDevAlarmPush_CBF;
        TUSR_RcvParsedEvent FRcvParsedOptionResponseReturn_CBF;
        TUSR_RcvRawFromDevEvent FRcvRawFromDev_CBF;

        public FormCloudDllDemo()
        {
            InitializeComponent();
            FConnAck_CBF = new TUSR_ConnAckEvent(ConnAck_CBF);
            FSubscribeAck_CBF = new TUSR_SubscribeAckEvent(SubscribeAck_CBF);
            FUnSubscribeAck_CBF = new TUSR_UnSubscribeAckEvent(UnSubscribeAck_CBF);
            FPubAck_CBF = new TUSR_PubAckEvent(PubAck_CBF);
            FRcvParsedDataPointPush_CBF = new TUSR_RcvParsedEvent(RcvParsedDataPointPush_CBF);
            FRcvParsedDevStatusPush_CBF = new TUSR_RcvParsedEvent(RcvParsedDevStatusPush_CBF);
            FRcvParsedDevAlarmPush_CBF = new TUSR_RcvParsedEvent(RcvParsedDevAlarmPush_CBF);
            FRcvParsedOptionResponseReturn_CBF = new TUSR_RcvParsedEvent(RcvParsedOptionResponseReturn_CBF);
            FRcvRawFromDev_CBF = new TUSR_RcvRawFromDevEvent(RcvRawFromDev_CBF);
        }

        //初始化   
        private void buttonInit_Click(object sender, EventArgs e)
        {
            string ip = "clouddata.usr.cn";//透传云服务器地址, 打死都不改
            ushort port = 1883;//透传云服务器端口, 打死都不改
            int vertion = 1;
            if (USR_Init(ip, port, vertion))
            {
                Log("初始化成功", true);
                USR_OnConnAck(FConnAck_CBF);
                USR_OnSubscribeAck(FSubscribeAck_CBF);
                USR_OnUnSubscribeAck(FUnSubscribeAck_CBF);
                USR_OnPubAck(FPubAck_CBF);
                USR_OnRcvParsedDataPointPush(FRcvParsedDataPointPush_CBF);
                USR_OnRcvParsedDevStatusPush(FRcvParsedDevStatusPush_CBF);
                USR_OnRcvParsedDevAlarmPush(FRcvParsedDevAlarmPush_CBF);
                USR_OnRcvParsedOptionResponseReturn(FRcvParsedOptionResponseReturn_CBF);
                USR_OnRcvRawFromDev(FRcvRawFromDev_CBF);
            }
            else
            {
                Log("初始化失败", true);
            }
        }

        //释放
        private void buttonRelease_Click(object sender, EventArgs e)
        {
            if (USR_Release())
            {
                Log("释放成功", true);
            }
            else
            {
                Log("释放失败", true);
            }
        }

        //查询版本
        private void button4_Click(object sender, EventArgs e)
        {
            Log("dll版本号: " + USR_GetVer().ToString(), true);
        }

        //连接
        private void button5_Click(object sender, EventArgs e)
        {
            string userName = textBox3.Text;
            string passWord = textBox4.Text;
            if (USR_Connect(userName, passWord))
            {
                Log("连接已发起", true);
            }
        }

        //断开
        private void button6_Click(object sender, EventArgs e)
        {
            if (USR_DisConnect())
            {
                Log("已断开", true);
            }
        }

        //订阅设备原始数据流
        private void button7_Click(object sender, EventArgs e)
        {
            string devId = textBox5.Text;
            int messageId = USR_SubscribeDevRaw(devId);
            if (messageId > -1)
            {
                Log("订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        //取消订阅
        private void button8_Click(object sender, EventArgs e)
        {
            string devId = textBox5.Text;
            int messageId = USR_UnSubscribeDevRaw(devId);
            if (messageId > -1)
            {
                Log("取消订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        //推送
        private void button10_Click(object sender, EventArgs e)
        {
            string devId = textBox6.Text;
            byte[] byteArray;
            if (checkBox_Hex.Checked == true)
            {
                byteArray = HexStringToByteArray(textBox7.Text);
            }
            else
            {
                byteArray = System.Text.Encoding.Default.GetBytes(textBox7.Text);
            }

            int messageId = USR_PublishRawToDev(devId, byteArray, byteArray.Length);
            if (messageId > -1)
            {
                Log("消息已推送 MsgId:" + messageId.ToString(), true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBoxLog.Clear();
        }

        private void Log(string str, Boolean bInsTime = false)
        {
            if (bInsTime)
            {
                richTextBoxLog.AppendText("\n------" + DateTime.Now.ToLongTimeString().ToString() + "------\n");
            }
            richTextBoxLog.AppendText(str + "\n");
            if (checkBox1.Checked)
            {
                richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length;
                richTextBoxLog.ScrollToCaret();
            }
        }

        private void ConnAck_CBF(int returnCode, IntPtr description)
        {
            string sLog =
                 "【连接事件】" + "\n" +
                 "returnCode: " + returnCode.ToString() + "  " + Marshal.PtrToStringAuto(description);
            Log(sLog);
            if (returnCode == 0)
            {
                Log("连接成功");
            }
            else
            {
                Log("连接失败");
            }

        }


        /* 自定义回调函数,用于判断订阅结果 */
        private void SubscribeAck_CBF(int messageID, IntPtr SubFunName, IntPtr SubParam, IntPtr returnCode)
        {
            string sSubFunName = Marshal.PtrToStringAuto(SubFunName);
            string sSubParam = Marshal.PtrToStringAuto(SubParam);
            string sReturnCode = Marshal.PtrToStringAuto(returnCode);
            string[] SubParamArray = sSubParam.Split(',');
            string[] retCodeArray = sReturnCode.Split(',');
            int len = SubParamArray.Length;
            string sLog =
               "【订阅事件】" + "\n" +
               "MsgId:" + messageID.ToString() + "\n" +
               "函数名称:" + sSubFunName + "\n";
            for (int i = 0; i < len; ++i)
            {
                sLog += "设备ID(或用户名)：" + SubParamArray[i] + "\n" +
                 "  订阅结果：" + retCodeArray[i] + "\n";
            }
            Log(sLog, true);
        }

        /* 自定义回调函数,用于判断取消订阅结果 */
        private void UnSubscribeAck_CBF(int messageID, IntPtr UnSubFunName, IntPtr UnSubParam)
        {
            string sUnSubFunName = Marshal.PtrToStringAuto(UnSubFunName);
            string sUnSubParam = Marshal.PtrToStringAuto(UnSubParam);
            string sLog =
               "【取消订阅事件】" + "\n" +
               "MsgId:" + messageID.ToString() + "\n" +
               "函数名称：" + sUnSubFunName + "\n" +
               "设备ID或用户名：" + sUnSubParam;
            Log(sLog, true);
        }


        protected void PubAck_CBF(int messageID)
        { 
            string sLog =
                "【推送回调事件】\n" +
                "MsgId    : " + messageID.ToString();
            Log(sLog, true);
        }

        /* 自定义回调函数,用于接收数据点值推送 */
        private void RcvParsedDataPointPush_CBF(
            int messageID, IntPtr DevId, IntPtr JsonStr)
        {
            string sDevId = Marshal.PtrToStringAuto(DevId);
            string sJsonStr = Marshal.PtrToStringAuto(JsonStr);
            string sLog =
                "【数据点值推送事件】\n" +
                "设备ID   : " + sDevId + "\n" +
                "MsgId    : " + messageID.ToString() + "\n" +
                "JSON数据: " + sJsonStr;
            Log(sLog, true);
            //todo json解析
        }

        /* 自定义回调函数,用于接收上下线推送 */
        private void RcvParsedDevStatusPush_CBF(
            int messageID, IntPtr DevId, IntPtr JsonStr)
        {
            string sDevId = Marshal.PtrToStringAuto(DevId);
            string sJsonStr = Marshal.PtrToStringAuto(JsonStr);
            string sLog =
                "【设备上下线推送事件】\n" +
                "设备ID   : " + sDevId + "\n" +
                "MsgId    : " + messageID.ToString() + "\n" +
                "JSON数据: " + sJsonStr;
            Log(sLog, true);
        }

        /* 自定义回调函数,用于接收报警推送 */
        private void RcvParsedDevAlarmPush_CBF(
            int messageID, IntPtr DevId, IntPtr JsonStr)
        {
            string sDevId = Marshal.PtrToStringAuto(DevId);
            string sJsonStr = Marshal.PtrToStringAuto(JsonStr);
            string sLog =
                "【设备报警推送事件】\n" +
                "设备ID   : " + sDevId + "\n" +
                "MsgId    : " + messageID.ToString() + "\n" +
                "JSON数据: " + sJsonStr;
            Log(sLog, true);
        }

        /* 自定义回调函数,用于接收数据点操作应答 */
        private void RcvParsedOptionResponseReturn_CBF(
            int messageID, IntPtr DevId, IntPtr JsonStr)
        {
            string sDevId = Marshal.PtrToStringAuto(DevId);
            string sJsonStr = Marshal.PtrToStringAuto(JsonStr);
            string sLog =
                "【数据点操作应答事件】\n" +
                "设备ID   : " + sDevId + "\n" +
                "MsgId    : " + messageID.ToString() + "\n" +
                "JSON数据: " + sJsonStr;
            Log(sLog,true);
        }

        /* 自定义回调函数,用于接收设备原始数据流 */
        private void RcvRawFromDev_CBF(
            int messageID, IntPtr devId, IntPtr pData, int DataLen)
        {
            string sDevId = Marshal.PtrToStringAuto(devId);
            byte[] byteArr = new byte[DataLen];
            Marshal.Copy(pData, byteArr, 0, DataLen);
            string sHex = BitConverter.ToString(byteArr).Replace(
                "-", " ");
            string sLog =
                "【接收数据流事件】\n" +
                "设备ID   : " + sDevId + "\n" +
                "MsgId    : " + messageID.ToString() + "\n" +
                "接收数据(Hex): " + sHex;
            Log(sLog,true);
        }


        ///////////////////////////////
        ///////  初始化和释放  ////////
        ///////////////////////////////

        /// <summary>
        /// 获取版本
        /// </summary>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_GetVer", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_GetVer();
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="vertion"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Init", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Init(string host, ushort port, int vertion);
        /// <summary>
        /// 释放
        /// </summary>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Release", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Release();

        ///////////////////////////////
        ////////  连接和断开  /////////
        ///////////////////////////////

        /// <summary>
        /// 连接回调
        /// </summary>
        /// <param name="returnCode"></param>
        /// <param name="description"></param>
        public delegate void TUSR_ConnAckEvent(int returnCode, IntPtr description);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnConnAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnConnAck(TUSR_ConnAckEvent OnConnAck);
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Connect", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Connect(string username, string password);
        /// <summary>
        /// 断开
        /// </summary>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_DisConnect", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_DisConnect();

        ///////////////////////////////
        //////  订阅和取消订阅  ///////
        ///////////////////////////////

        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="messageID">消息ID</param>
        /// <param name="SubFunName">函数名称,用于判断用户执行的哪个订阅函数, 得到了服务器的回应。可能的取值有:SubscribeDevParsed,SubscribeUserParsed,SubscribeDevRaw,SubscribeUserRaw</param>
        /// <param name="SubParam">SubParam值跟执行的订阅函数有关:如果订阅的是”单个设备的消息”, 则SubParam为设备ID;如果订阅的是”用户所有设备的消息”, 则SubParam为用户名</param>
        /// <param name="returnCode">0、1、2:订阅成功;  128:订阅失败</param>
        public delegate void TUSR_SubscribeAckEvent(int messageID, IntPtr SubFunName, IntPtr SubParam, IntPtr returnCode);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnSubscribeAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnSubscribeAck(TUSR_SubscribeAckEvent OnSubscribeAck);
        /// <summary>
        /// 取消订阅回调
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="UnSubFunName"></param>
        /// <param name="UnSubParam"></param>
        public delegate void TUSR_UnSubscribeAckEvent(int messageID, IntPtr UnSubFunName, IntPtr UnSubParam);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnUnSubscribeAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnUnSubscribeAck(TUSR_UnSubscribeAckEvent OnUnSubscribeAck);

        /// <summary>
        /// 订阅单个设备解析后的数据  【云组态】
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_SubscribeDevParsed", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_SubscribeDevParsed(string devId);
        /// <summary>
        /// 订阅账户下所有设备解析后的数据  【云组态】
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_SubscribeUserParsed", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_SubscribeUserParsed(string Username);
        /// <summary>
        /// 取消订阅单个设备解析后的数据  【云组态】
        /// </summary>
        /// <param name="DevId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribeDevParsed", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribeDevParsed(string DevId);
        /// <summary>
        /// 取消订阅账户下所有设备解析后的数据  【云组态】
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribeUserParsed", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribeUserParsed(string Username);

        /// <summary>
        /// 订阅单个设备原始数据流 【云交换机】
        /// </summary>
        /// <param name="devId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_SubscribeDevRaw", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_SubscribeDevRaw(string devId);
        /// <summary>
        /// 订阅账户下所有设备原始数据流 【云交换机】
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_SubscribeUserRaw", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_SubscribeUserRaw(string Username);
        /// <summary>
        /// 取消订阅单个设备原始数据流 【云交换机】
        /// </summary>
        /// <param name="DevId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribeDevRaw", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribeDevRaw(string DevId);
        /// <summary>
        /// 取消订阅账户下所有设备原始数据流 【云交换机】
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribeUserRaw", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribeUserRaw(string Username);

        //订阅回调 【不再推荐使用】
        public delegate void TUSR_SubAckEvent(int messageID, IntPtr devId, IntPtr returnCode);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnSubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnSubAck(TUSR_SubAckEvent OnSubAck);
        //订阅 【不再推荐使用】
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Subscribe", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_Subscribe(string devId);
        //取消订阅回调 【不再推荐使用】
        public delegate void TUSR_UnSubAckEvent(int messageID, IntPtr devId);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnUnSubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnUnSubAck(TUSR_UnSubAckEvent OnUnSubAck);
        //取消订阅 【不再推荐使用】
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribe", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribe(string devId);

        ///////////////////////////////
        /////////  推送消息 ///////////
        ///////////////////////////////
        /// <summary>
        /// 推送回调
        /// </summary>
        /// <param name="MessageID"></param>
        public delegate void TUSR_PubAckEvent(int MessageID);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnPubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnPubAck(TUSR_PubAckEvent OnPubAck);

        /// <summary>
        /// 设置数据点值【云组态】
        /// </summary>
        /// <param name="DevId"></param>
        /// <param name="SlaveIndex"></param>
        /// <param name="PointId"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishParsedSetSlaveDataPoint", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishParsedSetSlaveDataPoint(string DevId, string SlaveIndex, string PointId, string Value);
        /// <summary>
        /// 查询数据点值【云组态】
        /// </summary>
        /// <param name="DevId"></param>
        /// <param name="SlaveIndex"></param>
        /// <param name="PointId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishParsedQuerySlaveDataPoint", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishParsedQuerySlaveDataPoint(string DevId, string SlaveIndex, string PointId);

        /// <summary>
        /// 设置单台设备数据点值【云组态】 ---- 已弃, 用 USR_PublishParsedQuerySlaveDataPoint 代替
        /// </summary>
        /// <param name="DevId"></param>
        /// <param name="PointId"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishParsedSetDataPoint", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishParsedSetDataPoint(string DevId, string PointId, string Value);
        /// <summary>
        /// 查询单台设备数据点值【云组态】 ---- 已弃, 用 USR_PublishParsedQuerySlaveDataPoint 代替
        /// </summary>
        /// <param name="DevId"></param>
        /// <param name="PointId"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishParsedQueryDataPoint", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishParsedQueryDataPoint(string DevId, string PointId);

        /// <summary>
        /// 向单台设备推送原始数据流 【云交换机】
        /// </summary>
        /// <param name="DevId"></param>
        /// <param name="pData"></param>
        /// <param name="DataLen"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishRawToDev", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishRawToDev(string DevId, byte[] pData, int DataLen);

        /// <summary>
        /// 向账户下所有设备推送原始数据流 【云交换机】
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="pData"></param>
        /// <param name="DataLen"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_PublishRawToUser", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_PublishRawToUser(string Username, byte[] pData, int DataLen);

        //推送【不再推荐使用】
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Publish", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_Publish(string DevId, byte[] pData, int DataLen);

        ///////////////////////////////
        /////////  接收消息 ///////////
        ///////////////////////////////
        /// <summary>
        /// 接收设备解析后的数据 事件定义 【云组态】
        /// </summary>
        /// <param name="MessageID"></param>
        /// <param name="DevId"></param>
        /// <param name="JsonStr"></param>
        public delegate void TUSR_RcvParsedEvent(int MessageID, IntPtr DevId, IntPtr JsonStr);
        /// <summary>
        /// 设置 接收数据点推送 回调函数 【云组态】
        /// </summary>
        /// <param name="OnRcvParsed"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcvParsedDataPointPush", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnRcvParsedDataPointPush(TUSR_RcvParsedEvent OnRcvParsed);
        /// <summary>
        /// 设置 接收设备上下线推送 回调函数 【云组态】
        /// </summary>
        /// <param name="OnRcvParsed"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcvParsedDevStatusPush", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnRcvParsedDevStatusPush(TUSR_RcvParsedEvent OnRcvParsed);
        /// <summary>
        /// 设置 接收设备报警推送 回调函数 【云组态】
        /// </summary>
        /// <param name="OnRcvParsed"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcvParsedDevAlarmPush", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnRcvParsedDevAlarmPush(TUSR_RcvParsedEvent OnRcvParsed);
        /// <summary>
        /// 设置 接收数据点操作应答 【云组态】
        /// </summary>
        /// <param name="OnRcvParsed"></param>
        /// <returns></returns>
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcvParsedOptionResponseReturn", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnRcvParsedOptionResponseReturn(TUSR_RcvParsedEvent OnRcvParsed);

        /// <summary>
        /// 接收设备原始数据流 事件定义 【云交换机】 
        /// </summary>
        /// <param name="MessageID"></param>
        /// <param name="DevId"></param>
        /// <param name="pData"></param>
        /// <param name="DataLen"></param>
        public delegate void TUSR_RcvRawFromDevEvent(int MessageID, IntPtr DevId, IntPtr pData, int DataLen);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcvRawFromDev", CallingConvention = CallingConvention.StdCall)]
        /// <summary>
        /// 设置 接收设备原始数据流 回调函数 【云交换机】
        /// </summary>
        /// <param name="OnRcvRawFromDev"></param>
        /// <returns></returns>
        public static extern bool USR_OnRcvRawFromDev(TUSR_RcvRawFromDevEvent OnRcvRawFromDev);

        //接收回调 【不再推荐使用】
        public delegate void TUSR_OnRcvEvent(int MessageID, IntPtr DevId, IntPtr pData, int DataLen);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnRcv", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnRcv(TUSR_OnRcvEvent OnRcvEvent);

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://cloud.usr.cn/development_instruction.html");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://cloud.usr.cn/sdk/dll/");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://console.usr.cn/");
        }

        private void richTextBoxLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string userrname = textBox8.Text;
            int messageId = USR_SubscribeUserRaw(userrname);
            if (messageId > -1)
            {
                Log("订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string userrname = textBox8.Text;
            int messageId = USR_UnSubscribeUserRaw(userrname);
            if (messageId > -1)
            {
                Log("取消订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string username = textBox9.Text;
            byte[] byteArray;
            if (checkBox_Hex.Checked == true)
            {
                byteArray = HexStringToByteArray(textBox7.Text);
            }
            else
            {
                byteArray = System.Text.Encoding.Default.GetBytes(textBox7.Text);
            }

            int messageId = USR_PublishRawToUser(username, byteArray, byteArray.Length);
            if (messageId > -1)
            {
                Log("消息已推送 MsgId:" + messageId.ToString(), true);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string devId = textBox11.Text;
            int messageId = USR_SubscribeDevParsed(devId);
            if (messageId > -1)
            {
                Log("订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string devId = textBox11.Text;
            int messageId = USR_UnSubscribeDevParsed(devId);
            if (messageId > -1)
            {
                Log("取消订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string userrname = textBox10.Text;
            int messageId = USR_SubscribeUserParsed(userrname);
            if (messageId > -1)
            {
                Log("订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string userrname = textBox10.Text;
            int messageId = USR_UnSubscribeUserParsed(userrname);
            if (messageId > -1)
            {
                Log("取消订阅已发起  MsgId:" + messageId.ToString(), true);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string devId = textBox14.Text;
            string slaveIndex = textBox16.Text;
            string pointId = textBox13.Text;
            int iMsgId = USR_PublishParsedQuerySlaveDataPoint(
                devId, slaveIndex, pointId);
            if (iMsgId > -1)
            {
                Log("消息已推送 MsgId:" + iMsgId.ToString(), true);
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string devId = textBox14.Text;
            string slaveIndex = textBox16.Text;
            string pointId = textBox13.Text;
            string value = textBox12.Text;
            int iMsgId = USR_PublishParsedSetSlaveDataPoint(
                devId, slaveIndex, pointId, value);
            if (iMsgId > -1)
            {
                Log("消息已推送 MsgId:" + iMsgId.ToString(), true);
            }
        }
    }
}
