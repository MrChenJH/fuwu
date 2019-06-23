
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Util;


namespace FenXianRouteLasteZm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
          
        }


        /// <summary>
        /// 坐标转换
        /// </summary>
        /// <param name="ys">原始坐标</param>
        /// <returns></returns>
        public string Zb(string ys)
        {
            string address = "http://api.map.baidu.com/geoconv/v1/?coords=" + ys + "&from=1&to=5&ak=c9rSoqEeVaBX3jYZDhK28hHUvnOvF2Ex";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                var streamReader = new StreamReader(wr.GetResponseStream());
                string value = streamReader.ReadToEnd();
                var result = value.ToEntity<Result<V1>>();
                streamReader.Close();
                if (result.status == 0)
                {
                    return result.result[0].y.ToString() + "," + result.result[0].x.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取两坐标开车路距离
        /// </summary>
        /// <param name="origins">起点坐标</param>
        /// <param name="destinations">终点坐标</param>
        private distance GetDistance(string origins, string destinations)
        {
            string address = "http://api.map.baidu.com/routematrix/v2/driving?output=json&origins=" + origins + "&destinations=" + destinations + "&ak=c9rSoqEeVaBX3jYZDhK28hHUvnOvF2Ex";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
            req.Method = "GET";
            using (WebResponse wr = req.GetResponse())
            {
                var streamReader = new StreamReader(wr.GetResponseStream());
                string value = streamReader.ReadToEnd();
                var result = value.ToEntity<Result<V>>();
                streamReader.Close();
                if (result.status == 0)
                {
                    return result.result[0].distance;
                }
            }
            return null;
        }


        public delegate void ControlAddEx(string progressText);
        public ControlAddEx E;
        public void AddR(string progressText)
        {

            textBox1.Text = progressText;
        }

        private void 计算距离()
        {

            while (true)
            {
                Thread.Sleep(5000);
                string tblRouteSql = "select 线路ID from tblRoute ";
                DataTable routeData = SqlHelper.ExecuteDataTable(tblRouteSql);

                foreach (DataRow row in routeData.Rows)
                {
                    try
                    {
                        E("线路Id:" + row["线路ID"].ToString());
                        string origins = string.Empty, destinations = string.Empty;
                        string 该线上行终点坐标 = "select top 1 站名,经度,纬度,线路内序号 from tblRouteD WHERE 线路ID='" + row["线路ID"].ToString() + "' and 方向='上行' order by 线路内序号 desc";
                        var sxZd = SqlHelper.ExecuteDataTable(该线上行终点坐标);
                        if (sxZd.Rows.Count == 0)
                        {
                            continue;
                        }
                        //destinations = Zb(sxZd.Rows[0]["纬度"].ToString() + "," + sxZd.Rows[0]["经度"].ToString());
                        string 该线上行最前面车 = @"select 内部编号,离开中站序,位置描述    from  tblRouteBusStatus  
								           where 线路ID='" + row["线路ID"].ToString() + @"' 
                                            and  离开中站序<" + sxZd.Rows[0]["线路内序号"].ToString() + @"
                                            and  时刻>DATEADD(MINUTE,-5,GETDATE())
                                            and  方向='上行' 
                                            and  离开中站序 in (select MAX(离开中站序) from  tblRouteBusStatus where 线路ID='" + row["线路ID"].ToString() + @"' and 方向='上行'     and  离开中站序<" + sxZd.Rows[0]["线路内序号"].ToString() + @"   and  时刻>DATEADD(MINUTE,-5,GETDATE()))";
                        var sxC = SqlHelper.ExecuteDataTable(该线上行最前面车);

                        var listsXDs = new List<Udistance>();
                        if (sxC.Rows.Count > 0)
                        {
                            string sxjulisql = "select SUM(距离) 距离 from tblRouteD where  方向 = '上行'  and 线路ID='"+ row["线路ID"].ToString() + "'  and 线路内序号>" + sxC.Rows[0]["离开中站序"].ToString();
                            var sxjuli = SqlHelper.ExecuteDataTable(sxjulisql);
                            float 上行距离 = 0;
                            float.TryParse(sxjuli.Rows[0]["距离"].ToString(), out 上行距离);

                            foreach (DataRow sxRow in sxC.Rows)
                            {
                               
                                var 位置描述 = sxRow["位置描述"].ToString();
                                if (string.IsNullOrWhiteSpace(位置描述))
                                {
                                    continue;
                                }
                                var 位置 = float.Parse(位置描述.Split('+')[1].Replace("Km", string.Empty)) * 1000;
                                listsXDs.Add(new Udistance { nbbh = sxRow["内部编号"].ToString(), text = "", value = 上行距离 - 位置 });
                            }

                            var sxDistance = listsXDs.OrderBy(p => p.value).FirstOrDefault();
                            var sxfx = sxDistance.value / 500;
                            string sxsearchsql = @"select 线路ID from tblRouteArrivalVehicle 
                                           where  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + @"' and 线路ID='" + row["线路ID"].ToString() + "' and 方向='上行'";

                            var sxdataExist = SqlHelper.ExecuteDataTable(sxsearchsql);
                            if (sxdataExist.Rows.Count == 0)
                            {
                                string inserSql = @"INSERT INTO [dbo].[tblRouteArrivalVehicle]
                                            ([日期],[线路ID],[预计下一班到达终点站时间],[内部编号],[方向],更新时间)
                                       VALUES('" + DateTime.Now.ToString("yyyy-MM-dd") + @"','" + row["线路ID"] + @"','" + DateTime.Now.AddMinutes(sxfx).ToString("yyyy-MM-dd HH:mm:ss") + "','" + sxDistance.nbbh + "','上行','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                                SqlHelper.ExuteNonQuery(inserSql);
                            }
                            else
                            {
                                string updateSql = @" update [dbo].[tblRouteArrivalVehicle]
                                               set [预计下一班到达终点站时间]='" + DateTime.Now.AddMinutes(sxfx).ToString("yyyy-MM-dd HH:mm:ss") + @"',[内部编号]='" + sxDistance.nbbh + @"',更新时间='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"'
                                       where  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + @"' and 线路ID='" + row["线路ID"].ToString() + "' and 方向='上行'";
                                SqlHelper.ExuteNonQuery(updateSql);
                            }
                        }



                        string 该线下行终点坐标 = "select top 1 站名,经度,纬度,线路内序号 from tblRouteD WHERE 线路ID='" + row["线路ID"].ToString() + "' and 方向='下行' order by 线路内序号 desc";
                        var xxZd = SqlHelper.ExecuteDataTable(该线下行终点坐标);
                        destinations = Zb(xxZd.Rows[0]["纬度"].ToString() + "," + xxZd.Rows[0]["经度"].ToString());


                        string 该线下行最前面车 = @"select 内部编号,离开中站序,位置描述    from  tblRouteBusStatus  
								           where 线路ID='" + row["线路ID"].ToString() + @"' 
                                           and  离开中站序<" + xxZd.Rows[0]["线路内序号"].ToString() + @"
                                           and  时刻>DATEADD(MINUTE,-5,GETDATE())
                                           and 方向 = '下行'
                                           and  离开中站序 in (select MAX(离开中站序) from tblRouteBusStatus where 线路ID = '" + row["线路ID"].ToString() + @"' and 方向 = '下行' and 时刻>DATEADD(MINUTE, -5, GETDATE())     and  离开中站序 < " + xxZd.Rows[0]["线路内序号"].ToString() + @")";
                        var xxC = SqlHelper.ExecuteDataTable(该线下行最前面车);

                        var listXxDs = new List<Udistance>();
                        if (xxC.Rows.Count > 0)
                        {

                            string xxjulisql = "select SUM(距离) 距离 from tblRouteD where 方向 = '下行' and 线路ID='" + row["线路ID"].ToString() + "'  and 线路内序号>" + xxC.Rows[0]["离开中站序"].ToString();
                            var xxjuli = SqlHelper.ExecuteDataTable(xxjulisql);

                            float 下行距离 = 0;
                            float.TryParse(xxjuli.Rows[0]["距离"].ToString(), out 下行距离);
                            foreach (DataRow xxRow in xxC.Rows)
                            {
                                var 位置描述 = xxRow["位置描述"].ToString();
                                if (string.IsNullOrWhiteSpace(位置描述))
                                {
                                    continue;
                                }
                                var 位置 =float.Parse(位置描述.Split('+')[1].Replace("Km", string.Empty))*1000;

                             
                                listXxDs.Add(new Udistance { nbbh = xxRow["内部编号"].ToString(), text = "", value = 下行距离- 位置 });

                            }

                            var xxDistance = listXxDs.OrderBy(p => p.value).FirstOrDefault();
                            var xxfx = xxDistance.value / 500;

                            string xxsearchsql = @"select 线路ID from tblRouteArrivalVehicle 
                                           where  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + @"' and 线路ID='" + row["线路ID"].ToString() + "' and 方向='下行'";

                            var xxdataExist = SqlHelper.ExecuteDataTable(xxsearchsql);
                            if (xxdataExist.Rows.Count == 0)
                            {
                                string inserSql = @"INSERT INTO [dbo].[tblRouteArrivalVehicle]
                                            ([日期],[线路ID],[预计下一班到达终点站时间],[内部编号],[方向],更新时间)
                                       VALUES('" + DateTime.Now.ToString("yyyy-MM-dd") + @"','" + row["线路ID"].ToString() + @"','" + DateTime.Now.AddMinutes(xxfx).ToString("yyyy-MM-dd HH:mm:ss") + "','" + xxDistance.nbbh + "','下行','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                                SqlHelper.ExuteNonQuery(inserSql);
                            }
                            else
                            {
                                string updateSql = @" update [dbo].[tblRouteArrivalVehicle]
                                               set [预计下一班到达终点站时间]='" + DateTime.Now.AddMinutes(xxfx).ToString("yyyy-MM-dd HH:mm:ss") + @"',[内部编号]='" + xxDistance.nbbh + @"',更新时间='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"'
                                        where  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + @"' and 线路ID='" + row["线路ID"].ToString() + "' and 方向='下行'";
                                SqlHelper.ExuteNonQuery(updateSql);
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        E("异常:" + ex.Message);
                        Thread.Sleep(2000);
                    }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            E = AddR;
            Thread thread1 = new Thread(() =>
         {
             Parallel.Invoke(new Action[]{() => {
                计算距离();
            },
            ()=>{
                while(true){
                    SqlHelper.ExuteNonQuery("DELETE tblRouteArrivalVehicle where 更新时间<'"+ DateTime.Now.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss")+"'");
                    Thread.Sleep(1000);

                }
            }
         });
         });
            thread1.Start();




        }
    }
}
