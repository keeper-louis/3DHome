using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.K3.MFG.ServiceHelper.ENG;
using Kingdee.BOS.App.Data;
using Kingdee.K3.Core.MFG.EnumConst;
using Kingdee.BOS.App;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Metadata;

namespace Keeper_Louis.K3.Sal.ConvertPlugIn
{
    [Description("预订单转销售订单BOM版本赋值")]
    public class YuOrderToSaleOrder: AbstractConvertPlugIn
    {
        public override void AfterConvert(AfterConvertEventArgs e)
        {
            base.AfterConvert(e);
            ExtendedDataEntity[] array = e.Result.FindByEntityKey("FBillHead");
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();
            FormMetadata bomMeta = metaService.Load(this.Context, "ENG_BOM", true) as FormMetadata;
            IViewService viewService = ServiceHelper.GetService<IViewService>();
            foreach (ExtendedDataEntity item in array)
            {
                long orgId = Convert.ToInt64(item["SaleOrgId_Id"]);
                //long orgId = 100005;
                DynamicObjectCollection orderEntry = item["SaleOrderEntry"] as DynamicObjectCollection;
                if (orderEntry!=null&& orderEntry.Count()>0)
                {
                    foreach (DynamicObject order in orderEntry)
                    {
                        long materialId = Convert.ToInt64(order["MaterialId_Id"]);
                        long AuxPropId_Id = Convert.ToInt64(order["AuxPropId_Id"]);
                        string strSql = string.Format(@"/*dialect*/select FMASTERID from T_BD_MATERIAL where FMATERIALID = {0}",materialId);
                        long masterid = DBUtils.ExecuteScalar<long>(this.Context,strSql,0,null);
                        if (masterid!=0)
                        {

                            //1上下文，2物料masterid,3辅助属性内码id，
                            long bomId = BOMServiceHelper.GetDefaultBomKey(this.Context, masterid, orgId, AuxPropId_Id,Enums.Enu_BOMUse.TYBOM)>0? BOMServiceHelper.GetDefaultBomKey(this.Context, masterid, orgId, AuxPropId_Id, Enums.Enu_BOMUse.TYBOM):0;
                            if (bomId>0)
                            {
                                DynamicObject bomObject = viewService.LoadSingle(this.Context, bomId, bomMeta.BusinessInfo.GetDynamicObjectType());
                                order["BomId"] = bomObject;
                                order["BomId_Id"] = bomId;
                            }
                        }
                        
                    }
                }
            }
        }
    }
}
