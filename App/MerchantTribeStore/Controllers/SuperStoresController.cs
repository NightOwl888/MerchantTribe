using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Accounts;
using System.Text;
using MerchantTribeStore.Models;

namespace MerchantTribeStore.Controllers
{
    public class SuperStoresController : Shared.BaseSuperController
    {
        //
        // GET: /Super/Stores/

        public ActionResult Index(int pageNumber=1, int pageSize=100)
        {
            if (pageNumber < 1) pageNumber = 1;

            int allCount = MTApp.AccountServices.Stores.CountOfAll();
            MTApp.AccountServices.Stores.FindAllPaged(pageNumber, pageSize);

            StoreListViewModel model = new StoreListViewModel();

            foreach (Store s in MTApp.AccountServices.Stores.FindAllPaged(pageNumber, pageSize))
            {
                SuperStoreViewModel m = new SuperStoreViewModel(s);
                m.Users = MTApp.AccountServices.FindAdminUsersByStoreId(s.Id);
                model.Stores.Add(m);
            }           

            model.PagerData.CurrentPage = pageNumber;
            model.PagerData.PageSize = pageSize;
            model.PagerData.TotalItems = allCount;            
            model.PagerData.PagerUrlFormat = Url.Content("~/super/stores/?pageNumber={0}&pageSize=" + pageSize);
            model.PagerData.PagerUrlFormatFirst = Url.Content("~/super/stores/?pageNumber=1&pageSize=" + pageSize);
            
            return View(model);
        }

        // Get: /Super/Stores/NewStoreReport
        public ActionResult NewStoreReport()
        {
            List<Store> stores = MTApp.AccountServices.Stores.FindStoresCreatedAfterDateForSuper(DateTime.UtcNow.AddDays(-30));

            List<Models.SuperStoreViewModel> viewmodel = new List<Models.SuperStoreViewModel>();
            foreach (Store s in stores)
            {
                SuperStoreViewModel m = new SuperStoreViewModel(s);
                m.Users = MTApp.AccountServices.FindAdminUsersByStoreId(s.Id);
                viewmodel.Add(m);
            }

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Destroy(long id)
        {
            //MTApp.DestroyStore(id);
            return new RawResult("Complete", "text/html");
        }

                                 
    }
}
