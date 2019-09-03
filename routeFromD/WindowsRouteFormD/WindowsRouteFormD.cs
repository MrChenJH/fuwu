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
using System.Configuration;

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




        public void AddEx(string progressText)
        {



            //string 第一班 = "select top 1 CONVERT(varchar,isnull(调整时刻,计划发车时间),108) 计划发车时间  from tbldailyplan1 where 替换车号='" + 内部编号 + @"' and 线路id='" + 线路ID + "'  order by 计划发车时间 ";
            //DataTable fistShift = SqlHelper.ExecuteDataTable(第一班);

            //if (fistShift.Rows.Count > 0)
            //{
            //    if (Convert.ToString(fistShift.Rows[0]["计划发车时间"]).Equals(Convert.ToString(formR["计划发"])))
            //    {
            //        string jsysql = "select top 1 workName from WorkCardRecord where 记录日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and CardCode='" + 内部编号 + "' order by 记录时刻";
            //        DataTable jsysqldata = SqlHelper.ExecuteDataTable(jsysql);
            //        if (jsysqldata.Rows.Count > 0)
            //        {
            //            string 第四步 = "update tblRouteRFormD " +
            //          "                     set 驾驶员 = '" + Convert.ToString(jsysqldata.Rows[0]["workName"]) + "'" +
            //          "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
            //            SqlHelper.ExuteNonQuery(第四步);
            //        }
            //        else
            //        {
            //            continue;
            //        }
            //    }
            //}

            textBox1.AppendText("\\n\\t" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\\n\\t" + progressText);

        }

        public Dictionary<string, plan> _dics = new Dictionary<string, plan>();

        public class plan
        {
            public string planTime { get; set; }

            public string arrivedPlanTime { get; set; }
        }


        /// <summary>
        /// 出场日期
        /// </summary>
        private string _NonFormDCCdateTime = string.Empty;

        /// <summary>
        /// 进场日期
        /// </summary>
        private string _NonFormDJCdateTime = string.Empty;

        private void RunJsy()
        {

            string sql = "select 内部编号,convert(varchar(19), min(计划发车时刻),121)  计划发车时刻 from tblRouteRFormD where isnull(驾驶员,'')='' and 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' group by 内部编号";
            DataTable data = SqlHelper.ExecuteDataTable(sql);
            for (var i = 0; i < data.Rows.Count; i++)
            {
                try
                {
                    string 内部编号 = Convert.ToString(data.Rows[i]["内部编号"]);
                    string 计划发车时刻 = Convert.ToString(data.Rows[i]["计划发车时刻"]);
                    string jsysql = "select top 1 workName from WorkCardRecord where 记录日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and CardCode='" + 内部编号 + "' order by 记录时刻";
                    DataTable jsysqldata = SqlHelper.ExecuteDataTable(jsysql);
                    if (jsysqldata.Rows.Count > 0)
                    {
                        string 第四步 = "update tblRouteRFormD " +
                      "                     set 驾驶员 = '" + Convert.ToString(jsysqldata.Rows[0]["workName"]) + "'" +
                      "             where  内部编号='" + 内部编号 + "' and 计划发车时刻='" + 计划发车时刻 + "'";
                        SqlHelper.ExuteNonQuery(第四步);
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("错误信息" + ex.ToString());
                }
            }
        }


        /// <summary>
        /// 处理非营运路单
        /// </summary>
        private void ProcessNonFormD()
        {
            if (_NonFormDCCdateTime != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                if (DateTime.Now.Hour > 2)
                {
                    string deleteSql = @"delete from  tblRouteRFormD  where 日期=CONVERT(varchar(10),GETDATE(),121) and 备注=1 and 起点='出场'";
                    SqlHelper.ExuteNonQuery(deleteSql);
                    string sql = @"INSERT INTO [dbo].[tblRouteRFormD]
                          ([日期],[路单编号],[线路ID],[内部编号],[起点],[终点],[实际发车时刻],[实际到达时刻],[单程里程],[备注],[状态])
                          select CONVERT(varchar(10),GETDATE(),121),'',线路名称,内部编号,'出场','出场','','',非营运里程,1,2 from tblNonOperationBaseData main
                          where 非营运类型 = 1 and  exists(select * from  tbldailyplan1 where  内部编号=main.内部编号 and ActualRoute=main.线路名称)";
                    SqlHelper.ExuteNonQuery(sql);

                    string selectSql = @"select  *  from  tblRouteRFormD  where 日期=CONVERT(varchar(10),GETDATE(),121) and 备注=1 and 起点='出场'";

                    DataTable rFormData = SqlHelper.ExecuteDataTable(selectSql);
                    foreach (DataRow row in rFormData.Rows)
                    {

                        string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + Convert.ToString(row["内部编号"]) + "' and 线路名称='" + Convert.ToString(row["线路id"]) + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                        DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                        if (sqlFromData.Rows.Count > 0)
                        {
                            string 更新路牌语句 = "update tblRouteRFormD " +
                                 "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(更新路牌语句);
                        }

                    }
                    _NonFormDCCdateTime = DateTime.Now.ToString("yyyy-MM-dd");
                }
            }



            if (_NonFormDJCdateTime != DateTime.Now.ToString("yyyy-MM-dd"))
            {
                if (DateTime.Now.Hour >= 22)
                {
                    string deleteSql = @"delete from  tblRouteRFormD  where 日期=CONVERT(varchar(10),GETDATE(),121) and 备注=1 and 起点='进场'";
                    SqlHelper.ExuteNonQuery(deleteSql);
                    string sql = @"INSERT INTO [dbo].[tblRouteRFormD]
                          ([日期],[路单编号],[线路ID],[内部编号],[起点],[终点],[实际发车时刻],[实际到达时刻],[单程里程],[备注],[状态])
                          select CONVERT(varchar(10),GETDATE(),121),'',线路名称,内部编号,'进场','进场','','',非营运里程,1,2 from tblNonOperationBaseData main 
                          where 非营运类型 = 1  and  exists(select * from  tbldailyplan1 where  内部编号=main.内部编号 and ActualRoute=main.线路名称)";
                    SqlHelper.ExuteNonQuery(sql);

                    string selectSql = @"select  *  from  tblRouteRFormD  where 日期=CONVERT(varchar(10),GETDATE(),121) and 备注=1 and 起点='进场'";

                    DataTable rFormData = SqlHelper.ExecuteDataTable(selectSql);
                    foreach (DataRow row in rFormData.Rows)
                    {

                        string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + Convert.ToString(row["内部编号"]) + "' and 线路名称='" + Convert.ToString(row["线路id"]) + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                        DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                        if (sqlFromData.Rows.Count > 0)
                        {
                            string 更新路牌语句 = "update tblRouteRFormD " +
                                 "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(更新路牌语句);
                        }

                    }


                    _NonFormDJCdateTime = DateTime.Now.ToString("yyyy-MM-dd");
                }

            }


            string selectFormD = "select 流水号,备注,线路id,内部编号,ISNULL(状态,0)  状态 from tblRouteRFormD where 备注!=255 and 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "'  and   ISNULL(状态,0)!=2";
            var FormDTable = SqlHelper.ExecuteDataTable(selectFormD);
            string typeId = string.Empty;
            foreach (DataRow row in FormDTable.Rows)
            {
                try
                {
                    string name = string.Empty;
                    string type = Convert.ToString(row["备注"]);
                    switch (type)
                    {
                        case "2":
                            name = "请求加油/充气";
                            typeId = "2";
                            break;
                        case "3":
                            name = "车辆抛锚拖车";
                            typeId = "3";
                            break;
                        case "4":
                            name = "车辆抛锚非拖车";
                            typeId = "3";
                            break;
                        case "5":
                            name = "测试";
                            typeId = "4";
                            break;
                        case "6":
                            name = "修车";
                            typeId = "5";
                            break;
                        case "7":
                            name = "事故";
                            typeId = "6";
                            break;

                    }
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        string nonLcSql = "select 非营运里程 from  tblNonOperationBaseData where 线路名称='" + Convert.ToString(row["线路id"]) + "' and 内部编号='" + Convert.ToString(row["内部编号"]) + "'  and 非营运类型='" + type + "'";
                        var dataTable = SqlHelper.ExecuteDataTable(nonLcSql);
                        if (dataTable.Rows.Count > 0)
                        {

                            string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + Convert.ToString(row["内部编号"]) + "' and 线路名称='" + Convert.ToString(row["线路id"]) + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                            DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                            if (sqlFromData.Rows.Count > 0)
                            {
                                string 更新路牌语句 = "update tblRouteRFormD " +
                                     "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                                SqlHelper.ExuteNonQuery(更新路牌语句);
                            }

                            string updateSql = "update tblRouteRFormD set 起点='" + name + "',终点='" + name + "',备注='" + typeId + "',状态='2',单程里程='" + Convert.ToString(dataTable.Rows[0]["非营运里程"]) + "'   where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(updateSql);
                        }
                        else
                        {
                            string busStatusselect = "select 当前里程表数 from  tblRouteBusStatus where 内部编号='" + Convert.ToString(row["内部编号"]) + "'";
                            DataTable dataBusStatusselect = SqlHelper.ExecuteDataTable(busStatusselect);
                            if (dataBusStatusselect.Rows.Count > 0)
                            {

                                if (Convert.ToString(row["状态"]).Equals("3"))
                                {
                                    string updateSql = "update tblRouteRFormD set 起点='" + name + "',终点='" + name + "',备注='" + typeId + "',单程里程=" + Convert.ToString(dataBusStatusselect.Rows[0]["当前里程表数"]) + "-起点里程表数,状态='2',终点里程表数='" + Convert.ToString(dataBusStatusselect.Rows[0]["当前里程表数"]) + "'     where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                                    SqlHelper.ExuteNonQuery(updateSql);

                                }
                                if (Convert.ToString(row["状态"]).Equals("0"))
                                {
                                    string sqlss = "select  top 1 流水号 from tblRouteRFormD where 内部编号='" + Convert.ToString(row["内部编号"]) + "' and 流水号<" + Convert.ToString(row["流水号"]) + " and 日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 备注='255'   and ISNULL(状态,0)!=2   order by 流水号 desc";
                                    DataTable dxxx = SqlHelper.ExecuteDataTable(sqlss);
                                    if (dxxx.Rows.Count > 0)
                                    {
                                        string updateSqlxxx = "update tblRouteRFormD set    单程里程=" + Convert.ToString(dataBusStatusselect.Rows[0]["当前里程表数"]) + "-起点里程表数,状态='2',终点里程表数='" + Convert.ToString(dataBusStatusselect.Rows[0]["当前里程表数"]) + "'  where  流水号='" + Convert.ToString(dxxx.Rows[0]["流水号"]) + "'";
                                        SqlHelper.ExuteNonQuery(updateSqlxxx);
                                    }

                                    string updateSql = "update tblRouteRFormD set 起点='" + name + "',终点='" + name + "',备注='" + typeId + "',起点里程表数='" + Convert.ToString(dataBusStatusselect.Rows[0]["当前里程表数"]) + "',状态='1'  where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                                    SqlHelper.ExuteNonQuery(updateSql);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("错误信息" + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessLpData()
        {
            string sql = "select 线路ID from tblRouteRFormD  where 日期 ='" + DateTime.Now.ToString("yyyy-MM-dd") + "'   group by 线路ID";

            var routeData = SqlHelper.ExecuteDataTable(sql);
            ParallelLoopResult result = Parallel.ForEach(routeData.AsEnumerable(), (baseRow, pls, longs) =>
            {

                sql = "select  流水号,CONVERT(varchar(10),日期,120) 日期,线路ID,内部编号,起点,终点,方向,CONVERT(varchar,计划发车时刻,108) 计划发,CONVERT(varchar,实际发车时刻,108) 实际发,驾驶员,ISNULL(状态,0) 状态  from  [tblRouteRFormD] " +
                                                                     "   where   [线路ID]='" + Convert.ToString(baseRow["线路ID"]) + @"' and  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 路牌=0";
                var dt = SqlHelper.ExecuteDataTable(sql);

                foreach (DataRow row in dt.Rows)
                {
                    try
                    {
                        string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + Convert.ToString(row["内部编号"]) + "' and 线路名称='" + Convert.ToString(row["线路ID"]) + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                        DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                        if (sqlFromData.Rows.Count > 0)
                        {
                            string 更新路牌语句 = "update tblRouteRFormD " +
                                 "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(row["流水号"]) + "'";
                            SqlHelper.ExuteNonQuery(更新路牌语句);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.WriteLog("错误信息" + ex.ToString() + ",流水号:" + Convert.ToString(row["流水号"]));
                    }
                }


            });
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void ProcessData()
        {
            try
            {
                string 空调 = "0";

                if (DateTime.Now.Month >= 5 && DateTime.Now.Month <= 9)
                {
                    空调 = "1";
                }
                string sql = "select 线路ID from tblRouteRFormD  where 日期 ='" + DateTime.Now.ToString("yyyy-MM-dd") + "'   group by 线路ID";

                var routeData = SqlHelper.ExecuteDataTable(sql);
                ParallelLoopResult result = Parallel.ForEach(routeData.AsEnumerable(), (baseRow, pls, longs) =>
                {
                    //foreach (DataRow baseRow in routeData.Rows)
                    //{
                    string 线路ID = Convert.ToString(baseRow["线路ID"]);
                    sql = "select  流水号,CONVERT(varchar(10),日期,120) 日期,线路ID,内部编号,起点,终点,方向,CONVERT(varchar,计划发车时刻,108) 计划发,CONVERT(varchar,实际发车时刻,108) 实际发,驾驶员,ISNULL(状态,0) 状态  from  [tblRouteRFormD] " +
                                                         "   where ISNULL(状态,0)!=2 and  [线路ID]='" + Convert.ToString(baseRow["线路ID"]) + @"' and  日期='" + DateTime.Now.ToString("yyyy-MM-dd") + "' and 备注='255'  ";
                    var dt = SqlHelper.ExecuteDataTable(sql);

                    foreach (DataRow formR in dt.Rows)
                    {
                        R("线路ID:" + Convert.ToString(baseRow["线路ID"]) + "\n\r" + "  流水号:" + Convert.ToString(formR["流水号"]));
                        try
                        {


                            string 内部编号 = Convert.ToString(formR["内部编号"]);

                            string 方向 = Convert.ToString(formR["方向"]);
                            string 计划发 = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), Convert.ToString(formR["计划发"]));
                            string 实际发 = string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), Convert.ToString(formR["实际发"]));
                            string 第一步 = @"select  isnull(min(流水号),0) 流水号 from tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + @"
                                              where 内部编号 = '" + 内部编号 + @"'
                                              and 驶离时刻>'" + 计划发 + @"'
                                              and 方向='" + Convert.ToString(formR["方向"]) + @"'
                                              and 线路ID = '" + 线路ID + "'  order by 流水号";

                            DataTable table = SqlHelper.ExecuteDataTable(第一步);

                            var rRgs = _routeSRangeS.Where(t => t.线路 == 线路ID && t.类型.Contains(方向)).ToList();
                            if (rRgs.Count == 0)
                            {
                                continue;
                            }
                            if (table.Rows.Count > 0)
                            {
                                string cs = 方向 + "始发";
                                string 流水号 = Convert.ToString(table.Rows[0]["流水号"]);
                                var zs = _routeSRangeS.FirstOrDefault(t => t.线路 == 线路ID && t.类型 == cs);
                                string zd = 方向 + "终点";
                                var zds = _routeSRangeS.FirstOrDefault(t => t.线路 == 线路ID && t.类型 == zd);

                                if (Convert.ToString(formR["状态"]).Equals("0"))
                                {
                                    string 第二步 = @"select top 20  convert(varchar(19), 到达时刻,121) 到达时刻,convert(varchar(19), 驶离时刻,121) 驶离时刻,站序  
                                              from tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + @"
                                              where 内部编号 = '" + 内部编号 + @"'
                                              and 流水号<='" + 流水号 + @"'
                                              and 线路ID = '" + 线路ID + "'  order by 流水号 desc ";
                                    DataTable rTable = SqlHelper.ExecuteDataTable(第二步);
                                    for (int i = 0; i < rTable.Rows.Count; i++)
                                    {
                                        try
                                        {
                                            if (Convert.ToInt32(rTable.Rows[i]["站序"]) == zs.线路内序号 || Convert.ToInt32(rTable.Rows[i]["站序"]) == zds.线路内序号)
                                            {

                                                string _dailySql = "select  流水号,convert(varchar(8), 计划到达时间,108)  AS 计划到达时间 from tbldailyplan1 where CONVERT(varchar,isnull(调整时刻,计划发车时间),108)='" + Convert.ToString(formR["计划发"]) + "' and 替换车号='" + Convert.ToString(formR["内部编号"]) + "' and 方向='" + Convert.ToString(formR["方向"]) + "'";
                                                var dailyTable = SqlHelper.ExecuteDataTable(_dailySql);

                                                if (dailyTable.Rows.Count > 0)
                                                {



                                                    DateTime 计划到达时间 = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), Convert.ToString(dailyTable.Rows[0]["计划到达时间"])));

                                                    if (Convert.ToDateTime(rTable.Rows[i]["驶离时刻"]) > 计划到达时间)
                                                    {
                                                        continue;
                                                    }


                                                    string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + 内部编号 + "' and 线路名称='" + 线路ID + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                                                    DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                                                    if (sqlFromData.Rows.Count > 0)
                                                    {
                                                        string 更新路牌语句 = "update tblRouteRFormD " +
                                                             "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                        SqlHelper.ExuteNonQuery(更新路牌语句);
                                                    }

                                                    string 第四步 = "update tblRouteRFormD " +
                                                "                     set 实际发车时刻 = '" + Convert.ToString(rTable.Rows[i]["驶离时刻"]) + "'," +
                                                "                         状态=1,计划到达时刻='" + 计划到达时间.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                                "                         趟次='" + 空调 + "'" +
                                                "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                    SqlHelper.ExuteNonQuery(第四步);
                                                    break;
                                                }

                                            }
                                            if (Convert.ToInt32(rTable.Rows[i]["站序"]) < zs.线路内序号 || Convert.ToInt32(rTable.Rows[i]["站序"]) > zds.线路内序号)
                                            {
                                                string _dailySql = "select  流水号,convert(varchar(8), 计划到达时间,108)  AS 计划到达时间 from tbldailyplan1 where convert(varchar, 计划发车时间,108)='" + Convert.ToString(formR["计划发"]) + "' and 替换车号='" + Convert.ToString(formR["内部编号"]) + "' and 方向='" + Convert.ToString(formR["方向"]) + "'";
                                                var dailyTable = SqlHelper.ExecuteDataTable(_dailySql);
                                                if (dailyTable.Rows.Count > 0)
                                                {
                                                    DateTime 计划到达时间 = Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), Convert.ToString(dailyTable.Rows[0]["计划到达时间"])));
                                                    if (i > 0)
                                                    {

                                                        if (Convert.ToDateTime(rTable.Rows[i - 1]["驶离时刻"]) > 计划到达时间)
                                                        {
                                                            continue;
                                                        }

                                                        string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + 内部编号 + "' and 线路名称='" + 线路ID + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                                                        DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                                                        if (sqlFromData.Rows.Count > 0)
                                                        {
                                                            string 更新路牌语句 = "update tblRouteRFormD " +
                                                                 "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                            SqlHelper.ExuteNonQuery(更新路牌语句);
                                                        }

                                                        string 第四步 = "update tblRouteRFormD " +
                                              "                     set 实际发车时刻 = '" + Convert.ToString(rTable.Rows[i - 1]["驶离时刻"]) + "'," +
                                              "                         状态=1,计划到达时刻='" + 计划到达时间.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                              "                         趟次='" + 空调 + "'" +
                                              "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                        SqlHelper.ExuteNonQuery(第四步);



                                                    }
                                                    else
                                                    {
                                                        if (Convert.ToDateTime(rTable.Rows[i]["驶离时刻"]) > 计划到达时间)
                                                        {
                                                            continue;
                                                        }
                                                        string sqlfromsql = "select Day_" + DateTime.Now.Day + " as lp from [dbo].[Table_DriverSchedule] where 车辆编号='" + 内部编号 + "' and 线路名称='" + 线路ID + "' and 月份='" + DateTime.Now.ToString("yyyy-MM") + "'";

                                                        DataTable sqlFromData = SqlHelper.ExecuteDataTable(sqlfromsql);

                                                        if (sqlFromData.Rows.Count > 0)
                                                        {
                                                            string 更新路牌语句 = "update tblRouteRFormD " +
                                                                 "   set 路牌 = '" + Convert.ToString(sqlFromData.Rows[0]["lp"]) + @"'   where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                            SqlHelper.ExuteNonQuery(更新路牌语句);
                                                        }

                                                        string 第四步 = "update tblRouteRFormD " +
                                                    "                     set 实际发车时刻 = '" + Convert.ToString(rTable.Rows[i]["驶离时刻"]) + "'," +
                                                    "                         状态=1,计划到达时刻='" + 计划到达时间.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                                    "                         趟次='" + 空调 + "'" +
                                                    "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                        SqlHelper.ExuteNonQuery(第四步);
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //E(ex.ToString());
                                            var esss = ex;
                                            LogHelper.WriteLog("错误信息" + ex.ToString() + ",流水号:" + Convert.ToString(formR["流水号"]));
                                        }
                                    }
                                }
                                else if (Convert.ToString(formR["状态"]).Equals("1"))
                                {
                                    string plansql = "select top 1 CONVERT(varchar,isnull(调整时刻,计划发车时间),108)  as 计划发车时间,CONVERT(varchar(8), 计划到达时间,108) as 计划到达时间 from tbldailyplan1 where 替换车号='" + 内部编号 + @"' and ActualRoute='" + 线路ID + "' order by 计划发车时间 desc";

                                    ///最后班次
                                    DataTable lastplans = SqlHelper.ExecuteDataTable(plansql);
                                    if (lastplans.Rows.Count > 0)
                                    {
                                        if (Convert.ToString(formR["计划发"]).Equals(lastplans.Rows[0]["计划发车时间"]))
                                        {
                                            string 第四步 = @"select top 150   convert(varchar(19), 到达时刻,121) 到达时刻,convert(varchar(19), 驶离时刻,121) 驶离时刻,站序 
                                              from tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + @"
                                              where 内部编号 = '" + 内部编号 + @"'
                                              and 流水号>'" + 流水号 + @"'
                                              and 线路ID = '" + 线路ID + "'  order by 流水号 ";
                                            DataTable rt = SqlHelper.ExecuteDataTable(第四步);
                                            for (int i = 0; i < rt.Rows.Count; i++)
                                            {
                                                try
                                                {
                                                    var fs = _routeS.FirstOrDefault(t => t.线路 == 线路ID && t.方向 == 方向);
                                                    if (Convert.ToInt32(rt.Rows[i]["站序"]) == zs.线路内序号 || Convert.ToInt32(rt.Rows[i]["站序"]) == zds.线路内序号)
                                                    {
                                                        string 第五步 = "update tblRouteRFormD " +
                                             "                     set 实际到达时刻 = '" + Convert.ToDateTime(rt.Rows[i]["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                             "                         状态=2,单程里程='" + fs.长度 + "'" +
                                             "          where  流水号='" + Convert.ToString(formR["流水号"]) + "'";

                                                        SqlHelper.ExuteNonQuery(第五步);
                                                    }
                                                    else
                                                    {
                                                        string sqlxx = "select CONVERT(varchar, 时刻,108) sk from tblCommData" + DateTime.Now.ToString("yyyyMMdd") + @" where SRCID='" + 内部编号 + @"' and 命令字=0x42 and cast(CONVERT(varchar, 时刻,108) as time)>dateadd(minute,1,cast('" + lastplans.Rows[0]["计划发车时间"] + "' as time)) and cast(CONVERT(varchar, 时刻,108) as time)>dateadd(minute,1,cast('" + 实际发 + "' as time)) and stationidx in('" + zs.线路内序号 + "','" + zds.线路内序号 + "')";
                                                        DataTable skt = SqlHelper.ExecuteDataTable(sqlxx);
                                                        if (skt.Rows.Count > 0)
                                                        {
                                                            string 第五步 = "update tblRouteRFormD " +
                                          "                     set 实际到达时刻 = '" + Convert.ToDateTime(string.Format("{0} {1}", DateTime.Now.ToString("yyyy-MM-dd"), skt.Rows[0]["sk"])) + "'," +
                                          "                         状态=2,单程里程='" + fs.长度 + "'" +
                                          "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                            SqlHelper.ExuteNonQuery(第五步);
                                                        }


                                                    }

                                                }
                                                catch (Exception ex)
                                                {
                                                    LogHelper.WriteLog("错误信息" + ex.ToString() + ",流水号:" + Convert.ToString(formR["流水号"]));
                                                }
                                            }

                                            if (rt.Rows.Count > 0)
                                            {

                                            }


                                            continue;
                                        }

                                    }

                                    string 第二步 = @"select top 150   convert(varchar(19), 到达时刻,121) 到达时刻,convert(varchar(19), 驶离时刻,121) 驶离时刻,站序 
                                              from tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + @"
                                              where 内部编号 = '" + 内部编号 + @"'
                                              and 流水号>'" + 流水号 + @"'
                                              and 线路ID = '" + 线路ID + "'  order by 流水号 ";
                                    DataTable rTable = SqlHelper.ExecuteDataTable(第二步);
                                    for (int i = 0; i < rTable.Rows.Count; i++)
                                    {
                                        try
                                        {
                                            if (Convert.ToInt32(rTable.Rows[i]["站序"]) == zs.线路内序号 || Convert.ToInt32(rTable.Rows[i]["站序"]) == zds.线路内序号)
                                            {
                                                var fs = _routeS.FirstOrDefault(t => t.线路 == 线路ID && t.方向 == 方向);
                                                string 第四步 = "update tblRouteRFormD " +
                                                "                     set 实际到达时刻 = '" + Convert.ToDateTime(rTable.Rows[i]["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                                "                         状态=2,单程里程='" + fs.长度 + "'" +
                                       "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                SqlHelper.ExuteNonQuery(第四步);
                                            }
                                            else if (Convert.ToInt32(rTable.Rows[i]["站序"]) < zs.线路内序号 || Convert.ToInt32(rTable.Rows[i]["站序"]) > zds.线路内序号)
                                            {
                                                if (i == 0)
                                                {
                                                    var fs = _routeS.FirstOrDefault(t => t.线路 == 线路ID && t.方向 == 方向);
                                                    string 第四步 = "update tblRouteRFormD " +
                                                    "                     set 实际到达时刻 = '" + Convert.ToDateTime(rTable.Rows[i]["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                                    "                         状态=2,单程里程='" + fs.长度 + "'" +
                                           "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                    SqlHelper.ExuteNonQuery(第四步);
                                                }
                                                else
                                                {
                                                    var fs = _routeS.FirstOrDefault(t => t.线路 == 线路ID && t.方向 == 方向);
                                                    string 第四步 = "update tblRouteRFormD " +
                                                    "                     set 实际到达时刻 = '" + Convert.ToDateTime(rTable.Rows[i - 1]["到达时刻"]).ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                                                    "                         状态=2,单程里程='" + fs.长度 + "'" +
                                           "             where  流水号='" + Convert.ToString(formR["流水号"]) + "'";
                                                    SqlHelper.ExuteNonQuery(第四步);
                                                }

                                                break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            LogHelper.WriteLog("错误信息" + ex.ToString() + ",流水号:" + Convert.ToString(formR["流水号"]));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //E(ex.ToString());
                            //var esss = ex;
                            LogHelper.WriteLog("错误信息" + ex.ToString() + ",流水号:" + Convert.ToString(formR["流水号"]));
                        }
                    }
                    //}


                });
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("错误信息" + ex.ToString());

            }
        }

        /// <summary>
        /// 日期
        /// </summary>
        private DateTime _dateTime = new DateTime();


        private DateTime _cTime = new DateTime();
        public class Route
        {
            public string 线路 { get; set; }

            public string 方向 { get; set; }

            public string 长度 { get; set; }
        }

        public class RouteSRange
        {
            public string 线路 { get; set; }
            public int 线路内序号 { get; set; }
            public string 类型 { get; set; }

            public string 站名 { get; set; }
        }

        public List<RouteSRange> _routeSRangeS = new List<RouteSRange>();

        public List<Route> _routeS = new List<Route>();
        private bool isexcute = true;
        private void Form1_Load(object sender, EventArgs e)
        {

            E = new ControlAddEx(AddR);

            R = new ControlAddR(AddR);

            _routeS = (from p in SqlHelper.ExecuteDataTable(@"select 线路ID,'上行' as 方向,ISNULL(CAST(上行长度 as varchar),'0')  as 长度 from tblRoute  
                                                                  union all 
                                                                 select 线路ID,'下行' as 方向,ISNULL(CAST(下行长度 as varchar),'0')  as 长度 from tblRoute    ").AsEnumerable()
                       select new Route
                       {
                           线路 = p.Field<string>("线路ID"),
                           方向 = p.Field<string>("方向"),
                           长度 = p.Field<string>("长度")
                       }).ToList();


            var data = SqlHelper.ExecuteDataTable(@"select 线路id,线路内序号,'上行始发'  as 类型,站名  from  tblrouted  where 上行始发=1
                                                                  union all
                                                   select 线路id, 线路内序号,'上行终点'  as 类型,站名   from tblrouted  where 上行终点 = 1
                                                                  union all
                                                   select 线路id, 线路内序号,'下行始发'  as 类型,站名   from tblrouted  where 下行始发 = 1
                                                                   union all
                                                   select 线路id, 线路内序号,'下行终点'  as 类型,站名   from tblrouted  where 下行终点 = 1");

            foreach (DataRow row in data.Rows)
            {
                _routeSRangeS.Add(new RouteSRange
                {
                    线路 = Convert.ToString(row["线路id"]),
                    线路内序号 = Convert.ToInt32(row["线路内序号"]),
                    类型 = Convert.ToString(row["类型"]),
                    站名 = Convert.ToString(row["站名"]),
                });
            }


            ///处理非营运路单
            Thread threadNonFormD = new Thread(() =>
            {
                while (true)
                {
                    ProcessNonFormD();
                    Thread.Sleep(10000);
                }
            });
            threadNonFormD.IsBackground = true;
            threadNonFormD.Start();

            ///处理路牌
            Thread threadLp = new Thread(() =>
             {
                 while (true)
                 {
                     ProcessLpData();
                     Thread.Sleep(10000);
                 }
             });
            threadLp.IsBackground = true;
            threadLp.Start();


            ///更新 实际发，实际到，单边里程
            Thread Formd = new Thread(() =>
            {
                while (true)
                {
                    ProcessData();
                    Thread.Sleep(50000);
                }
            });
            Formd.IsBackground = true;
            Formd.Start();

            DateTime excuteTime = DateTime.Now.AddDays(-1).Date;

            ///处理 ,最后路单的实际到
            //Thread threadLastForm = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        if (!excuteTime.Equals(DateTime.Now.Date))
            //        {
            //            if (DateTime.Now.Hour > 4)
            //            {
            //                isexcute = true;
            //            }
            //        }
            //        if (isexcute)
            //        {
            //            string sql = "select 替换车号,ActualRoute,convert(varchar,max(isnull(调整时刻,计划发车时间)),108) 发车时间 from tbldailyplan1_bak where 日期=convert(varchar(10),dateadd(day,-1,getdate()),121) group by 替换车号,ActualRoute";
            //            DataTable data1 = SqlHelper.ExecuteDataTable(sql);
            //            if (data1.Rows.Count > 0)
            //            {
            //                foreach (DataRow row in data1.Rows)
            //                {
            //                    try
            //                    {
            //                        string rformdsql = "select 流水号,状态 from tblRouteRFormD where 线路id='" + Convert.ToString(row["ActualRoute"]) + "' and 内部编号='" + Convert.ToString(row["替换车号"]) + "' and convert(varchar, 计划发车时刻,108)='" + Convert.ToString(row["发车时间"]) + "'  and 日期=convert(varchar(10),dateadd(day,-1,getdate()),121)";
            //                        DataTable formD = SqlHelper.ExecuteDataTable(rformdsql);
            //                        if (formD.Rows.Count > 0)
            //                        {
            //                            if (!formD.Rows[0]["状态"].ToString().Equals("2"))
            //                            {
            //                                string formSql = "select top 1 CONVERT(varchar(19), 到达时刻,121) 到达时刻 from tblRouteRFormX" + DateTime.Now.ToString("yyyyMMdd") + "  where 内部编号='" + Convert.ToString(row["替换车号"]) + "' and 线路id='" + Convert.ToString(row["ActualRoute"]) + "'      order by 流水号 desc";
            //                                DataTable rFormXData = SqlHelper.ExecuteDataTable(formSql);
            //                                if (rFormXData.Rows.Count > 0)
            //                                {
            //                                    string updateSql = "update  tblRouteRFormD set 实际到达时刻='" + Convert.ToString(rFormXData.Rows[0]["到达时刻"]) + "' where 流水号=" + Convert.ToString(formD.Rows[0]["流水号"]);
            //                                    SqlHelper.ExuteNonQuery(updateSql);
            //                                }
            //                            }
            //                        }
            //                    }
            //                    catch (Exception ex)
            //                    {

            //                    }
            //                }
            //            }
            //            isexcute = false;
            //            excuteTime = DateTime.Now.Date;
            //        }
            //        Thread.Sleep(100000);
            //    }
            //});
            //threadLastForm.IsBackground = true;
            //threadLastForm.Start();


            _dateTime = DateTime.Now;

            ///更新 ,第一班路单驾驶员信息
            Thread FormDUpdateJsy = new Thread(() =>
            {
                while (true)
                {
                    RunJsy();
                    Thread.Sleep(100000);
                }
            });

            FormDUpdateJsy.IsBackground = true;
            FormDUpdateJsy.Start();


            string maxsql = "select convert(varchar(10), dateadd( DAY,1,max(日期)),21) 日期  from   tblDailyReport";
            _cTime = Convert.ToDateTime(SqlHelper.ExecuteDataTable(maxsql).Rows[0]["日期"].ToString());


            ///备份大日报
            Thread DailyReportBak = new Thread(() =>
            {
                while (true)
                {
                    if (_cTime.Date < DateTime.Now.Date)
                        BuildDailyReport();
                    Thread.Sleep(10000);
                }
            });

            DailyReportBak.IsBackground = true;
            DailyReportBak.Start();


        }



        /// <summary>
        /// 生成大日报
        /// </summary>
        public void BuildDailyReport()
        {
            try
            {
                string sql = "select 线路id from tblRoute ";
                var routeData = SqlHelper.ExecuteDataTable(sql);
                foreach (DataRow row in routeData.Rows)
                {
                    sql = @"insert into tblDailyReport
                     select * from ( 
                      SELECT *, ROW_NUMBER() OVER(ORDER BY 车次, 计划发车时刻) AS num from(
                      SELECT  CONVERT(varchar(10), trd.日期, 120) as 日期,
                        ISNULL((select top 1 workcard from  WorkCardRecord where CardCode=trd.内部编号  AND 记录日期=trd.日期       and WorkName=trd.驾驶员),'')      AS 员工号,
                         ISNULL(tr.线路名称, '')   AS 线路,
                         trd.路牌 as 车次,  
		                 dbo.carTrip(trd.线路ID, trd.内部编号, trd.日期, trd.计划发车时刻) as 趟次,  
	                     ISNULL(bus.自编号, '') as 车号,    
		                 ISNULL(bus.车牌号码, '') as 车牌号码,    
		                 ISNULL(trd.驾驶员, '') as 驾驶员,       
		                 trd.计划发车时刻 ,    
		                 ISNULL(trd.方向, '') as 任务属性,   
		 	             (select 站名 from tblRouteD where  线路ID = trd.线路ID and 线路内序号 =
                         (case trd.方向 when '上行' then tr.上行始发线内站序
		                  else  tr.下行始发线内站序 end)) as 始发站,  
		 	             (select 站名 from tblRouteD where  线路ID = trd.线路ID and 线路内序号 =
                         (case trd.方向 when '上行' then tr.上行终点线内站序
		                 else  tr.下行终点线内站序 end)) 终点站,  
		                 CONVERT(varchar(10), trd.计划发车时刻, 108) as 计划发车,  
		                 '0' as 调整时刻,  
		                 CONVERT(varchar(10), trd.实际发车时刻, 108) as 实际发车,  
		                 datediff(minute, trd.实际发车时刻, 计划发车时刻) as 发车延时,  
		                 CONVERT(varchar(10), trd.实际到达时刻, 108) as 实际到达, 
		                (case trd.备注 when '255' then '运营' else '非运营' end) as 里程属性, 
		                isnull(trd.描述, '')  备注,  
		                 (case trd.备注 when '255' then  (case trd.方向 when '下行'
                        then tr.下行长度  else tr.上行长度 end)  else 0  end)   as 计划里程, 
		                (case trd.备注 when '255' then ISNULL(trd.单程里程 ,0)   else 0  end)    as GPS里程, 
		                ROUND(case  datediff(minute, trd.实际发车时刻, trd.实际到达时刻) when 0 then 0  else   ROUND(cast((case trd.备注    when '255' then ISNULL(trd.单程里程, 0)   else 0  end) as float) / cast(datediff(minute, trd.实际发车时刻, trd.实际到达时刻) as float), 2) * cast(60 as float)  end,2)  平均车速,
	 	         (case trd.备注 when '1' then ISNULL(trd.单程里程 ,0)      else   0 end) as 进出场, 
       	         (case trd.备注 when '2' then ISNULL(trd.单程里程 ,0)     else   0 end) as 加油,  
	   	         (case trd.备注 when '3' then ISNULL(trd.单程里程 ,0)     else   0 end) as 抛锚, 
	   	         (case trd.备注 when '4' then ISNULL(trd.单程里程 ,0)     else   0 end) as 车辆测试, 
	   	         (case trd.备注 when '5' then ISNULL(trd.单程里程 ,0)       else   0 end) as 修车, 
	   	         (case trd.备注 when '6' then ISNULL(trd.单程里程 ,0)      else   0 end) as 事故, 
	   	         (case trd.备注 when '255'  then  0  else ISNULL(trd.单程里程, 0)  end)   as 非营运里程  , 
		         isnull(CONVERT(varchar, CAST(trd.计划到达时刻 as datetime), 108), '') as 计划到达,    
		         ISNULL(datediff(minute, trd.计划发车时刻, trd.计划到达时刻), 0) as 计划单程, 
		         datediff(minute, trd.实际发车时刻, trd.实际到达时刻) as 实际单程, 
		         datediff(minute, trd.实际发车时刻, trd.实际到达时刻) - ISNULL(datediff(minute, trd.计划发车时刻, trd.计划到达时刻), 0) as 准点,trd.线路ID,       
			     (case  趟次 when '1' then '空调' else '' end ) as 空调,
                 trd.内部编号
                 from tblRouteRFormD trd
                 left  join tblRoute tr    on trd.线路ID = tr.线路ID
                 left  join tblroutebus bus   on trd.内部编号 = bus.内部编号
                 where trd.线路id = '" + Convert.ToString(row["线路id"]) + @"' and trd.日期='" + _cTime.ToString("yyyy-MM-dd") + @"'
				 ) d ) d";
                    SqlHelper.ExuteNonQuery(sql);
                }
                _cTime = _cTime.AddDays(1);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("异常:" + ex.Message + "  sql:" + ex.Message + " _dateTime:" + _dateTime.ToString("yyyy-MM-dd") + "  now" + DateTime.Now.ToString("yyyy-MM-dd"));
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
