using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace FexXianRouteFromDUpdate
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer timer = new System.Timers.Timer();
        private string time = string.Empty;
        public Form1()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            time = DateTime.Now.ToString("yyyy-MM-dd");
            string hour = "23:30";
            string strHourTime = string.Format("{0} {1}", time, hour);
            timer.Interval = 1000 * 60 * 60 * 24;
            timer.Elapsed += delegate
            {
                string routeSql = "select distinct 线路ID  from tblRouteD";
                DataTable routeDt = SqlHelper.ExecuteDataTable(routeSql);
                foreach (DataRow routeRow in routeDt.Rows)
                {
                    textBox1.Text = routeRow["线路id"].ToString();
                    string routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='上行' order by 线路内序号";

                    DataTable routeDUpQSdt = SqlHelper.ExecuteDataTable(routeDsql);
                    string sql = "update  tblRouteRFormD set  起点='" + routeDUpQSdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='上行'  and 起点!='" + routeDUpQSdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='上行' order by 线路内序号 desc ";

                    DataTable routeDUpZDdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  终点='" + routeDUpZDdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='上行'  and 终点!='" + routeDUpZDdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='下行' order by 线路内序号";

                    DataTable routeDDownQSdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  起点='" + routeDDownQSdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='下行'  and 起点!='" + routeDDownQSdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='下行' order by 线路内序号 desc";

                    DataTable routeDDownZDdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  终点='" + routeDDownZDdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='下行'  and 终点!='" + routeDDownZDdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);
                }

            };



            var Threads = new System.Threading.Thread(delegate ()
            {
                string routeSql = "select distinct 线路ID  from tblRouteD";
                DataTable routeDt = SqlHelper.ExecuteDataTable(routeSql);
                foreach (DataRow routeRow in routeDt.Rows)
                {
                    textBox1.Text = routeRow["线路id"].ToString();
                    string routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='上行' order by 线路内序号";

                    DataTable routeDUpQSdt = SqlHelper.ExecuteDataTable(routeDsql);
                    string sql = "update  tblRouteRFormD set  起点='" + routeDUpQSdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='上行'  and 起点!='" + routeDUpQSdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='上行' order by 线路内序号 desc ";

                    DataTable routeDUpZDdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  终点='" + routeDUpZDdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='上行'  and 终点!='" + routeDUpZDdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='下行' order by 线路内序号";

                    DataTable routeDDownQSdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  起点='" + routeDDownQSdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='下行'  and 起点!='" + routeDDownQSdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);


                    routeDsql = "select top 1 站名 from tblRouteD where 线路ID='" + routeRow["线路id"].ToString() + "'  and  方向='下行' order by 线路内序号 desc";

                    DataTable routeDDownZDdt = SqlHelper.ExecuteDataTable(routeDsql);
                    sql = "update  tblRouteRFormD set  终点='" + routeDDownZDdt.Rows[0]["站名"].ToString() + "' where 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 线路id='" + routeRow["线路id"].ToString() + "'   and  方向='下行'  and 终点!='" + routeDDownZDdt.Rows[0]["站名"].ToString() + "' ";
                    SqlHelper.ExuteNonQuery(sql);
                }
                while (true)
                {
                    if (DateTime.Now > Convert.ToDateTime(strHourTime))
                    {
                        timer.Start();
                        break;
                    }
                    System.Threading.Thread.Sleep(10000);
                }
            });

            Threads.Start();
        }
    }
}
