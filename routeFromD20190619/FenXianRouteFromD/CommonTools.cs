using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace FenXianRouteFromD
{
    public class CommonTools
    {

        /// <summary>
        ///  对输入字符串进行DES加密。
        /// </summary>
        /// <param name="strToEncrypt">要加密的字符串。</param>
        /// <param name="encryptKey">密钥，且必须为8位。(如：bocomkey)</param>
        /// <returns>以Base64格式返回的加密字符串。</returns>
        public static string BocomEncrypt(string strToEncrypt, string encryptKey)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(strToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(encryptKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(encryptKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();

                return str;
            }
        }
        //DataTable 转换为List
        public static IList DtConvert2List(DataTable dt)
        {
            IList list = new ArrayList(dt.Rows.Count);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                list.Add(dt.Rows[i]);
            }
            if (list.Count > 0)
            {
                return list;
            }
            else
            {
                return null;
            }
        }
        //DataTable 转换为List
        public static IList<T> DtConvert2ListT<T>(DataTable dt)
        {
            IList<T> list = new List<T>();
            T t = default(T);
            PropertyInfo[] properTypes = null;
            string tempName = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                t = Activator.CreateInstance<T>();
                properTypes = t.GetType().GetProperties();
                foreach (PropertyInfo pro in properTypes)
                {
                    tempName = pro.Name;
                    if (dt.Columns.Contains(tempName.ToUpper()))
                    {
                        object value = row[tempName];
                        pro.SetValue(t, value, null);
                    }
                }
                list.Add(t);
            }
            return list;
        }

        /// <summary>
        /// 从字符串中取BCD码
        /// 若str的长度不为偶数，则前面补0
        /// 若str为空，则返回长为 byteLength 的字节数组
        /// byteLength必须大于0，为要返回的数组长
        /// </summary>
        /// <param name="str"></param>
        /// <param name="byteLength">期望数组长度</param>
        /// <returns></returns>
        public static byte[] GetBCDFromString(string str, int byteLength)
        {
            if (byteLength <= 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(str))
            {
                return new byte[byteLength];
            }

            byte[] buffer = new byte[byteLength];

            if (str.Length % 2 != 0)
            {
                str = "0" + str;
            }

            int strNewLen = str.Length / 2;

            int len = Math.Min(strNewLen, byteLength);

            for (int i = 0; i < len; i++)
            {
                try
                {
                    buffer[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("解析BCD码错：{0}", ex));
                }
            }

            if (byteLength > strNewLen)
            {
                for (int i = strNewLen; i < byteLength; i++)
                {
                    buffer[i] = 0x20;   // 不足则补空格
                }
            }

            return buffer;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMiliseconds;
        }


        [DllImport("Kernel32.dll")]
        public static extern bool SetSystemTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern void GetSystemTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern bool SetLocalTime(ref SystemTime sysTime);

        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);


        /// <summary> 
        /// 设置本机时间 
        /// </summary> 
        public static bool SyncTime(DateTime currentTime)
        {
            bool flag = false;
            SystemTime sysTime = new SystemTime();
            sysTime.wYear = Convert.ToUInt16(currentTime.Year);
            sysTime.wMonth = Convert.ToUInt16(currentTime.Month);
            sysTime.wDay = Convert.ToUInt16(currentTime.Day);
            sysTime.wHour = Convert.ToUInt16(currentTime.Hour);
            sysTime.wMinute = Convert.ToUInt16(currentTime.Minute);
            sysTime.wSecond = Convert.ToUInt16(currentTime.Second);
            try
            {
                flag = SetLocalTime(ref sysTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("SetSystemDateTime函数执行异常" + e.Message);
            }
            return flag;
        }


    }
}
