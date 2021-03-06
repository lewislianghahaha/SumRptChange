using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.FIN.AR.App.Report;

namespace SumRptChange
{
    public class CustomerReportChange : ARSumReportService
    {
        //定义临时表数组
        private string[] _customRptTempDt;

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            //创建临时表,用于存放自已的数据
            var dbservice = ServiceHelper.GetService<IDBService>();
            _customRptTempDt = dbservice.CreateTemporaryTableName(Context, 1);
            var strDt = _customRptTempDt[0];

            //调用基类的方法,获取初步的查询结果赋值到临时表
            base.BuilderReportSqlAndTempTable(filter, strDt);

            //对初步的查询结果进行处理,然后写回基类默认的存放查询结果的临时表
            var strSql = $@"
                             SELECT T1.*,C.FDATAVALUE FDI,E.FDATAVALUE FCountry,F.FCREDITAMOUNT FCREDIT
                             INTO {tableName}
                             FROM {strDt} T1 /*后台‘应收款汇总表’临时表*/
                             INNER JOIN T_BD_CUSTOMER a ON T1.FCONTACTUNITNUMBER=a.FNUMBER
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY B ON A.FPROVINCIAL=B.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L C ON B.FENTRYID=C.FENTRYID AND C.FLOCALEID=2052

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY D ON A.F_YTC_ASSISTANT3=D.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L E ON D.FENTRYID=E.FENTRYID AND E.FLOCALEID=2052   
                            
                             LEFT JOIN  T_CRE_CUSTARCHIVESENTRY F ON A.FCUSTID=F.FOBJECTID              
                         ";
            DBUtils.Execute(Context, strSql);
        }



        /// <summary>
        /// 关闭报表时执行
        /// </summary>
        public override void CloseReport()
        {
            //删除临时表
            if (_customRptTempDt.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }
            var dbService = ServiceHelper.GetService<IDBService>();
            //使用后的临时表删除
            dbService.DeleteTemporaryTableName(Context, _customRptTempDt);

            base.CloseReport();
        }
    }
}
