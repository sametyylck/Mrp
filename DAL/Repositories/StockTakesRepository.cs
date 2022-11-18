using DAL.Contracts;
using DAL.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DAL.DTO.StockAdjusmentDTO;
using static DAL.DTO.StockTakesDTO;

namespace DAL.Repositories
{
    public class StockTakesRepository : IStockTakesRepository
    {
        IDbConnection _db;
        IStockAdjusmentRepository _adjusment;


        public StockTakesRepository(IDbConnection db, IStockAdjusmentRepository adjusment)
        {
            _db = db;
            _adjusment = adjusment;
        }

        public async Task Delete(IdControl T, int CompanyId,int User)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@User", User);
            prm.Add("@Date", DateTime.Now);

            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", false);
           await _db.ExecuteAsync($"Update StockTakes Set IsActive=@IsActive,DeleteDate=@Date,DeletedUser=@User where id = @id and CompanyId = @CompanyId ", prm);
            await _db.ExecuteAsync($"delete from StockTakesItem where StockTakesId=@id and CompanyId=@CompanyId", prm);

        }

        public async Task DeleteItems(StockTakeDelete T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@ItemId", T.ItemId);
            prm.Add("@CompanyId", CompanyId);
          await  _db.ExecuteAsync($"Delete from StockTakesItem where  ItemId=@ItemId and id = @id and CompanyId = @CompanyId ", prm);
        }

        public async Task<IEnumerable<StockTakes>> Detail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sql = $@"select * from StockTakes where CompanyId=@CompanyId and id=@id";
            var details =await _db.QueryAsync<StockTakes>(sql, prm);
            return details.ToList();
        }

        public async Task<int> Insert(StockTakesInsert T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            prm.Add("@StockTake", T.StockTake);
            prm.Add("@CreatedDate", T.CreadtedDate);
            prm.Add("@Reason", T.Reason);
            prm.Add("@Info", T.Info);
            prm.Add("@LocationId", T.LocationId);

            prm.Add("@CompanyId", CompanyId);
            prm.Add("@IsActive", true);
            prm.Add("@Status", 0);

            return await _db.QuerySingleAsync<int>($"Insert into StockTakes (StockTake,CreatedDate,LocationId,Reason,Info,CompanyId,IsActive,Status) OUTPUT INSERTED.[id] values (@StockTake,@CreatedDate,@LocationId,@Reason,@Info,@CompanyId,@IsActive,@Status)", prm);
        }

        public async Task<int> InsertItem(List<StockTakeInsertItems> T, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();

            foreach (var item in T)
            {
                prm.Add("@CompanyId", CompanyId);
                prm.Add("@StockTakesId", item.StockTakesId);
                prm.Add("@ItemId", item.ItemId);
                prm.Add("@Notes", item.Notes);
               await _db.QuerySingleAsync<int>($"Insert into StockTakesItem(ItemId,Note,StockTakesId,CompanyId) OUTPUT INSERTED.[id] values (@ItemId,@Notes,@StockTakesId,@CompanyId)", prm);
            }
            return 1;
        }

        public async Task<IEnumerable<StockTakeItems>> ItemDetail(int CompanyId, int id)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@id", id);
            string sql = $@"select StockTakesItem.id,ItemId,Categories.Name,Note,InStock,StockTakesId,CountedQuantity,Discrepancy,StockTakesItem.CompanyId from StockTakesItem
			left join Items on Items.id=ItemId
			left join Categories on Categories.id=Items.CategoryId
            where StockTakesItem.StockTakesId=@id ";
            var ItemsDetail = await  _db.QueryAsync<StockTakeItems>(sql, prm);
            return ItemsDetail.ToList();
        }

        public async Task<int> StockTakesCount(StockTakeList T, int CompanyId)
        {
            DynamicParameters param = new();
            param.Add("@CompanyId", CompanyId);
            var  kayitsayisi =await _db.QueryFirstAsync<int>($@" select Count(*) as kayitsayisi from(
            select x.* from (
            select StockTakes.id,StockTake,StockTakes.Reason,StockTakes.LocationId,Locations.LocationName,CreatedDate,CompletedDate,StockAdjusmentId,StockAdjusment.Name,Status from StockTakes
            left join Locations on Locations.id=StockTakes.LocationId
			left join StockAdjusment on StockAdjusment.id=StockTakes.StockAdjusmentId
            where StockTakes.CompanyId=@CompanyId and StockTakes.IsActive=1 and StockTakes.Status!=3 AND ISNULL(StockTake,'') like '%{T.StockTake}%' 
			and ISNULL(StockTakes.Reason,'') Like '%{T.Reason}%' AND    ISNULL(LocationName,'') like '%{T.LocationName}%' and
        ISNULL(Name,'') like '%{T.Name}%' and ISNULL(StockTakes.Status,'') like '%{T.Status}%'
		Group by StockTake,StockTakes.Reason,StockTakes.LocationId,StockTakes.id,Locations.LocationName,CreatedDate,CompletedDate,StockAdjusmentId,StockAdjusment.Name,Status)x)kayitsayisi ", param);
            return kayitsayisi;
        }

        public async Task StockTakesDone(StockTakesDone T, int CompanyId,int UserId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@Status", T.Status);

            string sql = $@"select Status from StockTakes where CompanyId={CompanyId} and id={T.id}";
            var Status =await _db.QueryFirstAsync<int>(sql);
            int eskiStatus = Status;
           var Locations =await _db.QueryFirstAsync<int>($"select id from Locations where  CompanyId={CompanyId} and Tip='SettingsLocation'");
            prm.Add("@LocationId", Locations);

            if (eskiStatus == 0)
            {
                if (T.Status == 1)
                {

                    await _db.ExecuteAsync($"Update StockTakes Set Status=@Status where id = {T.id} and CompanyId = {CompanyId}");
                }
            }
            else if (eskiStatus == 1)
            {
                if (T.Status == 2)
                {

                    var degerler =await _db.QueryAsync<StockTakeItems>($@"select * from StockTakesItem where CompanyId={CompanyId} and StockTakesId={T.id}");
                    foreach (var item in degerler)
                    {
                        var Discrepancy = item.CountedQuantity - item.InStock;
                        string sqlquery = $@"Update StockTakesItem Set Discrepancy={Discrepancy}  where id = {item.id} and CompanyId = {CompanyId}";
                         await  _db.ExecuteAsync(sqlquery);
                    }

                  await  _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId} ");

                }
                else if (T.Status == 3)
                {
                    prm.Add("@id", T.id);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@Status", T.Status);
                    string sqlsorgu = $@"select * from StockTakesItem where StockTakesItem.CompanyId={CompanyId} and StockTakesItem.StockTakesId={T.id}";
                    var degerler =await _db.QueryAsync<StockTakeItems>(sqlsorgu, prm);
                    StockAdjusmentInsert stockAdjusmentAll = new StockAdjusmentInsert();
                    stockAdjusmentAll.Date = DateTime.Now;
                    stockAdjusmentAll.Name = "ST";
                    stockAdjusmentAll.Reason = "STK";
                    stockAdjusmentAll.LocationId = Locations;
                    stockAdjusmentAll.StockTakesId = T.id;
                    int id =await _adjusment.Insert(stockAdjusmentAll,CompanyId);
                    foreach (var item in degerler)
                    {
                        var Discrepancys = item.CountedQuantity - item.InStock;
                        _db.Execute($"Update StockTakesItem Set Discrepancy={Discrepancys} where id = {item.id} and CompanyId = {CompanyId} ");
                        if (Discrepancys == null)
                        {

                        }
                        else
                        {
                            StockAdjusmentInsertItem A = new StockAdjusmentInsertItem();
                            A.ItemId = item.ItemId;
                            A.LocationId = Locations;
                            A.Adjusment = (float)item.Discrepancy;
                           
                          await _adjusment.InsertItem(A, id, CompanyId,UserId);
                        }


                    }
                   await _db.QueryAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId} ");

                }
                else
                {
                 await  _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId}");
                }

            }
            else if (eskiStatus == 2)
            {
                if (T.Status == 3)
                {
                    prm.Add("@id", T.id);
                    prm.Add("@CompanyId", CompanyId);
                    prm.Add("@Status", T.Status);
                    await   _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId} ");
                    string sqlsorgu = $@"  select ItemId,Discrepancy,Min(Discrepancy) as [Control] from StockTakesItem where StockTakesItem.CompanyId={CompanyId} and StockTakesItem.StockTakesId={T.id}  Group by ItemId,Discrepancy";
                    var degerler =await _db.QueryAsync<StockTakeItems>(sqlsorgu);
                    string mindiscrepanyc = $@"    select Min(Discrepancy) as Discrepancy  from StockTakesItem where StockTakesItem.CompanyId={CompanyId} and StockTakesItem.StockTakesId={T.id} ";
                    var discrepancy =await _db.QueryAsync<StockTakeItems>(mindiscrepanyc);

                    if (discrepancy.First().Discrepancy != null)
                    {
                        StockAdjusmentInsert stockAdjusmentAll = new StockAdjusmentInsert();
                        stockAdjusmentAll.Date = DateTime.Now;
                        stockAdjusmentAll.Name = "ST";
                        stockAdjusmentAll.Reason = "STK";
                        stockAdjusmentAll.LocationId = Locations;
                        stockAdjusmentAll.StockTakesId = T.id;


                        int id =await _adjusment.Insert(stockAdjusmentAll, CompanyId);
                        foreach (var item in degerler)
                        {
                            if (item.Discrepancy == null)
                            {

                            }
                            else
                            {
                                StockAdjusmentInsertItem A = new StockAdjusmentInsertItem();
                                A.ItemId = item.ItemId;
                                A.LocationId = Locations;
                                A.Adjusment = (float)item.Discrepancy;
                               await _adjusment.InsertItem(A, id, CompanyId,UserId);
                            }

                        }
                    }

                    else
                    {
                     await   _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId} ");
                    }

                }
                else
                {
                  await  _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId} ");
                }

            }
            else if (eskiStatus == 3)
            {
                if (T.Status == 3)
                {
                   await _db.ExecuteAsync($"Update StockTakes Set Status={T.Status} where id = {T.id} and CompanyId = {CompanyId}");
                }
                else
                {
                    string sqlsorgu = $@"select id from StockAdjusment where StockTakesId={T.id} and CompanyId={CompanyId}";
                    var adjusmentid =await _db.QueryAsync(sqlsorgu);
                    if (adjusmentid.First() != 0)
                    {
                        IdControl delete = new IdControl();
                        delete.id = adjusmentid.First();
                       await _adjusment.Delete(delete, CompanyId,UserId);
                    }

                }

            }
        }

        public async Task<IEnumerable<StockTakeList>> StockTakesList(StockTakeList T, int CompanyId, int KAYITSAYISI, int SAYFA)
        {
            DynamicParameters param = new DynamicParameters();
            param.Add("@CompanyId", CompanyId);

            string sql = $@"DECLARE @KAYITSAYISI int DECLARE @SAYFA int SET @KAYITSAYISI ={KAYITSAYISI} SET @SAYFA = {SAYFA}
            select x.* from (
            select StockTakes.id,StockTake,StockTakes.Reason,StockTakes.LocationId,Locations.LocationName,CreatedDate,CompletedDate,StockAdjusmentId,StockAdjusment.Name,Status from StockTakes
            left join Locations on Locations.id=StockTakes.LocationId
			left join StockAdjusment on StockAdjusment.id=StockTakes.StockAdjusmentId
            where StockTakes.CompanyId=@CompanyId and StockTakes.IsActive=1 and StockTakes.Status!=3 AND ISNULL(StockTake,'') like '%{T.StockTake}%' 
			and ISNULL(StockTakes.Reason,'') Like '%{T.Reason}%' AND    ISNULL(LocationName,'') like '%{T.LocationName}%' and
        ISNULL(Name,'') like '%{T.Name}%' and ISNULL(StockTakes.Status,'') like '%{T.Status}%'
		Group by StockTake,StockTakes.Reason,StockTakes.LocationId,StockTakes.id,Locations.LocationName,CreatedDate,CompletedDate,StockAdjusmentId,StockAdjusment.Name,Status)x
		  ORDER BY x.id OFFSET @KAYITSAYISI * (@SAYFA - 1) ROWS FETCH NEXT @KAYITSAYISI ROWS ONLY";
            var List =await _db.QueryAsync<StockTakeList>(sql, param);

            return List.ToList();
        }

        public async Task Update(StockTakesUpdate T, int id, int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.id);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@StockTake", T.StockTake);
            prm.Add("@CreatedDate", T.CreadtedDate);
            prm.Add("@StartedDate", T.StartedDate);
            prm.Add("@CompletedDate", T.CompletedDate);
            prm.Add("@Reason", T.Reason);
            prm.Add("@Info", T.Info);
            string sql = $@"select Status from StockTakes where CompanyId=@CompanyId and id=@id ";
            var Status =await _db.QueryFirstAsync<int>(sql, prm);
            if (Status == 0)
            {
              await  _db.ExecuteAsync($"Update StockTakes set Info=@Info,Reason=@Reason,CreatedDate=@CreatedDate,StockTake=@StockTake where CompanyId=@CompanyId and id=@id ", prm);

            }
            else if (Status==1)
            {
                await _db.ExecuteAsync($"Update StockTakes set Info=@Info,Reason=@Reason,CreatedDate=@CreatedDate,StockTake=@StockTake,StartedDate=@StartedDate where CompanyId=@CompanyId and id=@id", prm);
            }
            else if (Status==2)
            {
                await _db.ExecuteAsync($"Update StockTakes set Info=@Info,Reason=@Reason,CreatedDate=@CreatedDate,StockTake=@StockTake,StartedDate=@StartedDate,CompletedDate=@CompletedDate where CompanyId=@CompanyId and id=@id", prm);
            }
            else if (Status==3)
            {
                await _db.ExecuteAsync($"Update StockTakes set Info=@Info,Reason=@Reason,CreadtedDate=@CreadtedDate,StockTake=@StockTake,StartedDate=@StartedDate,CompletedDate=@CompletedDate where CompanyId=@CompanyId and StockTakesId=@id", prm);
            }
        }
        public async Task UpdateItems(StockTakesUpdateItems T , int CompanyId)
        {
            DynamicParameters prm = new DynamicParameters();
            prm.Add("@id", T.StockTakesId);
            prm.Add("@CompanyId", CompanyId);
            prm.Add("@StockTakesItemId", T.StockTakesItemId);
            prm.Add("@CountedQuantity", T.CountedQuantity);
            prm.Add("@Note", T.Note);

            string sql = $@"select Status from StockTakes where CompanyId=@CompanyId and id=@id ";
            var Status = await _db.QueryFirstAsync<int>(sql, prm);
            if (Status == 0)
            {
                await _db.ExecuteAsync($"Update StockTakesItem set Note=@Note where CompanyId=@CompanyId and StockTakesId=@id and id=@StockTakesItemId", prm);

            }
            else if (Status == 1)
            {
                await _db.ExecuteAsync($"Update StockTakesItem set Note=@Note,CountedQuantity=@CountedQuantity where CompanyId=@CompanyId and StockTakesId=@id and id=@StockTakesItemId", prm);
            }
        }
    }
}
