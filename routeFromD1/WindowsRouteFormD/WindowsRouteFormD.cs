using ClientRouteProcess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Util;

namespace WindowsRouteFormD
{
    public partial class WindowsRouteFormD : Form
    {

        public WindowsRouteFormD()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

        }

        public delegate void ControlAddEx(string progressText);
        public delegate void ControlAddR(string progressText);
        public ControlAddEx E;
        public ControlAddR R;
        public void AddR(string progressText)
        {

            textBox2.Text = progressText;
        }




        public class Demo
        {
            public string Sname { get; set; }
            public string Stype { get; set; }

            public string Ptype { get; set; }

            public string Online { get; set; }


            public string Lines { get; set; }

            public string Remark { get; set; }
        }

        public void AddEx(string progressText)
        {
            textBox1.AppendText("\\n\\t" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\\n\\t" + progressText);

        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            try
            {
                string sql = "select 线路ID from tblRouteRFormD  where 日期 =convert(varchar(10),getdate(),120)   group by 线路ID";
                var routeData = SqlHelper.ExecuteDataTable(sql);
                foreach (DataRow row in routeData.Rows)
                {
                  
                    var route = (from p in SqlHelper.ExecuteDataTable(@"select '上行' as 方向,ISNULL(CAST(上行长度 as varchar),'0')  as 长度 from tblRoute   where 线路ID='" + Convert.ToString(row["线路ID"]) + @"'
                                                                            union all 
                                                                      select '下行' as 方向,ISNULL(CAST(下行长度 as varchar),'0')  as 长度 from tblRoute   where 线路ID='" + Convert.ToString(row["线路ID"]) + @"' ").AsEnumerable()
                                 select new
                                 {
                                     方向 = p.Field<string>("方向"),
                                     长度 = p.Field<string>("长度")
                                 }).ToList();

                    sql = "select  流水号,CONVERT(varchar(10),日期,120) 日期,线路ID,内部编号,起点,终点,方向, CONVERT(varchar,计划发车时刻,108) 计划发,驾驶员,状态 from  [tblRouteRFormD] " +
                                      "   where 流水号=4463597 ";
                    var dt = SqlHelper.ExecuteDataTable(sql);


                    foreach (DataRow dr in dt.Rows)
                    {
                        R("线路ID:" + Convert.ToString(row["线路ID"]) + "\n\r" + "  流水号:" + dr["流水号"].ToString());
                        try
                        {
                            string sqlcount = " select count(1) c  from  tblRouteRFormD   where 内部编号='" + Convert.ToString(dr["内部编号"]) + "' and  CONVERT(varchar,计划发车时刻,108)='" + Convert.ToString(dr["计划发"]) + "' and   日期='" + Convert.ToString(dr["日期"]) + "'";
                            var fcount = SqlHelper.ExecuteDataTable(sqlcount);
                            if (Convert.ToInt64(fcount.Rows[0]["c"]) > 1)
                            {
                                string deletesql = "delete from tblRouteRFormD  where 流水号=" + Convert.ToString(dr["流水号"]);
                                SqlHelper.ExuteNonQuery(deletesql);
                                continue;
                            }


                            var dateNow = dr["日期"].ToString();
                            string sql111 = @"select ROW_NUMBER()over(order by 驶离时刻) 索引,  convert(varchar,驶离时刻,108) as 驶离时刻, 里程表数  
                                           from tblRouteRFormX" + Convert.ToDateTime(dr["日期"].ToString()).ToString("yyyyMMdd") + @"
                                          where   内部编号 ='" + dr["内部编号"].ToString() + @"'
                                              and 日期 = '" + dr["日期"].ToString() + @"' 
                                              and 站点名称='" + dr["起点"].ToString() + @"'        
                                              and 方向 = '" + dr["方向"].ToString() + @"' ";
                            var sfData1 = SqlHelper.ExecuteDataTable(sql111);

                            string sql1111 = @"  select *,convert(varchar,计划到达时间,108) 计划到达 from tbldailyplan1
                                             where   日期 = '" + dr["日期"].ToString() + @"'
                                                    and   替换车号 ='" + dr["内部编号"].ToString() + @"' 
                                                    and   cast(isnull(调整时刻,计划发车时间)  as time(7))=cast( '" + dr["计划发"].ToString() + @"'as time(7))
                                                    and   方向 = '" + dr["方向"].ToString() + @"'  ";
                            var sfData2 = SqlHelper.ExecuteDataTable(sql1111);

                            var sfData = new DataTable();
                            decimal sf里程表数 = 0;

                            string 实际发 = string.Empty;

                            bool isExistSjf = false;
                            foreach (DataRow rrow in sfData1.Rows)
                            {

                                if (Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()).AddMinutes(30) >= Convert.ToDateTime(dateNow + " " + rrow["驶离时刻"].ToString()) && Convert.ToDateTime(dateNow + " " + rrow["驶离时刻"].ToString()) >= Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()).AddMinutes(-30))
                                {
                                    实际发 = rrow["驶离时刻"].ToString();
                                    sf里程表数 = Convert.ToDecimal(rrow["里程表数"]);
                                    isExistSjf = true;
                                    break;
                                }

                            }

                            if (!isExistSjf)
                            {
                                if (Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()) <= DateTime.Now.AddMinutes(5))
                                {
                                    var r = new Random();
                                    实际发 = Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()).AddMinutes(r.Next(1, 5)).ToString("HH:mm:ss");
                                }
                                else
                                {
                                    实际发 = "";
                                }
                            }

                            DataTable actData = new DataTable();

                            string sql2 = string.Empty;
                            if (!string.IsNullOrWhiteSpace(实际发))
                            {
                                sql2 = @"select convert(varchar,到达时刻,108) 到达时刻, 里程表数 from tblRouteRFormX" + Convert.ToDateTime(dr["日期"].ToString()).ToString("yyyyMMdd") + @" main where
                                                                     日期='" + dr["日期"].ToString() + @"' and 内部编号='" + dr["内部编号"].ToString() + @"'   and
                                                                         exists(  select b.sk from
                                                                               (select min(到达时刻) sk from tblRouteRFormX" + Convert.ToDateTime(dr["日期"].ToString()).ToString("yyyyMMdd") + @"
                                                                         where 
                                                                           日期='" + dr["日期"].ToString() + @"'
                                                                     
                                                                       and 内部编号='" + dr["内部编号"].ToString() + @"' 
                                                                       and 站点名称 ='" + dr["终点"].ToString() + @"'
                                                                       and 到达时刻>DATEADD(MINUTE,15,cast( '" + dr["日期"].ToString() + " " + 实际发 + @"' as datetime))) b 
                                                                       where b.sk=main.到达时刻)";
                                actData = SqlHelper.ExecuteDataTable(sql2);
                            }

                            string sql6 = @"   select ISNULL(MIN(DATEDIFF(minute,计划发车时间,计划到达时间)),0)*2 单边运行 from tbldailyplan1 
                                                                               where  日期='" + dr["日期"].ToString() + @"'
                                                                                      and  cast(isnull(调整时刻,计划发车时间)  as time(7))=cast( '" + dr["计划发"].ToString() + @"'as time(7))
                                                                                      and 替换车号='" + dr["内部编号"].ToString() + @"'";
                            var dbyx = SqlHelper.ExecuteDataTable(sql6);

                            string 驾驶员 = string.Empty;

                            var drviers = SqlHelper.ExecuteDataTable(@"select WorkName,记录时刻 from   WorkCardRecord 
                                              where CardCode='" + dr["内部编号"].ToString() + @"'    and  记录日期='" + dr["日期"].ToString() + @"'   order by  记录时刻 desc");

                            if (drviers.Rows.Count > 0)
                            {
                                if (drviers.Rows.Count == 1)
                                {
                                    驾驶员 = Convert.ToString(drviers.Rows[0]["WorkName"]);
                                }
                                else
                                {
                                    if (Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()).AddMinutes(10) > Convert.ToDateTime(drviers.Rows[0]["记录时刻"]))
                                    {
                                        驾驶员 = Convert.ToString(drviers.Rows[0]["WorkName"]);
                                    }
                                    else
                                    {
                                        驾驶员 = Convert.ToString(drviers.Rows[1]["WorkName"]);
                                    }
                                }
                            }
                            else
                            {
                                驾驶员 = "";
                            }



                            string 计划到达 = string.Empty;


                            if (sfData2.Rows.Count > 0)
                            {

                                计划到达 = sfData2.Rows[0]["计划到达"].ToString();
                            }
                            else
                            {
                                计划到达 = "";
                            }


                            float 理程 = 0;

                            string 实际到 = string.Empty;

                            if (actData.Rows.Count > 0)
                            {
                                if (Convert.ToDateTime(dateNow + " " + dr["计划发"].ToString()).AddMinutes(Convert.ToUInt32(dbyx.Rows[0][0])) > Convert.ToDateTime(dateNow + " " + actData.Rows[0]["到达时刻"].ToString()))
                                {
                                    实际到 = actData.Rows[0]["到达时刻"].ToString();

                                }
                                else
                                {
                                    if (sfData2.Rows.Count > 0)
                                    {
                                        var r = new Random();
                                        实际到 = Convert.ToDateTime(dateNow + " " + sfData2.Rows[0]["计划到达"].ToString()).AddMinutes(-r.Next(1, 5)).ToString("HH:mm:ss");
                                    }
                                    else
                                    {
                                        实际到 = "00:00:00";
                                    }

                                }

                                var l = decimal.Parse(Convert.ToString(actData.Rows[0]["里程表数"])) - sf里程表数;
                                if (l <= 0 || sf里程表数 == 0)
                                {
                                    var lc = route.FirstOrDefault(t => t.方向 == dr["方向"].ToString());
                                    if (lc != null)
                                    {
                                        理程 = float.Parse(lc.长度);
                                    }
                                }
                                else
                                {
                                    理程 = (float)l;
                                }
                            }
                            else
                            {

                                if (sfData2.Rows.Count > 0)
                                {
                                    if (Convert.ToDateTime(dateNow + " " + sfData2.Rows[0]["计划到达"].ToString()).AddMinutes(30) <= DateTime.Now)
                                    {

                                        var lc = route.FirstOrDefault(t => t.方向 == dr["方向"].ToString());
                                        if (lc != null)
                                        {
                                            理程 = float.Parse(lc.长度);
                                        }
                                        var r = new Random();
                                        实际到 = Convert.ToDateTime(dateNow + " " + sfData2.Rows[0]["计划到达"].ToString()).AddMinutes(r.Next(1, 8)).ToString("HH:mm:ss");

                                    }
                                    else
                                    {
                                        实际到 = "";
                                    }
                                }
                                else
                                {
                                    LogHelper.WriteLog("流水号:" + Convert.ToString(dr["流水号"]));
                                }

                            }
                            string 空调 = "0";
                            if (DateTime.Now.Month >= 5 && DateTime.Now.Month <= 9)
                            {
                                空调 = "1";
                            }

                            if (!string.IsNullOrWhiteSpace(实际到))
                            {
                                string updateSql = "update [tblRouteRFormD] set          驾驶员='" + 驾驶员 + "',实际发车时刻='" + dateNow + " " + 实际发 + "',计划到达时刻='" + dateNow + " " + 计划到达 + "',实际到达时刻='" + dateNow + " " + 实际到 + "',单程里程='" + 理程 + "',趟次='" + 空调 + "',状态=2           where    流水号='" + Convert.ToString(dr["流水号"]) + "'";
                                SqlHelper.ExuteNonQuery(updateSql);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(实际发))
                                {
                                    if (dr["状态"].ToString().Equals("0"))
                                    {
                                        string updateSql = "update [tblRouteRFormD]    set       实际发车时刻='" + dateNow + " " + 实际发 + "',计划到达时刻='" + dateNow + " " + 计划到达 + "',实际到达时刻='" + dateNow + " 00:00:00',单程里程='0',趟次='" + 空调 + "'   where     流水号='" + Convert.ToString(dr["流水号"]) + "'";
                                        SqlHelper.ExuteNonQuery(updateSql);
                                    }
                                }
                                else
                                {
                                    string updateSql = "      update [tblRouteRFormD]  set               实际发车时刻='" + dateNow + " 00:00:00',计划到达时刻='" + dateNow + " " + 计划到达 + "',实际到达时刻='" + dateNow + " 00:00:00',单程里程='0',趟次='" + 空调 + "'   where     流水号='" + Convert.ToString(dr["流水号"]) + "'";
                                    SqlHelper.ExuteNonQuery(updateSql);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            E(ex.ToString());
                            var esss = ex;
                            LogHelper.WriteLog("错误信息" + ex.StackTrace + ",流水号:" + Convert.ToString(dr["流水号"]));
                            string ss = dr[0].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                E(ex.ToString());
                var esss = ex;

            }
        }

        /// <summary>
        /// 日期
        /// </summary>
        private DateTime _dateTime = new DateTime();
        private void Form1_Load(object sender, EventArgs e)
        {
            E = new ControlAddEx(AddR);

            R = new ControlAddR(AddR);

            Thread thread1 = new Thread(() =>
            {
                while (true)
                {
                    ProcessData();
                    Thread.Sleep(50000);
                }
            });
            thread1.Start();

            //_dateTime = DateTime.Now;
            //Thread thread2 = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        if (DateTime.Now.Hour == 20)
            //        {
            //            try
            //            {
            //                string sql = "select 内部编号  " +
            //                      "                                 from tblRouteBusStatus" +
            //                      "     where convert(varchar(10), UpdateTime, 120) < '" + DateTime.Now.ToString("yyyy-MM-dd") + "'" +
            //                      "     and  convert(varchar(10),时刻,120) < '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            //                var routeData1 = SqlHelper.ExecuteDataTable(sql);
            //                foreach (DataRow row in routeData1.Rows)
            //                {

            //                    sql = "insert into tblRouteRFormD_bak2019 " +
            //                        "  select  * from tblRouteRFormD " +
            //                        "  where  内部编号='" + Convert.ToString(row["内部编号"]) + "' " +
            //                        "  and  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
            //                    SqlHelper.ExuteNonQuery(sql);
            //                    sql = " delete  from tblRouteRFormD  " +
            //                         "   where  内部编号='" + Convert.ToString(row["内部编号"]) + "' " +
            //                         "    and    日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'   ";
            //                    SqlHelper.ExuteNonQuery(sql);
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                LogHelper.WriteLog(ex.Message);
            //            }

            //        }
            //        if (_dateTime.Date < DateTime.Now.Date)
            //            DelFormD();
            //        Thread.Sleep(100000);
            //    }
            //});
            //thread2.Start();

        }

        public void DelFormD()
        {
            try
            {


                string sql = "select 内部编号,max(到达时刻) 到达时刻 from tblRouteRFormX" + _dateTime.ToString("yyyyMMdd") + " group by 内部编号";
                var routeData = SqlHelper.ExecuteDataTable(sql);
                foreach (DataRow row in routeData.Rows)
                {
                    sql = "insert into tblRouteRFormD_bak2019 " +
                        "  select  * from tblRouteRFormD " +
                        "  where  内部编号='" + Convert.ToString(row["内部编号"]) + "' " +
                        "  and  计划发车时刻>'" + Convert.ToDateTime(row["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                        "  and  日期='" + _dateTime.ToString("yyyy-MM-dd") + "'";
                    SqlHelper.ExuteNonQuery(sql);
                    sql = " delete  from tblRouteRFormD  " +
                        "  where  内部编号='" + Convert.ToString(row["内部编号"]) + "' " +
                        "  and    计划发车时刻>'" + Convert.ToDateTime(row["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                        "  and    日期='" + _dateTime.ToString("yyyy-MM-dd") + "'   ";
                    SqlHelper.ExuteNonQuery(sql);
                }



                _dateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
            }
        }

        private void WindowsRouteFormD_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void WindowsRouteFormD_FormClosed(object sender, FormClosedEventArgs e)
        {


            //关闭这个程序
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
