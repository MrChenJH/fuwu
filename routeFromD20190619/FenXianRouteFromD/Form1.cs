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


namespace FenXianRouteFromD
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

        }
        public delegate void ControlAddEx(string progressText);
        public ControlAddEx E;
        public void AddR(string progressText)
        {

            textBox1.Text = progressText;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            E = AddR;
            new Thread(() =>
              {
                  string bz = "0";
                  while (true)
                  {
                      Thread.Sleep(5000);
                      string bzsql = "select 标记 from tblRouteFormDBag where 日期 ='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                      var bzData = SqlHelper.ExecuteDataTable(bzsql);
                      if (bzData.Rows.Count == 0)
                      {
                          string insertSql = "insert into  tblRouteFormDBag(日期,标记)VALUES('" + DateTime.Now.ToString("yyyy-MM-dd") + "',0)";
                          SqlHelper.ExuteNonQuery(insertSql);
                      }
                      else
                      {
                          bz = bzData.Rows[0][0].ToString();

                      }

                      string formx = "select 流水号,CONVERT(varchar(10),日期,120) 日期,线路ID," +
                         "内部编号,驾驶员,行号,方向,站点名称,CONVERT(varchar,到达时刻,108) 到达时刻," +
                         "CONVERT(varchar,驶离时刻,108) 驶离时刻," +
                         "里程表数  from  tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + "  where 线路ID!='' and 流水号>" + bz;

                      DataTable dtFormx = SqlHelper.ExecuteDataTable(formx);
                      if (dtFormx.Rows.Count == 0)
                      {
                          continue;
                      }
                      var formxSet = (from t in dtFormx.AsEnumerable()
                                      select new
                                      {
                                          流水号 = t.Field<long>("流水号"),
                                          日期 = t.Field<string>("日期"),
                                          线路ID = t.Field<string>("线路ID"),
                                          内部编号 = t.Field<string>("内部编号"),
                                          驾驶员 = t.Field<string>("驾驶员"),
                                          站点名称 = t.Field<string>("站点名称"),
                                          方向 = t.Field<string>("方向"),
                                          到达时刻 = t.Field<string>("到达时刻"),
                                          驶离时刻 = t.Field<string>("驶离时刻"),
                                          里程表数 = t.Field<Single>("里程表数")
                                      }).ToList();

                      bz = formxSet.Max(t => t.流水号).ToString();
                      string updatesql = "update tblRouteFormDBag set  标记=" + bz + " where  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'";
                      SqlHelper.ExuteNonQuery(updatesql);
                      var nbbhSet = formxSet.Select(t => new { t.内部编号, t.线路ID }).Where(p => p.线路ID != null && p.线路ID != string.Empty).Distinct();
                      Parallel.ForEach(nbbhSet, (item, pls, longs) =>
                      {
                          //    foreach (var item in nbbhSet)
                          //{

                          var formxSetCj = formxSet.Where(t => t.内部编号 == item.内部编号).OrderBy(t => t.流水号);

                          foreach (var itemcj in formxSetCj)
                          {
                              E("内部编号:" + itemcj.内部编号);
                              Process(itemcj.线路ID, itemcj.内部编号, itemcj.驾驶员, itemcj.日期, itemcj.站点名称, itemcj.站点名称, itemcj.驶离时刻.Equals("00:00:00") ? itemcj.到达时刻 : itemcj.驶离时刻, itemcj.驶离时刻.Equals("00:00:00") ? itemcj.到达时刻 : itemcj.驶离时刻, itemcj.到达时刻, itemcj.到达时刻, itemcj.方向, itemcj.里程表数, itemcj.里程表数);
                          }
                          //}
                      });

                  }
              }).Start();



        }


        /// <summary>
        /// 操作路单主方法
        /// </summary>
        /// <param name="线路ID"></param>
        /// <param name="内部编号"></param>
        /// <param name="驾驶员"></param>
        /// <param name="日期"></param>
        /// <param name="起点"></param>
        /// <param name="终点"></param>
        /// <param name="计划发车时刻"></param>
        /// <param name="实际发车时刻"></param>
        /// <param name="实际到达时刻"></param>
        /// <param name="方向"></param>
        /// <param name="计划到达时刻"></param>
        /// <param name="起点里程表数"></param>
        /// <param name="终点里程表数"></param>
        /// <param name="单程里程"></param>
        private void Process(string 线路ID, string 内部编号, string 驾驶员, string 日期, string 起点, string 终点,
                                   string 计划发车时刻, string 实际发车时刻, string 实际到达时刻,
                                   string 计划到达时刻, string 方向, Single 起点里程表数, Single 终点里程表数)
        {

            try
            {
                DataTable xlcd = new DataTable();
                if (方向.Trim().Equals("上行"))
                {
                    string lccd = "select  上行长度  as  长度 from  tblRoute where 线路ID='" + 线路ID + "'";
                    xlcd = SqlHelper.ExecuteDataTable(lccd);
                }
                else
                {
                    string lccd = "select  下行长度 as  长度   from  tblRoute where 线路ID='" + 线路ID + "'";
                    xlcd = SqlHelper.ExecuteDataTable(lccd);
                }



                string 当前车号已经创建的该车路单 = "select top 1 流水号,线路ID,方向,起点,终点,起点里程表数,CONVERT(varchar,实际发车时刻,108)  实际发车时刻," +
                    "CONVERT(varchar,实际到达时刻,108)  实际到达时刻  from  tblRouteRFormD  where 内部编号='" + 内部编号 + "' and 日期='" + 日期 + "' order by 流水号 desc";
                DataTable createdFormSet = SqlHelper.ExecuteDataTable(当前车号已经创建的该车路单);
                if (createdFormSet.Rows.Count == 0)
                {
                    string sql = @"Insert into tblRouteRFormD(日期,路单编号,线路ID,内部编号,路牌,行号,方向,起点,终点,驾驶员,乘务员,调度员,计划报到时刻,实际报到时刻,路牌计划时刻
                                       ,计划发车时刻,实际发车时刻,发车响应时刻,计划到达时刻,实际到达时刻,实际发车有效,校时点快慢,校时点时耗
                                       ,发车间隔,快慢,单程限时,单程时耗,累计油耗起,累计油耗止,单程油耗,油量余额,起点里程表数,终点里程表数
                                       ,单程里程,非营运,非营运1用时,非营运2用时,非营运3用时,非营运4用时,异常记录,记录来源,备注,单程刷卡次数
                                       ,单程刷卡金额,单程免费刷卡次数,单程免费刷卡金额,实际是否运行,是否确认,最小单程限时,趟次)
                                     values('" + 日期 + @"','','" + 线路ID + @"','" + 内部编号 + @"','0','0','" + 方向 + @"','" + 起点 + @"','" + 终点 + @"','" + 驾驶员 + @"','',''
                                       ,'1900-01-01 00:00:00','1900-01-01 00:00:00','1900-01-01 00:00:00'
                                       ,'" + 计划发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','0','0','0'
                                       ,'0','0','0','0','0','0','0','0','" + 起点里程表数 + @"','" + 终点里程表数 + @"'
                                       ,'0','0','0','0','0','0','0','0','0','0'
                                       ,'0','0','0','0','0','0','0')";
                    SqlHelper.ExuteNonQuery(sql);

                }
                else
                {
                    string y方向 = createdFormSet.Rows[0]["方向"].ToString().Trim();
                    string y线路ID = createdFormSet.Rows[0]["线路ID"].ToString().Trim();
                    string y终点 = createdFormSet.Rows[0]["终点"].ToString().Trim();
                    string y起点 = createdFormSet.Rows[0]["起点"].ToString().Trim();
                    string y实际发车时刻 = createdFormSet.Rows[0]["实际发车时刻"].ToString();
                    string y实际到达时刻 = createdFormSet.Rows[0]["实际到达时刻"].ToString();

                    Single y起点里程表数 = 0;
                    Single.TryParse(Convert.ToString(createdFormSet.Rows[0]["起点里程表数"]), out y起点里程表数);
                    string zdSql = "  select 线路内序号,站名 from tblRouteD where 方向='" + y方向 + "' and  线路ID = '" + y线路ID + "'";


                    var zdSet = SqlHelper.ExecuteDataTable(zdSql).AsEnumerable().Select(p => new { 线路内序号 = p.Field<short>("线路内序号"), 站名 = p.Field<string>("站名") }).ToList();

                    if (方向.Trim().Equals(y方向.Trim()))
                    {

                        TimeSpan time = Convert.ToDateTime(实际发车时刻) - Convert.ToDateTime(y实际到达时刻);
                        if (time.Minutes < 30)
                        {
                            var n1 = zdSet.FirstOrDefault(t => t.站名 == 终点).线路内序号;
                            var n2 = zdSet.FirstOrDefault(t => t.站名 == y终点).线路内序号;
                            if (n2 - n1 <= 5)
                            {
                                if (y实际发车时刻.Equals("00:00:00"))
                                {
                                    string updateSql = "update   tblRouteRFormD  set 终点='" + 终点 + "',计划发车时刻='" + 实际发车时刻 + "',实际发车时刻='" + 实际发车时刻 + "',实际到达时刻='" + 实际到达时刻 + "',终点里程表数='" + 终点里程表数 + "',单程里程='" + xlcd.Rows[0]["长度"].ToString() + "'   where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                    SqlHelper.ExuteNonQuery(updateSql);

                                }
                                else
                                {
                                    string updateSql = "update   tblRouteRFormD  set 终点='" + 终点 + "',实际到达时刻='" + 实际到达时刻 + "',终点里程表数='" + 终点里程表数 + "',单程里程='" + xlcd.Rows[0]["长度"].ToString() + "' where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                    SqlHelper.ExuteNonQuery(updateSql);
                                }
                            }

                        }
                        else
                        {
                            if (y起点.Equals(y终点))
                            {
                                string updateSql1 = "delete   tblRouteRFormD  where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                SqlHelper.ExuteNonQuery(updateSql1);
                            }
                            else
                            {
                                var zm = zdSet.OrderBy(t => t.线路内序号).FirstOrDefault();
                                if (zm.站名.Equals(y终点))
                                {
                                    string updateSql1 = "delete   tblRouteRFormD  where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                    SqlHelper.ExuteNonQuery(updateSql1);
                                }
                                else
                                {
                                    var zd = zdSet.OrderByDescending(t => t.线路内序号).FirstOrDefault();
                                    string updateSql = "update   tblRouteRFormD  set 起点='" + zm.站名 + "',终点='" + zd.站名 + "'      where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                    SqlHelper.ExuteNonQuery(updateSql);
                                }

                            }



                            string sql = @"Insert into tblRouteRFormD(日期,路单编号,线路ID,内部编号,路牌,行号,方向,起点,终点,驾驶员,乘务员,调度员,计划报到时刻,实际报到时刻,路牌计划时刻
                                       ,计划发车时刻,实际发车时刻,发车响应时刻,计划到达时刻,实际到达时刻,实际发车有效,校时点快慢,校时点时耗
                                       ,发车间隔,快慢,单程限时,单程时耗,累计油耗起,累计油耗止,单程油耗,油量余额,起点里程表数,终点里程表数
                                       ,单程里程,非营运,非营运1用时,非营运2用时,非营运3用时,非营运4用时,异常记录,记录来源,备注,单程刷卡次数
                                       ,单程刷卡金额,单程免费刷卡次数,单程免费刷卡金额,实际是否运行,是否确认,最小单程限时,趟次)
                                     values('" + 日期 + @"','','" + 线路ID + @"','" + 内部编号 + @"','0','0','" + 方向 + @"','" + 起点 + @"','" + 终点 + @"','" + 驾驶员 + @"','',''
                                       ,'1900-01-01 00:00:00','1900-01-01 00:00:00','1900-01-01 00:00:00'
                                       ,'" + 计划发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','0','0','0'
                                       ,'0','0','0','0','0','0','0','0','" + 起点里程表数 + @"','" + 终点里程表数 + @"'
                                       ,'0','0','0','0','0','0','0','0','0','0'
                                       ,'0','0','0','0','0','0','0')";
                            SqlHelper.ExuteNonQuery(sql);
                        }


                    }
                    else
                    {
                        if (y起点.Equals(y终点))
                        {
                            string updateSql1 = "delete   tblRouteRFormD  where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                            SqlHelper.ExuteNonQuery(updateSql1);
                        }
                        else
                        {
                            var zm = zdSet.OrderBy(t => t.线路内序号).FirstOrDefault();
                            if (zm.站名.Equals(y终点))
                            {
                                string updateSql1 = "delete   tblRouteRFormD  where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                SqlHelper.ExuteNonQuery(updateSql1);
                            }
                            else
                            {
                                if (!方向.Trim().Equals("上行"))
                                {
                                    string lccd = "select  上行长度  as  长度 from  tblRoute where 线路ID='" + 线路ID + "'";
                                    xlcd = SqlHelper.ExecuteDataTable(lccd);
                                }
                                else
                                {
                                    string lccd = "select  下行长度 as  长度   from  tblRoute where 线路ID='" + 线路ID + "'";
                                    xlcd = SqlHelper.ExecuteDataTable(lccd);
                                }
                                var zmd = zdSet.OrderByDescending(t => t.线路内序号).FirstOrDefault();
                                string updateSql = "update   tblRouteRFormD  set 起点='" + zm.站名 + "',终点='" + zmd.站名 + "',单程里程='" + xlcd.Rows[0]["长度"].ToString() + "' where 流水号=" + createdFormSet.Rows[0]["流水号"].ToString();
                                SqlHelper.ExuteNonQuery(updateSql);
                            }

                        }

                        string 当前车号已经创建的该车路单1 = "select top 1 流水号,线路ID,方向,起点,终点,起点里程表数,CONVERT(varchar,实际发车时刻,108)  实际发车时刻,CONVERT(varchar,实际到达时刻,108)  实际到达时刻  from  tblRouteRFormD  where 内部编号='" + 内部编号 + "' and 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' order by 流水号 desc";
                        DataTable createdFormSet1 = SqlHelper.ExecuteDataTable(当前车号已经创建的该车路单);
                        if (createdFormSet.Rows.Count == 0)
                        {
                            string sql1 = @"Insert into tblRouteRFormD(日期,路单编号,线路ID,内部编号,路牌,行号,方向,起点,终点,驾驶员,乘务员,调度员,计划报到时刻,实际报到时刻,路牌计划时刻
                                       ,计划发车时刻,实际发车时刻,发车响应时刻,计划到达时刻,实际到达时刻,实际发车有效,校时点快慢,校时点时耗
                                       ,发车间隔,快慢,单程限时,单程时耗,累计油耗起,累计油耗止,单程油耗,油量余额,起点里程表数,终点里程表数
                                       ,单程里程,非营运,非营运1用时,非营运2用时,非营运3用时,非营运4用时,异常记录,记录来源,备注,单程刷卡次数
                                       ,单程刷卡金额,单程免费刷卡次数,单程免费刷卡金额,实际是否运行,是否确认,最小单程限时,趟次)
                                     values('" + 日期 + @"','','" + 线路ID + @"','" + 内部编号 + @"','0','0','" + 方向 + @"','" + 起点 + @"','" + 终点 + @"','" + 驾驶员 + @"','',''
                                       ,'1900-01-01 00:00:00','1900-01-01 00:00:00','1900-01-01 00:00:00'
                                       ,'" + 计划发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','0','0','0'
                                       ,'0','0','0','0','0','0','0','0','" + 起点里程表数 + @"','" + 终点里程表数 + @"'
                                       ,'0','0','0','0','0','0','0','0','0','0'
                                       ,'0','0','0','0','0','0','0')";
                            SqlHelper.ExuteNonQuery(sql1);

                        }
                        else
                        {
                            string y方向1 = createdFormSet.Rows[0]["方向"].ToString();
                            string y线路ID1 = createdFormSet.Rows[0]["线路ID"].ToString();
                            string y终点1 = createdFormSet.Rows[0]["终点"].ToString();
                            string y起点1 = createdFormSet.Rows[0]["起点"].ToString();
                            string y实际发车时刻1 = createdFormSet.Rows[0]["实际发车时刻"].ToString();
                            string y实际到达时刻1 = createdFormSet.Rows[0]["实际到达时刻"].ToString();

                            if (!y方向1.Equals(方向))
                            {

                                string sql = @"Insert into tblRouteRFormD(日期,路单编号,线路ID,内部编号,路牌,行号,方向,起点,终点,驾驶员,乘务员,调度员,计划报到时刻,实际报到时刻,路牌计划时刻
                                       ,计划发车时刻,实际发车时刻,发车响应时刻,计划到达时刻,实际到达时刻,实际发车有效,校时点快慢,校时点时耗
                                       ,发车间隔,快慢,单程限时,单程时耗,累计油耗起,累计油耗止,单程油耗,油量余额,起点里程表数,终点里程表数
                                       ,单程里程,非营运,非营运1用时,非营运2用时,非营运3用时,非营运4用时,异常记录,记录来源,备注,单程刷卡次数
                                       ,单程刷卡金额,单程免费刷卡次数,单程免费刷卡金额,实际是否运行,是否确认,最小单程限时,趟次)
                                     values('" + 日期 + @"','','" + 线路ID + @"','" + 内部编号 + @"','0','0','" + 方向 + @"','" + 起点 + @"','" + 终点 + @"','" + 驾驶员 + @"','',''
                                       ,'1900-01-01 00:00:00','1900-01-01 00:00:00','1900-01-01 00:00:00'
                                       ,'" + 计划发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','" + 实际发车时刻 + @"','0','0','0'
                                       ,'0','0','0','0','0','0','0','0','" + 起点里程表数 + @"','" + 终点里程表数 + @"'
                                       ,'0','0','0','0','0','0','0','0','0','0'
                                       ,'0','0','0','0','0','0','0')";
                                SqlHelper.ExuteNonQuery(sql);
                            }


                        }

                    }
                }

            }
            catch (Exception ex)
            {
                E(ex.Message);
            }

        }

    }
}
