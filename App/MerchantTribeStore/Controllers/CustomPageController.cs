﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MerchantTribeStore.Controllers.Shared;
using MerchantTribe.Commerce;
using MerchantTribe.Commerce.Catalog;
using MerchantTribe.Commerce.Content;
using MerchantTribe.Commerce.Utilities;
using MerchantTribeStore.code.TemplateEngine;
using System.Text;

namespace MerchantTribeStore.Controllers
{
    public class CustomPageController : BaseStoreController
    {
        //
        // GET: /CustomPage/
        public ActionResult Index(string slug)
        {
            Category cat = MTApp.CatalogServices.Categories.FindBySlugForStore(slug, 
                                        MTApp.CurrentRequestContext.CurrentStore.Id);
            if (cat == null) cat = new Category();
            MTApp.CurrentRequestContext.CurrentCategory = cat;
            
            // Record View for Analytics
            RecordCategoryView(cat.Bvin);
            
            // Get page.html Template
            ThemeManager tm = MTApp.ThemeManager();
            if (cat.TemplateName == string.Empty) { cat.TemplateName = "default.html"; }
            string template = tm.GetTemplateFromCurrentTheme(cat.TemplateName, "default.html"); // Try default in theme before system

            // Fill with data from category, making sure legacy description is used if no area data
            CategoryPageVersion version = cat.GetCurrentVersion();
            if (version.Id == 0)
            {
                // Create Initial Version
                version.PublishedStatus = PublishStatus.Draft;
                version.PageId = cat.Bvin;
                cat.Versions.Add(version);
                MTApp.CatalogServices.Categories.Update(cat);
                version = cat.GetCurrentVersion();
            }
            if (!version.Areas.HasArea("main"))
            {
                version.Areas.SetAreaContent("main", cat.PreTransformDescription);
            }

            ITagProvider tagProvider = new TagProvider();
            Processor proc = new Processor(this.MTApp, template, tagProvider);

            var model = proc.RenderForDisplay();
            return View("~/views/shared/templateengine.cshtml", model);                                    
        }

        private void RecordCategoryView(string bvin)
        {
            MerchantTribe.Commerce.SessionManager.CategoryLastId = bvin;
        }
        
    }
}
