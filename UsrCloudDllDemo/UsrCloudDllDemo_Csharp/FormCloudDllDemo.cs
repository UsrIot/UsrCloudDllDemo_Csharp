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
        TUSR_SubAckEvent FSubAck_CBF;
        TUSR_UnSubAckEvent FUnSubAck_CBF;
        TUSR_PubAckEvent FPubAck_CBF;
        TUSR_OnRcvEvent FRcv_CBF;
        public FormCloudDllDemo()
        {
            InitializeComponent();
            FConnAck_CBF = new TUSR_ConnAckEvent(ConnAck_CBF);
            FSubAck_CBF = new TUSR_SubAckEvent(SubAck_CBF);
            FUnSubAck_CBF = new TUSR_UnSubAckEvent(UnSubAck_CBF);
            FPubAck_CBF = new TUSR_PubAckEvent(PubAck_CBF);
            FRcv_CBF = new TUSR_OnRcvEvent(Rcv_CBF);
        }

        //初始化   
        private void buttonInit_Click(object sender, EventArgs e)
        {
            string ip = textBox1.Text;
            ushort port = Convert.ToUInt16(textBox2.Text);
            int vertion = 1;
            if (USR_Init(ip, port, vertion))
            {
                Log("初始化成功", true);
                USR_OnConnAck(FConnAck_CBF);
                USR_OnSubAck(FSubAck_CBF);
                USR_OnUnSubAck(FUnSubAck_CBF);
                USR_OnPubAck(FPubAck_CBF);
                USR_OnRcv(FRcv_CBF);
            }
            else
            {
                Log("初始化失败", true);
            }
        }

        //释放
        private void buttonRelease_Click(object sender, EventArgs e)
        {
            if(USR_Release())
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
       
        //订阅
        private void button7_Click(object sender, EventArgs e)
        {
            string devId = textBox5.Text;
            int messageId = USR_Subscribe(devId);
            if(messageId > -1)
            {
                Log("订阅已发起", true);  
            }       
        }

        //取消订阅
        private void button8_Click(object sender, EventArgs e)
        {
            string devId = textBox5.Text;
            int messageId = USR_UnSubscribe(devId);
            if (messageId > -1)
            {
                Log("取消订阅已发起", true);
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
            if (checkBox_Hex.Checked== true)
            {
                byteArray = HexStringToByteArray(textBox7.Text);
            }
            else
            {
                byteArray = System.Text.Encoding.Default.GetBytes(textBox7.Text);
            }
             
            int messageId = USR_Publish(devId, byteArray, byteArray.Length);
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
            richTextBoxLog.AppendText(str+ "\n");
            if (checkBox1.Checked)
            {
                richTextBoxLog.SelectionStart = richTextBoxLog.Text.Length; 
                richTextBoxLog.ScrollToCaret();  
            }
        } 
 
        private void ConnAck_CBF(int returnCode, IntPtr description)
        {
            Log("【连接回调】",true);
            Log("returnCode: " + returnCode.ToString() + "  " + Marshal.PtrToStringAuto(description));
            if (returnCode==0)
            {
                Log("连接成功");
            }
            else
            {
                Log("连接失败");
            }

        }

        private void SubAck_CBF(int messageID, IntPtr devId, IntPtr returnCode)
        {
            string sDevId= Marshal.PtrToStringAuto(devId);
            string sReturnCode = Marshal.PtrToStringAuto(returnCode); 
            string[] devIdArray = sDevId.Split(',');
            string[] retCodeArray = sReturnCode.Split(',');
            int len = devIdArray.Length;

            Log("【订阅回调】", true);
            Log("MsgId:" + messageID.ToString());
            for (int i = 0; i < len; ++i)
            {
                Log("设备：" + devIdArray[i] + "  订阅结果：" + retCodeArray[i]);
            }
        }

        private void UnSubAck_CBF(int messageID, IntPtr devId)
        {
            string sDevId = Marshal.PtrToStringAuto(devId);
            Log("【取消订阅回调】", true);
            Log("MsgId:" + messageID.ToString());
            Log("设备：" + sDevId);          
        }

        protected void PubAck_CBF(int messageID)
        {
            Log("【推送回调】", true);
            Log("MsgId:" + messageID.ToString());
        }

        private void Rcv_CBF(int messageID, IntPtr devId, IntPtr pData, int DataLen)
        {
            string sDevId = Marshal.PtrToStringAuto(devId);
            byte[] byteArr = new byte[DataLen];
            Marshal.Copy(pData, byteArr, 0, DataLen);
            string sHex = BitConverter.ToString(byteArr).Replace("-", " ");
            Log("【接收回调】", true);
            Log("设备ID   : "+ sDevId);
            Log("MsgId    : " + messageID.ToString());
            Log("接收数据(Hex): " + sHex);
        }
  
        //获取版本
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_GetVer", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_GetVer();
        //初始化
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Init", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Init(string host, ushort port, int vertion);
        //释放
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Release", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Release();
        //连接回调
        public delegate void TUSR_ConnAckEvent(int returnCode, IntPtr description);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnConnAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnConnAck(TUSR_ConnAckEvent OnConnAck);
        //连接
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Connect", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_Connect(string username, string password);
        //断开
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_DisConnect", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_DisConnect();
        //订阅回调
        public delegate void TUSR_SubAckEvent(int messageID, IntPtr devId, IntPtr returnCode);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnSubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnSubAck(TUSR_SubAckEvent OnSubAck);
        //订阅
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Subscribe", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_Subscribe(string devId);
        //取消订阅回调
        public delegate void TUSR_UnSubAckEvent(int messageID, IntPtr devId);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnUnSubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnUnSubAck(TUSR_UnSubAckEvent OnUnSubAck);
        //取消订阅
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_UnSubscribe", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_UnSubscribe(string devId);
        //推送回调
        public delegate void TUSR_PubAckEvent(int MessageID);
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_OnPubAck", CallingConvention = CallingConvention.StdCall)]
        public static extern bool USR_OnPubAck(TUSR_PubAckEvent OnPubAck);
        //推送
        [DllImport("UsrCloud.dll", CharSet = CharSet.Auto, EntryPoint = "USR_Publish", CallingConvention = CallingConvention.StdCall)]
        public static extern int USR_Publish(string DevId, byte[] pData, int DataLen);
        //接收回调
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

        private void richTextBoxLog_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }


}
